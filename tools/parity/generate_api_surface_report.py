#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Iterable


CORE_MODULES = ("io", "fontbox", "xmpbox", "pdfbox")
CS_PROJECTS = (
    "src/PdfBox.Net.IO/PdfBox.Net.IO.csproj",
    "src/PdfBox.Net.FontBox/PdfBox.Net.FontBox.csproj",
    "src/PdfBox.Net.XmpBox/PdfBox.Net.XmpBox.csproj",
    "src/PdfBox.Net/PdfBox.Net.csproj",
)
CS_ROOTS = (
    "src/PdfBox.Net.IO",
    "src/PdfBox.Net.FontBox",
    "src/PdfBox.Net.XmpBox",
    "src/PdfBox.Net",
)
REPORT_JSON = Path("reports/api-surface-comparison.json")
REPORT_MD = Path("reports/pdfbox-api-surface-analysis.md")
SOURCE_PATH_RE = re.compile(r"PDFBOX_SOURCE_PATH:\s*(.+?)\s*$")
TYPE_DECL_RE = re.compile(
    r"\b(?:(?:public|protected|internal|private|abstract|sealed|static|partial|readonly|unsafe|new)\s+)*"
    r"(?P<kind>class|interface|struct|enum|record(?:\s+class|\s+struct)?)\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)"
)
JAVA_TYPE_RE = re.compile(
    r"(?P<prefix>(?:(?:@[A-Za-z_][A-Za-z0-9_.]*(?:\s*\([^)]*\))?)|"
    r"(?:public|protected|private|abstract|static|final|sealed|non-sealed|strictfp))\s+)*"
    r"(?P<kind>class|interface|enum|@interface)\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)\b"
)
JAVA_MODIFIERS = {
    "public",
    "protected",
    "private",
    "abstract",
    "static",
    "final",
    "sealed",
    "non-sealed",
    "strictfp",
    "default",
    "synchronized",
    "native",
    "transient",
    "volatile",
}


def utc_now_iso() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z")


def run(args: list[str], cwd: Path | None = None, capture: bool = False) -> subprocess.CompletedProcess:
    completed = subprocess.run(
        args,
        cwd=str(cwd) if cwd else None,
        check=False,
        encoding="utf-8",
        stdout=subprocess.PIPE if capture else None,
        stderr=subprocess.STDOUT if capture else None,
    )
    if completed.returncode != 0:
        if capture and completed.stdout:
            sys.stderr.write(completed.stdout)
        completed.check_returncode()
    return completed


def git_head(path: Path) -> str:
    return run(["git", "rev-parse", "HEAD"], cwd=path, capture=True).stdout.strip()


def module_and_family(source_path: str) -> tuple[str, str]:
    module = source_path.split("/", 1)[0]
    marker = "/src/main/java/"
    if marker not in source_path:
        return module, "(unknown)"

    package_parts = source_path.split(marker, 1)[1].split("/")
    if len(package_parts) < 5:
        return module, "(root)"
    root = package_parts[2]
    if root in {"pdfbox", "fontbox", "xmpbox"}:
        return module, package_parts[3]
    return module, root


def strip_comments_and_literals(text: str) -> str:
    chars = list(text)
    i = 0
    state = "normal"
    quote = ""
    while i < len(chars):
        c = chars[i]
        nxt = chars[i + 1] if i + 1 < len(chars) else ""
        if state == "normal":
            if c == "/" and nxt == "/":
                chars[i] = chars[i + 1] = " "
                i += 2
                state = "line"
                continue
            if c == "/" and nxt == "*":
                chars[i] = chars[i + 1] = " "
                i += 2
                state = "block"
                continue
            if c in {"\"", "'"}:
                quote = c
                chars[i] = " "
                i += 1
                state = "string"
                continue
        elif state == "line":
            if c == "\n":
                state = "normal"
            else:
                chars[i] = " "
        elif state == "block":
            if c == "*" and nxt == "/":
                chars[i] = chars[i + 1] = " "
                i += 2
                state = "normal"
                continue
            if c != "\n":
                chars[i] = " "
        elif state == "string":
            if c == "\\":
                if c != "\n":
                    chars[i] = " "
                if i + 1 < len(chars) and chars[i + 1] != "\n":
                    chars[i + 1] = " "
                i += 2
                continue
            if c == quote:
                chars[i] = " "
                state = "normal"
            elif c != "\n":
                chars[i] = " "
        i += 1
    return "".join(chars)


def find_matching_brace(text: str, open_index: int) -> int:
    depth = 0
    for i in range(open_index, len(text)):
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
            if depth == 0:
                return i
    return -1


def collapse_ws(text: str) -> str:
    return re.sub(r"\s+", " ", text).strip()


def modifier_words(text: str) -> set[str]:
    return {w for w in re.findall(r"\b[A-Za-z_-]+\b", text) if w in JAVA_MODIFIERS}


