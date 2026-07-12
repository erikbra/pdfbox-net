from __future__ import annotations

import hashlib
import importlib.util
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
MODULE_PATH = ROOT / "tools/conversion_quality/run_remote_corpus.py"
SPEC = importlib.util.spec_from_file_location("remote_corpus", MODULE_PATH)
assert SPEC and SPEC.loader
remote_corpus = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = remote_corpus
SPEC.loader.exec_module(remote_corpus)


class Response(io.BytesIO):
    def __init__(self, content: bytes, url: str = "https://example.test/sample.pdf"):
        super().__init__(content)
        self._url = url

    def geturl(self):
        return self._url

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self.close()


class RemoteCorpusTest(unittest.TestCase):
    def test_checked_in_manifest_has_pinned_https_documents(self) -> None:
        description, documents = remote_corpus.load_manifest(remote_corpus.DEFAULT_MANIFEST)

        self.assertIn("academic", description)
        self.assertEqual(["jmlr-lda", "acl-bert", "arxiv-adam", "arxiv-unet"], [item.id for item in documents])
        self.assertTrue(all(item.pdf_url.startswith("https://") for item in documents))
        self.assertTrue(all(len(item.sha256) == 64 for item in documents))
        self.assertEqual(
            {
                "jmlr-lda": (
                    "https://www.jmlr.org/papers/volume3/blei03a/blei03a.pdf",
                    "4667de63545b57d55d6c43e5af6f3429edfaac9472ed9eff68fdf43572735dd9",
                ),
                "acl-bert": (
                    "https://aclanthology.org/N19-1423.pdf",
                    "987545ffb087f1ece898142c403a516baeabeb70ce19089397fac6f7db12c3d4",
                ),
                "arxiv-adam": (
                    "https://arxiv.org/pdf/1412.6980",
                    "eab9c73ae2ceda884b94830bda99312254bac4806f6c9f045cbab90721ecda31",
                ),
                "arxiv-unet": (
                    "https://arxiv.org/pdf/1505.04597",
                    "a3172b2124f38e260dc2c7ed968d87c31bc94dbc19a42a7ab3dcbd7534319c44",
                ),
            },
            {item.id: (item.pdf_url, item.sha256) for item in documents},
        )

    def test_manifest_rejects_non_https_and_duplicate_ids(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            manifest = Path(temp_dir) / "manifest.json"
            entry = self._entry("sample")
            entry["pdfUrl"] = "http://example.test/sample.pdf"
            self._write_manifest(manifest, [entry])
            with self.assertRaisesRegex(ValueError, "HTTPS"):
                remote_corpus.load_manifest(manifest)

            entry["pdfUrl"] = "https://example.test/sample.pdf"
            self._write_manifest(manifest, [entry, dict(entry)])
            with self.assertRaisesRegex(ValueError, "duplicate"):
                remote_corpus.load_manifest(manifest)

    def test_manifest_rejects_invalid_hash_and_expectations(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            manifest = Path(temp_dir) / "manifest.json"
            entry = self._entry("sample")
            entry["sha256"] = "not-a-hash"
            self._write_manifest(manifest, [entry])
            with self.assertRaisesRegex(ValueError, "sha256"):
                remote_corpus.load_manifest(manifest)

            entry["sha256"] = "0" * 64
            entry["expectations"]["requiredText"] = []
            self._write_manifest(manifest, [entry])
            with self.assertRaisesRegex(ValueError, "requiredText"):
                remote_corpus.load_manifest(manifest)

    def test_fetch_document_retries_verifies_hash_and_installs_atomically(self) -> None:
        content = b"pinned-pdf-content"
        expected_hash = hashlib.sha256(content).hexdigest()
        document = self._document(expected_hash)
        attempts = 0

        def open_url(request, *, timeout):
            nonlocal attempts
            attempts += 1
            self.assertEqual("https://example.test/sample.pdf", request.full_url)
            self.assertEqual(7, timeout)
            if attempts == 1:
                raise OSError("transient")
            return Response(content)

        with tempfile.TemporaryDirectory() as temp_dir:
            cache_dir = Path(temp_dir)
            target = remote_corpus.fetch_document(
                document,
                cache_dir,
                retries=2,
                timeout_seconds=7,
                open_url=open_url,
                sleep=lambda _: None,
            )

            self.assertEqual(content, target.read_bytes())
            self.assertEqual(2, attempts)
            self.assertEqual([], list(cache_dir.glob("*.tmp")))

            cached = remote_corpus.fetch_document(
                document,
                cache_dir,
                open_url=lambda *args, **kwargs: self.fail("verified cache should not be downloaded"),
            )
            self.assertEqual(target, cached)

    def test_fetch_document_rejects_hash_mismatch_without_installing_pdf(self) -> None:
        document = self._document("0" * 64)
        with tempfile.TemporaryDirectory() as temp_dir:
            cache_dir = Path(temp_dir)
            with self.assertRaisesRegex(RuntimeError, "SHA-256 mismatch"):
                remote_corpus.fetch_document(
                    document,
                    cache_dir,
                    retries=2,
                    open_url=lambda *args, **kwargs: Response(b"wrong"),
                    sleep=lambda _: None,
                )

            self.assertFalse((cache_dir / "sample.pdf").exists())
            self.assertEqual([], list(cache_dir.iterdir()))

    def test_fetch_document_rejects_redirect_to_non_https_url(self) -> None:
        content = b"pinned-pdf-content"
        document = self._document(hashlib.sha256(content).hexdigest())
        with tempfile.TemporaryDirectory() as temp_dir:
            with self.assertRaisesRegex(RuntimeError, "HTTPS"):
                remote_corpus.fetch_document(
                    document,
                    Path(temp_dir),
                    retries=1,
                    open_url=lambda *args, **kwargs: Response(content, "http://example.test/sample.pdf"),
                )

    def test_materialize_review_manifest_uses_relative_cached_pdf_and_expectations(self) -> None:
        document = self._document("0" * 64)
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            pdf = root / "cache/sample.pdf"
            pdf.parent.mkdir()
            pdf.write_bytes(b"pdf")
            output = root / "generated/review-manifest.json"

            remote_corpus.materialize_review_manifest("Remote corpus.", [document], {document.id: pdf}, output)

            data = json.loads(output.read_text(encoding="utf-8"))
            example = data["examples"][0]
            self.assertEqual("../cache/sample.pdf", example["sourcePdf"])
            self.assertEqual(1, example["expectations"]["pageCount"])
            self.assertIn(document.source_page, example["notes"])

    @staticmethod
    def _entry(document_id: str) -> dict:
        return {
            "id": document_id,
            "title": "Sample",
            "sourcePage": "https://example.test/sample",
            "pdfUrl": "https://example.test/sample.pdf",
            "sha256": "0" * 64,
            "categories": ["text-heavy"],
            "qualityPages": 1,
            "notes": "Sample document.",
            "expectations": {"pageCount": 1, "requiredText": ["sample"], "minTextRuns": 1},
        }

    @classmethod
    def _document(cls, sha256: str):
        entry = cls._entry("sample")
        entry["sha256"] = sha256
        with tempfile.TemporaryDirectory() as temp_dir:
            manifest = Path(temp_dir) / "manifest.json"
            cls._write_manifest(manifest, [entry])
            return remote_corpus.load_manifest(manifest)[1][0]

    @staticmethod
    def _write_manifest(path: Path, documents: list[dict]) -> None:
        path.write_text(json.dumps({"schema": 1, "description": "Academic remote corpus.", "documents": documents}))


if __name__ == "__main__":
    unittest.main()
