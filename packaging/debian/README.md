# Debian and APT distribution

The repository builder consumes the immutable unpdf release manifest, verifies
both Linux archives, builds `amd64` and `arm64` packages, generates architecture
indices, and signs `InRelease` and `Release.gpg`:

```console
python3 eng/build_unpdf_apt_repository.py \
  --manifest https://github.com/erikbra/pdfbox-net/releases/download/unpdf-v4.0.0-preview.1/release-manifest.json \
  --output artifacts/apt-repository \
  --suite preview \
  --gpg-key <fingerprint>
```

GitHub Pages is the selected static host because the repository is immutable,
cacheable content and the project already uses GitHub Actions/Releases. A
production publish workflow requires a persistent private signing key stored in
a protected GitHub environment; pull-request tests use an ephemeral key that is
discarded with the runner.

APT authenticates the signed repository metadata, which contains SHA-256 hashes
for every `.deb`. Individual Debian archives are not separately `dpkg-sig`
signed; this follows normal APT repository trust semantics.