def split_top_level(text: str, delimiter: str = ",") -> list[str]:
    parts: list[str] = []
    start = 0
    angle = paren = bracket = brace = 0
    for i, c in enumerate(text):
        if c == "<":
            angle += 1
        elif c == ">" and angle:
            angle -= 1
        elif c == "(":
            paren += 1
        elif c == ")" and paren:
            paren -= 1
        elif c == "[":
            bracket += 1
        elif c == "]" and bracket:
            bracket -= 1
        elif c == "{":
            brace += 1
        elif c == "}" and brace:
            brace -= 1
        elif c == delimiter and not angle and not paren and not bracket and not brace:
            parts.append(text[start:i].strip())
            start = i + 1
    tail = text[start:].strip()
    if tail:
        parts.append(tail)
    return parts


def parameter_count(params: str) -> int:
    params = params.strip()
    if not params:
        return 0
    return len([p for p in split_top_level(params) if p.strip()])


def field_names(statement: str) -> list[str]:
    left = statement.split("=", 1)[0]
    names: list[str] = []
    for part in split_top_level(left):
        candidate = part.split("=")[0].strip()
        m = re.search(r"([A-Za-z_][A-Za-z0-9_]*)\s*(?:\[\s*\])?$", candidate)
        if m:
            name = m.group(1)
            if name not in JAVA_MODIFIERS:
                names.append(name)
    return names


def iter_top_level_declarations(body: str) -> Iterable[tuple[str, str]]:
    start = 0
    paren = bracket = angle = 0
    i = 0
    while i < len(body):
        c = body[i]
        if c == "<":
            angle += 1
        elif c == ">" and angle:
            angle -= 1
        elif c == "(":
            paren += 1
        elif c == ")" and paren:
            paren -= 1
        elif c == "[":
            bracket += 1
        elif c == "]" and bracket:
            bracket -= 1
        elif c == ";" and not paren and not bracket:
            yield body[start:i], ";"
            start = i + 1
        elif c == "{" and not paren and not bracket:
            header = body[start:i]
            end = find_matching_brace(body, i)
            if end < 0:
                break
            yield header, "{"
            i = end
            start = end + 1
        i += 1


def java_visibility(mods: set[str], parent_kind: str, type_kind: str, top_level: bool) -> str | None:
    if "private" in mods:
        return None
    if "public" in mods or (parent_kind == "interface" and not top_level):
        return "public"
    if "protected" in mods:
        return "protected"
    return None


def extract_java_members(body: str, type_name: str, type_kind: str) -> list[dict]:
    members: list[dict] = []
    if type_kind == "enum":
        for chunk, terminator in iter_top_level_declarations(body):
            constants_part = chunk
            if not constants_part.strip():
                continue
            if re.search(r"\b(class|interface|enum|@interface)\b", constants_part):
                continue
            for value in split_top_level(constants_part):
                m = re.match(r"(?:@[A-Za-z_][A-Za-z0-9_.]*(?:\s*\([^)]*\))?\s+)*([A-Za-z_][A-Za-z0-9_]*)", value.strip())
                if m:
                    members.append(
                        {
                            "kind": "enum-value",
                            "name": m.group(1),
                            "arity": 0,
                            "visibility": "public",
                            "static": True,
                            "signature": m.group(1),
                        }
                    )
            if terminator == ";":
                break

    for header, terminator in iter_top_level_declarations(body):
        header = collapse_ws(re.sub(r"@[A-Za-z_][A-Za-z0-9_.]*(?:\s*\([^)]*\))?", " ", header))
        if not header:
            continue
        if re.search(r"\b(class|interface|enum|@interface)\b", header):
            continue
        mods = modifier_words(header)
        visible = java_visibility(mods, parent_kind="", type_kind=type_kind, top_level=False)
        if not visible and type_kind in {"interface", "@interface"} and "private" not in mods:
            visible = "public"
        if not visible:
            continue

        if "(" in header and ")" in header:
            m = re.search(r"([A-Za-z_][A-Za-z0-9_]*)\s*\(([^()]*)\)\s*(?:throws\b.*)?$", header)
            if not m:
                continue
            name = m.group(1)
            if name in {"if", "for", "while", "switch", "catch", "try", "synchronized"}:
                continue
            kind = "constructor" if name == type_name else "method"
            members.append(
                {
                    "kind": kind,
                    "name": name,
                    "arity": parameter_count(m.group(2)),
                    "visibility": visible,
                    "static": "static" in mods,
                    "signature": header,
                }
            )
            continue

        if terminator == ";":
            for name in field_names(header):
                members.append(
                    {
                        "kind": "field",
                        "name": name,
                        "arity": 0,
                        "visibility": visible,
                        "static": "static" in mods or type_kind in {"interface", "@interface"},
                        "signature": header,
                    }
                )
    return members


