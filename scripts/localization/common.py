"""Shared utilities for Smithbox localization tool scripts.

The localization data lives under:
    <repo>/src/Smithbox.Data/Assets/Localization/

Layout:
    Languages.json              # registry of available languages
    English/*.json              # source language (fallback)
    <Folder>/*.json             # one subfolder per language

Each entry file has the shape:
    { "Entries": [ { "Key": "...", "Text": "..." }, ... ] }

File format conventions (must be preserved when writing):
    - UTF-8, no BOM
    - CRLF line endings
    - 2-space indentation
    - Non-ASCII characters stored literally (ensure_ascii=False)
    - No trailing newline at end of file
"""

from __future__ import annotations

import json
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterator

# ---------------------------------------------------------------------------
# Path resolution
# ---------------------------------------------------------------------------

# This file lives at <repo>/scripts/localization/common.py
REPO_ROOT = Path(__file__).resolve().parents[2]
LOCALIZATION_DIR = REPO_ROOT / "src" / "Smithbox.Data" / "Assets" / "Localization"
LANGUAGES_FILE = LOCALIZATION_DIR / "Languages.json"
SOURCE_LANGUAGE = "English"  # the canonical / fallback language


# ---------------------------------------------------------------------------
# Data model
# ---------------------------------------------------------------------------


@dataclass
class LanguageEntry:
    """One entry from Languages.json."""

    key: str
    name: str
    folder: str
    ui_culture: str


@dataclass
class EntryFile:
    """A single localization JSON file on disk."""

    path: Path
    entries: list[dict] = field(default_factory=list)

    @property
    def name(self) -> str:
        return self.path.name

    def as_dict(self) -> dict[str, str | None]:
        """First-wins mapping (mirrors C# Dictionary.TryAdd behavior).

        Text may be None (untranslated placeholder) or a string.
        """
        out: dict[str, str | None] = {}
        for e in self.entries:
            k = e.get("Key")
            if k is not None and k not in out:
                out[k] = e.get("Text")  # None if missing or explicitly null
        return out


# ---------------------------------------------------------------------------
# Languages.json
# ---------------------------------------------------------------------------


def load_languages() -> list[LanguageEntry]:
    """Load and parse Languages.json. Returns [] if the file is missing."""
    if not LANGUAGES_FILE.exists():
        return []
    data = _load_json(LANGUAGES_FILE)
    out: list[LanguageEntry] = []
    for item in data.get("Languages", []):
        out.append(
            LanguageEntry(
                key=item.get("Key", ""),
                name=item.get("Name", ""),
                folder=item.get("Folder", ""),
                ui_culture=item.get("UICulture", ""),
            )
        )
    return out


def get_language(name: str) -> LanguageEntry | None:
    """Look up a language by its Name field (e.g. 'English', 'Chinese')."""
    for lang in load_languages():
        if lang.name == name:
            return lang
    return None


# ---------------------------------------------------------------------------
# Entry file I/O
# ---------------------------------------------------------------------------


def language_dir(folder: str) -> Path:
    return LOCALIZATION_DIR / folder


def iter_entry_files(folder: str) -> Iterator[Path]:
    """Yield every *.json path inside a language folder, sorted."""
    d = language_dir(folder)
    if not d.is_dir():
        return
    yield from sorted(d.glob("*.json"))


def load_entry_file(path: Path) -> EntryFile:
    """Load a single entry JSON file. Raises on parse error."""
    data = _load_json(path)
    entries = data.get("Entries", []) or []
    return EntryFile(path=path, entries=list(entries))


def load_language_entries(folder: str) -> dict[str, EntryFile]:
    """Load every entry file in a language folder.

    Returns {filename: EntryFile}.
    """
    out: dict[str, EntryFile] = {}
    for p in iter_entry_files(folder):
        out[p.name] = load_entry_file(p)
    return out


def load_merged_entries(folder: str) -> dict[str, str | None]:
    """Merge all files in a language folder into a single first-wins dict.

    Mirrors the C# LOC.Load() behavior: files are enumerated in sorted
    (alphabetical) order and the first occurrence of a key wins.
    Text may be None for untranslated placeholders.
    """
    merged: dict[str, str | None] = {}
    for ef in load_language_entries(folder).values():
        for k, v in ef.as_dict().items():
            if k not in merged:
                merged[k] = v
    return merged


def save_entry_file(ef: EntryFile) -> None:
    """Write an EntryFile back to disk, preserving repo formatting."""
    data = {"Entries": ef.entries}
    text = json.dumps(data, indent=2, ensure_ascii=False)
    # json.dumps uses LF; the repo uses CRLF.
    text = text.replace("\r\n", "\n").replace("\n", "\r\n")
    ef.path.write_bytes(text.encode("utf-8"))  # no BOM, no trailing newline


# ---------------------------------------------------------------------------
# Placeholder / parameter-slot helpers
# ---------------------------------------------------------------------------

# Matches {0}, {1}, {12}, {0:format} etc. as used by string.Format.
_PLACEHOLDER_RE = re.compile(r"\{(\d+)(?::[^}]*)?\}")


def extract_placeholders(text: str) -> list[str]:
    """Return the sorted list of numeric placeholders in a Text string.

    Example: 'Load {0}: {1}' -> ['0', '1']
    """
    return sorted({m.group(1) for m in _PLACEHOLDER_RE.finditer(text or "")})


def placeholder_mismatch(en_text: str, other_text: str) -> tuple[list[str], list[str]]:
    """Compare placeholders between source and translated text.

    Returns (missing_in_translation, extra_in_translation) where each list
    contains placeholder indices as strings.
    """
    en_set = set(extract_placeholders(en_text))
    other_set = set(extract_placeholders(other_text))
    missing = sorted(en_set - other_set)
    extra = sorted(other_set - en_set)
    return missing, extra


# ---------------------------------------------------------------------------
# Internal helpers
# ---------------------------------------------------------------------------


def _load_json(path: Path) -> dict:
    raw = path.read_text(encoding="utf-8")
    return json.loads(raw)