def extract_java_api(java_file: Path, upstream_root: Path) -> list[dict]:
    source_path = java_file.relative_to(upstream_root).as_posix()
    module, family = module_and_family(source_path)
    text = java_file.read_text(encoding="utf-8")
    clean = strip_comments_and_literals(text)
    pkg_match = re.search(r"\bpackage\s+([A-Za-z_][A-Za-z0-9_.]*)\s*;", clean)
    package = pkg_match.group(1) if pkg_match else ""
    types: list[dict] = []
    seen_bodies: list[tuple[int, int]] = []

    def inside_seen(pos: int) -> bool:
        return any(start <= pos <= end for start, end in seen_bodies)

    def parse_region(start: int, end: int, parents: list[str], parent_kind: str, top_level: bool) -> None:
        pos = start
        while True:
            m = JAVA_TYPE_RE.search(clean, pos, end)
            if not m:
                break
            if inside_seen(m.start()):
                pos = m.end()
                continue
            brace = clean.find("{", m.end(), end)
            if brace < 0:
                pos = m.end()
                continue
            body_end = find_matching_brace(clean, brace)
            if body_end < 0 or body_end > end:
                pos = m.end()
                continue
            mods = modifier_words(m.group("prefix") or "")
            visibility = java_visibility(mods, parent_kind, m.group("kind"), top_level)
            name = m.group("name")
            type_kind = m.group("kind").replace("@interface", "annotation")
            body = clean[brace + 1 : body_end]
            nested_name = ".".join(parents + [name])
            if visibility:
                members = extract_java_members(body, name, m.group("kind"))
                types.append(
                    {
                        "source_path": source_path,
                        "module": module,
                        "family": family,
                        "package": package,
                        "name": name,
                        "nested_name": nested_name,
                        "full_name": f"{package}.{nested_name}" if package else nested_name,
                        "kind": type_kind,
                        "visibility": visibility,
                        "members": members,
                    }
                )
            seen_bodies.append((brace, body_end))
            parse_region(brace + 1, body_end, parents + [name], m.group("kind"), top_level=False)
            pos = body_end + 1

    parse_region(0, len(clean), [], "", top_level=True)
    return types


def collect_java_api(upstream_root: Path) -> list[dict]:
    result: list[dict] = []
    for module in CORE_MODULES:
        module_root = upstream_root / module / "src/main/java"
        if not module_root.exists():
            continue
        for java_file in sorted(module_root.rglob("*.java")):
            result.extend(extract_java_api(java_file, upstream_root))
    return result


def collect_csharp_source_map(repo_root: Path) -> dict:
    by_source: dict[str, list[dict]] = defaultdict(list)
    by_type: dict[str, list[dict]] = defaultdict(list)
    for root_name in CS_ROOTS:
        root = repo_root / root_name
        for cs_file in sorted(root.rglob("*.cs")):
            rel = cs_file.relative_to(repo_root).as_posix()
            parts = set(cs_file.parts)
            if "bin" in parts or "obj" in parts:
                continue
            text = cs_file.read_text(encoding="utf-8", errors="ignore")
            source = None
            for line in text.splitlines()[:80]:
                m = SOURCE_PATH_RE.search(line)
                if m:
                    source = m.group(1).strip()
                    break
            if not source:
                continue
            scan_text = strip_comments_and_literals(text)
            ns_match = re.search(r"\bnamespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*;", text)
            namespace = ns_match.group(1) if ns_match else ""
            for m in TYPE_DECL_RE.finditer(scan_text):
                entry = {
                    "source_path": source,
                    "target_path": rel,
                    "namespace": namespace,
                    "name": m.group("name"),
                    "kind": m.group("kind"),
                }
                by_source[source].append(entry)
                by_type[m.group("name")].append(entry)
    return {"by_source": by_source, "by_type": by_type}


REFLECTOR_PROGRAM = r'''
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

var searchDirs = args.Select(Path.GetDirectoryName).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToArray();
var nugetRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".nuget",
    "packages");
AppDomain.CurrentDomain.AssemblyResolve += (_, resolveArgs) =>
{
    var name = new AssemblyName(resolveArgs.Name).Name + ".dll";
    foreach (var dir in searchDirs)
    {
        var candidate = Path.Combine(dir!, name);
        if (File.Exists(candidate))
        {
            return Assembly.LoadFrom(candidate);
        }
    }
    if (Directory.Exists(nugetRoot))
    {
        var packageName = Path.GetFileNameWithoutExtension(name).ToLowerInvariant();
        var packageDir = Path.Combine(nugetRoot, packageName);
        if (Directory.Exists(packageDir))
        {
            var candidate = Directory.EnumerateFiles(packageDir, name, SearchOption.AllDirectories)
                .Where(p => p.Contains($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}"))
                .OrderByDescending(p => p.Contains($"{Path.DirectorySeparatorChar}net8.0{Path.DirectorySeparatorChar}"))
                .ThenByDescending(p => p.Contains($"{Path.DirectorySeparatorChar}netstandard2.1{Path.DirectorySeparatorChar}"))
                .ThenByDescending(p => p.Contains($"{Path.DirectorySeparatorChar}netstandard2.0{Path.DirectorySeparatorChar}"))
                .FirstOrDefault();
            if (candidate != null)
            {
                return Assembly.LoadFrom(candidate);
            }
        }
    }
    return null;
};

static bool ApiType(Type t) => t.IsPublic || t.IsNestedPublic || t.IsNestedFamily || t.IsNestedFamORAssem;
static bool ApiMember(MethodBase m) => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
static string MethodVisibility(MethodBase m) => m.IsPublic ? "public" : "protected";
static string FieldVisibility(FieldInfo f) => f.IsPublic ? "public" : "protected";
static bool CompilerGenerated(MemberInfo m) => m.GetCustomAttribute<CompilerGeneratedAttribute>() != null;

var types = new List<object>();
foreach (var assemblyPath in args)
{
    var assembly = Assembly.LoadFrom(Path.GetFullPath(assemblyPath));
    foreach (var t in assembly.GetTypes().Where(ApiType).OrderBy(t => t.FullName))
    {
        if (CompilerGenerated(t) || t.Name.Contains("<"))
        {
            continue;
        }
        var members = new List<object>();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var ctor in t.GetConstructors(flags).Where(ApiMember))
        {
            members.Add(new {
                kind = "constructor",
                name = ".ctor",
                arity = ctor.GetParameters().Length,
                visibility = MethodVisibility(ctor),
                isStatic = ctor.IsStatic
            });
        }
        foreach (var method in t.GetMethods(flags).Where(ApiMember))
        {
            if ((method.IsSpecialName && !method.Name.StartsWith("op_", StringComparison.Ordinal)) ||
                CompilerGenerated(method) ||
                method.Name.Contains("<"))
            {
                continue;
            }
            members.Add(new {
                kind = "method",
                name = method.Name,
                arity = method.GetParameters().Length,
                visibility = MethodVisibility(method),
                isStatic = method.IsStatic
            });
        }
        foreach (var prop in t.GetProperties(flags))
        {
            var accessor = new[] { prop.GetMethod, prop.SetMethod }.FirstOrDefault(m => m != null && ApiMember(m));
            if (accessor == null || CompilerGenerated(prop))
            {
                continue;
            }
            var arity = prop.GetIndexParameters().Length;
            members.Add(new {
                kind = "property",
                name = prop.Name,
                arity,
                visibility = MethodVisibility(accessor),
                isStatic = accessor.IsStatic
            });
        }
        foreach (var field in t.GetFields(flags).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
        {
            if (CompilerGenerated(field) || field.Name.Contains("<"))
            {
                continue;
            }
            members.Add(new {
                kind = t.IsEnum && field.IsLiteral ? "enum-value" : "field",
                name = field.Name,
                arity = 0,
                visibility = FieldVisibility(field),
                isStatic = field.IsStatic
            });
        }
        foreach (var ev in t.GetEvents(flags))
        {
            var accessor = ev.AddMethod ?? ev.RemoveMethod;
            if (accessor != null && ApiMember(accessor))
            {
                members.Add(new {
                    kind = "event",
                    name = ev.Name,
                    arity = 0,
                    visibility = MethodVisibility(accessor),
                    isStatic = accessor.IsStatic
                });
            }
        }
        types.Add(new {
            assembly = assembly.GetName().Name,
            fullName = (t.FullName ?? t.Name).Split('`')[0],
            name = t.Name.Split('`')[0],
            nestedName = (t.FullName ?? t.Name).Split('.').Last().Replace("+", ".").Split('`')[0],
            kind = t.IsEnum ? "enum" : t.IsInterface ? "interface" : t.IsValueType ? "struct" : "class",
            visibility = t.IsPublic || t.IsNestedPublic ? "public" : "protected",
            members
        });
    }
}

Console.WriteLine(JsonSerializer.Serialize(new { types }, new JsonSerializerOptions { WriteIndented = false }));
'''


def build_core_projects(repo_root: Path, configuration: str) -> None:
    for project in CS_PROJECTS:
        run(["dotnet", "build", project, "--configuration", configuration, "--no-restore"], cwd=repo_root)


def reflect_csharp_api(repo_root: Path, configuration: str) -> list[dict]:
    assembly_paths = [
        repo_root / "src/PdfBox.Net.IO/bin" / configuration / "net10.0/PdfBox.Net.IO.dll",
        repo_root / "src/PdfBox.Net.FontBox/bin" / configuration / "net10.0/PdfBox.Net.FontBox.dll",
        repo_root / "src/PdfBox.Net.XmpBox/bin" / configuration / "net10.0/PdfBox.Net.XmpBox.dll",
        repo_root / "src/PdfBox.Net/bin" / configuration / "net10.0/PdfBox.Net.dll",
    ]
    missing = [str(p) for p in assembly_paths if not p.exists()]
    if missing:
        raise FileNotFoundError("Missing built assemblies: " + ", ".join(missing))

    with tempfile.TemporaryDirectory(prefix="pdfbox-api-reflect-") as tmp:
        tmp_path = Path(tmp)
        (tmp_path / "ApiReflector.csproj").write_text(
            '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>\n',
            encoding="utf-8",
        )
        (tmp_path / "Program.cs").write_text(REFLECTOR_PROGRAM, encoding="utf-8")
        run(["dotnet", "restore", "ApiReflector.csproj"], cwd=tmp_path)
        completed = run(
            ["dotnet", "run", "--configuration", "Release", "--project", "ApiReflector.csproj", "--"]
            + [str(p) for p in assembly_paths],
            cwd=tmp_path,
            capture=True,
        )
    return json.loads(completed.stdout)["types"]


def pascal(name: str) -> str:
    if not name:
        return name
    if len(name) > 1 and name[:2].isupper():
        return name
    return name[0].upper() + name[1:]


def property_name_from_accessor(name: str) -> str | None:
    if name.startswith("get") and len(name) > 3 and name[3].isupper():
        return name[3:]
    if name.startswith("set") and len(name) > 3 and name[3].isupper():
        return name[3:]
    if name.startswith("is") and len(name) > 2 and name[2].isupper():
        return name[2:]
    return None


def java_member_candidates(member: dict) -> list[tuple[str, str, int, str]]:
    kind = member["kind"]
    name = member["name"]
    arity = int(member["arity"])
    candidates: list[tuple[str, str, int, str]] = []
    if kind == "constructor":
        candidates.append(("constructor", ".ctor", arity, "constructor"))
        return candidates

    names = {name, pascal(name)}
    aliases = {
        "close": ["Close", "Dispose"],
        "clone": ["Clone"],
        "equals": ["Equals"],
        "hashCode": ["GetHashCode"],
        "toString": ["ToString"],
        "iterator": ["GetEnumerator"],
    }
    for alias in aliases.get(name, []):
        names.add(alias)

    if kind == "method":
        for n in names:
            candidates.append(("method", n, arity, "method"))
        prop = property_name_from_accessor(name)
        if prop and ((name.startswith("set") and arity == 1) or (not name.startswith("set") and arity == 0)):
            candidates.append(("property", prop, 0, "accessor-property"))
    elif kind in {"field", "enum-value"}:
        for n in names:
            candidates.append((kind, n, 0, kind))
            candidates.append(("property", n, 0, "field-property"))
    return candidates


def member_key(member: dict) -> tuple[str, str, int]:
    return (member["kind"], member["name"].lower(), int(member["arity"]))


def compare_members(java_members: list[dict], cs_members: list[dict]) -> tuple[list[dict], Counter, set[int]]:
    cs_by_key: dict[tuple[str, str, int], list[int]] = defaultdict(list)
    cs_by_kind_name: dict[tuple[str, str], list[int]] = defaultdict(list)
    for idx, member in enumerate(cs_members):
        key = member_key(member)
        cs_by_key[key].append(idx)
        cs_by_kind_name[(key[0], key[1])].append(idx)

    rows: list[dict] = []
    counts: Counter = Counter()
    matched_cs: set[int] = set()
    for member in java_members:
        candidates = java_member_candidates(member)
        match = None
        for kind, name, arity, via in candidates:
            indexes = cs_by_key.get((kind, name.lower(), arity))
            if indexes:
                match = ("matched", via, indexes[0])
                break
        if match is None:
            for kind, name, _arity, via in candidates:
                indexes = cs_by_kind_name.get((kind, name.lower()))
                if indexes:
                    match = ("arity-drift", via, indexes[0])
                    break
        if match is None:
            status = "missing"
            via = None
            counts[status] += 1
        else:
            status, via, idx = match
            matched_cs.add(idx)
            counts[status] += 1
        rows.append(
            {
                "java_kind": member["kind"],
                "java_name": member["name"],
                "java_arity": member["arity"],
                "java_visibility": member["visibility"],
                "status": status,
                "matched_via": via,
                "signature": member.get("signature"),
            }
        )
    return rows, counts, matched_cs


def compare_api(java_types: list[dict], cs_types: list[dict], source_map: dict) -> dict:
    cs_by_simple: dict[str, list[dict]] = defaultdict(list)
    for t in cs_types:
        cs_by_simple[t["name"]].append(t)

    source_type_names: dict[str, set[str]] = defaultdict(set)
    for source, entries in source_map["by_source"].items():
        for entry in entries:
            source_type_names[source].add(entry["name"])

    type_rows: list[dict] = []
    totals = Counter()
    module_counts: dict[str, Counter] = defaultdict(Counter)
    family_counts: dict[str, Counter] = defaultdict(Counter)
    all_missing_members: list[dict] = []
    top_partial: list[dict] = []

    for jt in sorted(java_types, key=lambda t: (t["module"], t["family"], t["full_name"])):
        totals["java_types"] += 1
        module_counts[jt["module"]]["java_types"] += 1
        family_counts[f"{jt['module']}:{jt['family']}"]["java_types"] += 1

        source_entries = source_map["by_source"].get(jt["source_path"], [])
        exact_source_candidates = [
            entry
            for entry in source_entries
            if entry["name"] == jt["name"] or entry["name"] == jt["nested_name"].split(".")[-1]
        ]
        candidate_names = [entry["name"] for entry in exact_source_candidates] or [jt["name"]]
        cs_candidate = None
        type_name_status = "same-name"
        for candidate_name in candidate_names:
            candidates = cs_by_simple.get(candidate_name, [])
            if candidates:
                cs_candidate = candidates[0]
                break

        if cs_candidate is None:
            def replacement_priority(entry: dict) -> tuple[int, str]:
                name = entry["name"]
                java_name = jt["name"]
                if name in {f"I{java_name}", f"{java_name}Attribute"}:
                    return (0, name)
                if name.startswith(java_name) or name.endswith(java_name):
                    return (1, name)
                return (2, name)

            for entry in sorted(source_entries, key=replacement_priority):
                candidates = cs_by_simple.get(entry["name"], [])
                if candidates:
                    cs_candidate = candidates[0]
                    type_name_status = "renamed-public"
                    break

        if cs_candidate is None:
            if source_entries:
                type_status = "nonpublic-or-replacement-type"
                totals["nonpublic_or_replacement_types"] += 1
                module_counts[jt["module"]]["nonpublic_or_replacement_types"] += 1
                family_counts[f"{jt['module']}:{jt['family']}"]["nonpublic_or_replacement_types"] += 1
            else:
                type_status = "missing-type"
                totals["missing_types"] += 1
                module_counts[jt["module"]]["missing_types"] += 1
                family_counts[f"{jt['module']}:{jt['family']}"]["missing_types"] += 1
            missing_members = len(jt["members"])
            totals["java_members"] += missing_members
            totals["missing_members"] += missing_members
            module_counts[jt["module"]]["java_members"] += missing_members
            module_counts[jt["module"]]["missing_members"] += missing_members
            family_counts[f"{jt['module']}:{jt['family']}"]["java_members"] += missing_members
            family_counts[f"{jt['module']}:{jt['family']}"]["missing_members"] += missing_members
            type_rows.append(
                {
                    "source_path": jt["source_path"],
                    "module": jt["module"],
                    "family": jt["family"],
                    "java_type": jt["full_name"],
                    "java_type_name": jt["name"],
                    "status": type_status,
                    "java_member_count": missing_members,
                    "missing_member_count": missing_members,
                    "matched_member_count": 0,
                    "arity_drift_count": 0,
                    "csharp_type": None,
                    "csharp_targets": source_map["by_source"].get(jt["source_path"], []),
                    "members": [
                        {
                            "java_kind": m["kind"],
                            "java_name": m["name"],
                            "java_arity": m["arity"],
                            "status": "missing",
                            "signature": m.get("signature"),
                        }
                        for m in jt["members"]
                    ],
                }
            )
            continue

        member_rows, member_counts, matched_cs = compare_members(jt["members"], cs_candidate.get("members", []))
        member_total = len(jt["members"])
        missing_count = member_counts["missing"]
        matched_count = member_counts["matched"]
        arity_drift = member_counts["arity-drift"]
        totals["matched_types"] += 1
        if type_name_status == "same-name":
            totals["same_name_matched_types"] += 1
            module_counts[jt["module"]]["same_name_matched_types"] += 1
            family_counts[f"{jt['module']}:{jt['family']}"]["same_name_matched_types"] += 1
        else:
            totals["renamed_public_types"] += 1
            module_counts[jt["module"]]["renamed_public_types"] += 1
            family_counts[f"{jt['module']}:{jt['family']}"]["renamed_public_types"] += 1
        totals["java_members"] += member_total
        totals["matched_members"] += matched_count
        totals["arity_drift_members"] += arity_drift
        totals["missing_members"] += missing_count
        totals["csharp_extra_members"] += max(0, len(cs_candidate.get("members", [])) - len(matched_cs))
        for counter in (module_counts[jt["module"]], family_counts[f"{jt['module']}:{jt['family']}"]):
            counter["matched_types"] += 1
            counter["java_members"] += member_total
            counter["matched_members"] += matched_count
            counter["arity_drift_members"] += arity_drift
            counter["missing_members"] += missing_count
            counter["csharp_extra_members"] += max(0, len(cs_candidate.get("members", [])) - len(matched_cs))

        status_prefix = "renamed-public-type" if type_name_status == "renamed-public" else "same-name-type"
        status = f"{status_prefix}/full-member-match" if missing_count == 0 else f"{status_prefix}/partial-member-match"
        if missing_count:
            for row in member_rows:
                if row["status"] == "missing":
                    all_missing_members.append(
                        {
                            "source_path": jt["source_path"],
                            "module": jt["module"],
                            "family": jt["family"],
                            "java_type": jt["full_name"],
                            **row,
                        }
                    )
            top_partial.append(
                {
                    "source_path": jt["source_path"],
                    "module": jt["module"],
                    "family": jt["family"],
                    "java_type": jt["full_name"],
                    "csharp_type": cs_candidate["fullName"],
                    "java_member_count": member_total,
                    "missing_member_count": missing_count,
                    "matched_member_count": matched_count,
                    "arity_drift_count": arity_drift,
                }
            )

        type_rows.append(
            {
                "source_path": jt["source_path"],
                "module": jt["module"],
                "family": jt["family"],
                "java_type": jt["full_name"],
                "java_type_name": jt["name"],
                "type_name_status": type_name_status,
                "status": status,
                "java_member_count": member_total,
                "missing_member_count": missing_count,
                "matched_member_count": matched_count,
                "arity_drift_count": arity_drift,
                "csharp_type": cs_candidate["fullName"],
                "csharp_member_count": len(cs_candidate.get("members", [])),
                "csharp_extra_member_count": max(0, len(cs_candidate.get("members", [])) - len(matched_cs)),
                "csharp_targets": source_map["by_source"].get(jt["source_path"], []),
                "members": member_rows,
            }
        )

    top_partial.sort(key=lambda row: (row["missing_member_count"], row["java_member_count"]), reverse=True)
    all_missing_members.sort(key=lambda row: (row["module"], row["family"], row["java_type"], row["java_name"], row["java_arity"]))
    for key in (
        "java_types",
        "matched_types",
        "same_name_matched_types",
        "renamed_public_types",
        "nonpublic_or_replacement_types",
        "missing_types",
        "java_members",
        "matched_members",
        "arity_drift_members",
        "missing_members",
        "csharp_extra_members",
    ):
        totals.setdefault(key, 0)
        for counts in module_counts.values():
            counts.setdefault(key, 0)
        for counts in family_counts.values():
            counts.setdefault(key, 0)
    return {
        "totals": dict(totals),
        "by_module": {module: dict(counts) for module, counts in sorted(module_counts.items())},
        "by_family": {family: dict(counts) for family, counts in sorted(family_counts.items())},
        "type_rows": type_rows,
        "top_partial_types": top_partial[:50],
        "missing_members": all_missing_members,
    }


def pct(numerator: int, denominator: int) -> float:
    return 100.0 * numerator / denominator if denominator else 0.0


def build_markdown(payload: dict) -> str:
    totals = payload["totals"]
    matched_or_drift = totals.get("matched_members", 0) + totals.get("arity_drift_members", 0)
    lines: list[str] = []
    lines.append("# PDFBox API Surface Parity Analysis")
    lines.append("")
    lines.append(f"Generated (UTC): {payload['generated_at_utc']}")
    lines.append(f"Apache PDFBox source commit: `{payload['upstream_commit']}`")
    lines.append(f"PdfBox.Net commit: `{payload['pdfbox_net_commit']}`")
    lines.append("")
    lines.append("## Scope")
    lines.append("")
    lines.append("- Compared public/protected API surface from Apache PDFBox library modules: `io`, `fontbox`, `xmpbox`, and `pdfbox`.")
    lines.append("- Java side is parsed from `**/src/main/java/**/*.java` because this environment has only Apple Java stubs, not a runnable JDK.")
    lines.append("- .NET side is reflected from Release `net10.0` assemblies after building the core projects.")
    lines.append("- Matching allows normal C# capitalization and JavaBean accessor-to-property mappings, and records arity drift separately from missing members.")
    lines.append("- This is an API-shape comparison. It does not prove behavioral equivalence; the runtime parity corpus covers behavior separately.")
    lines.append("")
    lines.append("## Summary")
    lines.append("")
    lines.append("| Metric | Count |")
    lines.append("|---|---:|")
    lines.append(f"| Java public/protected types | {totals.get('java_types', 0)} |")
    lines.append(f"| Matched public .NET types | {totals.get('matched_types', 0)} |")
    lines.append(f"| Same-name public .NET types | {totals.get('same_name_matched_types', 0)} |")
    lines.append(f"| Renamed public .NET replacements | {totals.get('renamed_public_types', 0)} |")
    lines.append(f"| Mapped but non-public/replacement-marker types | {totals.get('nonpublic_or_replacement_types', 0)} |")
    lines.append(f"| Missing mapped public .NET types | {totals.get('missing_types', 0)} |")
    lines.append(f"| Java public/protected members | {totals.get('java_members', 0)} |")
    lines.append(f"| Matched members | {totals.get('matched_members', 0)} |")
    lines.append(f"| Arity-drift members | {totals.get('arity_drift_members', 0)} |")
    lines.append(f"| Missing members | {totals.get('missing_members', 0)} |")
    lines.append(f"| Reflected .NET extra members on matched types | {totals.get('csharp_extra_members', 0)} |")
    lines.append("")
    lines.append(
        f"Member coverage by name/signature heuristic: **{matched_or_drift} / {totals.get('java_members', 0)} = {pct(matched_or_drift, totals.get('java_members', 0)):.1f}%**."
    )
    lines.append("")
    lines.append("## Module Breakdown")
    lines.append("")
    lines.append("| Module | Java types | Same-name types | Renamed public types | Non-public/replacement types | Missing types | Java members | Matched/arity-drift members | Missing members | Member coverage |")
    lines.append("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|")
    for module, row in payload["by_module"].items():
        covered = row.get("matched_members", 0) + row.get("arity_drift_members", 0)
        lines.append(
            f"| `{module}` | {row.get('java_types', 0)} | {row.get('same_name_matched_types', 0)} | "
            f"{row.get('renamed_public_types', 0)} | {row.get('nonpublic_or_replacement_types', 0)} | "
            f"{row.get('missing_types', 0)} | "
            f"{row.get('java_members', 0)} | {covered} | {row.get('missing_members', 0)} | {pct(covered, row.get('java_members', 0)):.1f}% |"
        )
    lines.append("")
    lines.append("## Highest Missing-Member Types")
    lines.append("")
    lines.append("| Missing | Java members | Module | Java type | .NET type | Source |")
    lines.append("|---:|---:|---|---|---|---|")
    for row in payload["top_partial_types"][:30]:
        lines.append(
            f"| {row['missing_member_count']} | {row['java_member_count']} | `{row['module']}` | "
            f"`{row['java_type']}` | `{row.get('csharp_type') or ''}` | `{row['source_path']}` |"
        )
    if not payload["top_partial_types"]:
        lines.append("| 0 | 0 |  | No partial types found |  |  |")
    lines.append("")
    lines.append("## Java-Named Public API Type Gaps")
    lines.append("")
    missing_types = [row for row in payload["type_rows"] if row["status"] in {"missing-type", "nonpublic-or-replacement-type"}]
    renamed_types = [row for row in payload["type_rows"] if row.get("type_name_status") == "renamed-public"]
    if missing_types or renamed_types:
        lines.append("| Status | Module | Java type | .NET type | Members | Source |")
        lines.append("|---|---|---|---|---:|---|")
        for row in missing_types:
            status = "mapped but non-public/replacement" if row["status"] == "nonpublic-or-replacement-type" else "missing mapped public type"
            target_names = ", ".join(sorted({entry["name"] for entry in row.get("csharp_targets", [])}))
            lines.append(
                f"| {status} | `{row['module']}` | `{row['java_type']}` | `{target_names}` | "
                f"{row['java_member_count']} | `{row['source_path']}` |"
            )
        for row in renamed_types:
            lines.append(
                f"| renamed public replacement | `{row['module']}` | `{row['java_type']}` | `{row.get('csharp_type') or ''}` | "
                f"{row['java_member_count']} | `{row['source_path']}` |"
            )
    else:
        lines.append("No public/protected Java types in the scoped library modules were missing, renamed, or hidden from the public .NET API.")
    lines.append("")
    lines.append("## Assessment")
    lines.append("")
    lines.append("- The port has complete source-file coverage for the scoped library modules, and the current runtime corpus is green, but the Java-compatible API surface is not yet complete.")
    lines.append("- The largest API-shape gaps are public/protected overloads and extension points, especially where Java exposes `File`, `InputStream`, `RandomAccessRead`, AWT, collection, and checked-exception-oriented signatures that were narrowed or adapted in C#.")
    lines.append("- Missing members in a matched type do not automatically mean the underlying feature is absent; some are deliberate .NET idioms or overload collapses. They do identify places where Java client code cannot be mechanically ported without adaptation.")
    lines.append("- Renamed public replacements and non-public compatibility markers are source-coverage wins but Java API-compatibility gaps unless the project intentionally documents them as .NET-only API design.")
    lines.append("- Arity-drift rows require manual review: the member name exists in .NET, but overload coverage does not match Java.")
    lines.append("- The machine-readable detail in `reports/api-surface-comparison.json` should be used as the backlog seed for API compatibility issues, with behavioral parity tests added before marking each family complete.")
    lines.append("")
    lines.append("## Next API-Parity Work")
    lines.append("")
    lines.append("1. Review the top missing-member types and decide which Java overloads should be preserved versus documented as intentional .NET adaptations.")
    lines.append("2. Add compatibility overloads for stable, low-risk entry points such as `Loader`, `PDDocument`, `PDFMergerUtility`, `PDFTextStripper`, font loaders, image factories, and annotation/form models.")
    lines.append("3. Split high-risk areas into feature issues where API shape and behavior must land together: encryption/public-key loading, external signing, image factories, rendering extension points, and font embedding/subsetting.")
    lines.append("4. Add an API parity gate that fails only on newly introduced missing Java API rows, then ratchet reviewed gaps downward.")
    lines.append("")
    return "\n".join(lines) + "\n"


def write_json(path: Path, payload: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate Java-vs-.NET API surface parity reports.")
    parser.add_argument(
        "--upstream-root",
        type=Path,
        default=Path.home() / "src/Repos/apache/pdfbox",
        help="Path to the Apache PDFBox checkout.",
    )
    parser.add_argument("--configuration", default="Release", help="Build configuration for reflected .NET assemblies.")
    parser.add_argument("--no-build", action="store_true", help="Use existing .NET build outputs.")
    args = parser.parse_args()

    repo_root = Path.cwd()
    upstream_root = args.upstream_root.expanduser().resolve()
    if not upstream_root.exists():
        raise FileNotFoundError(upstream_root)
    if shutil.which("dotnet") is None:
        raise RuntimeError("dotnet is required to reflect the C# API surface")

    if not args.no_build:
        build_core_projects(repo_root, args.configuration)

    java_types = collect_java_api(upstream_root)
    source_map = collect_csharp_source_map(repo_root)
    cs_types = reflect_csharp_api(repo_root, args.configuration)
    comparison = compare_api(java_types, cs_types, source_map)
    payload = {
        "schema": 1,
        "generated_at_utc": utc_now_iso(),
        "upstream_root": str(upstream_root),
        "upstream_commit": git_head(upstream_root),
        "pdfbox_net_commit": git_head(repo_root),
        "scope": {
            "modules": list(CORE_MODULES),
            "java": "public/protected types and members parsed from source",
            "dotnet": f"public/protected types and members reflected from {args.configuration} net10.0 assemblies",
            "matching": "case-normalized method matching, JavaBean accessor-to-property matching, arity drift tracked separately",
        },
        **comparison,
    }
    write_json(REPORT_JSON, payload)
    REPORT_MD.write_text(build_markdown(payload), encoding="utf-8")


if __name__ == "__main__":
    main()
