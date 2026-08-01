"""Validate localization data for correctness and consistency.

Checks performed (per language, against the English source):
    1. JSON parseability of every entry file.
    2. No duplicate keys within a single file.
    3. No duplicate keys across files in the same language (the C# loader
       silently drops duplicates via TryAdd; duplicates are almost always
       a mistake).
    4. Parameter-slot ({0}, {1}, ...) counts match the English source for
       every translated key. A mismatch will crash string.Format at runtime.
    5. Empty-string Text values are reported as errors (should be null
       instead, so the C# loader skips them and falls back to English).
       null Text is the normal "untranslated" marker and is NOT an error.
    6. Key naming convention: keys should be non-empty and contain no
       whitespace.

Text value semantics:
    null            untranslated (UI shows English via fallback) — OK
    ""              empty string — ERROR (use null instead)
    "some text"     translated or intentionally-kept English — OK

Exit code is non-zero if any error (or warning with --strict) is found.

Usage:
    python validate.py                 # validate all languages
    python validate.py Chinese         # validate one language
    python validate.py --strict        # treat warnings as errors
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from common import (
    SOURCE_LANGUAGE,
    extract_placeholders,
    get_language,
    iter_entry_files,
    language_dir,
    load_languages,
    load_merged_entries,
    placeholder_mismatch,
)


def validate_language(folder: str, is_test_language: bool = False) -> tuple[list[dict], dict]:
    """Validate one language folder.

    Returns (issues, stats) where issues is a list of dicts:
        {severity: 'ERROR'|'WARNING', file, key, message}
    and stats has counts: total, translated, untranslated, empty_string.
    """
    issues: list[dict] = []
    seen_keys_global: dict[str, str] = {}  # key -> first file seen
    stats = {"total": 0, "translated": 0, "untranslated": 0, "empty_string": 0}

    for path in iter_entry_files(folder):
        fname = path.name

        # 1. JSON parseability
        try:
            raw = path.read_text(encoding="utf-8")
            data = json.loads(raw)
        except json.JSONDecodeError as e:
            issues.append(
                {
                    "severity": "ERROR",
                    "file": fname,
                    "key": "",
                    "message": f"JSON parse error: {e}",
                }
            )
            continue
        except OSError as e:
            issues.append(
                {
                    "severity": "ERROR",
                    "file": fname,
                    "key": "",
                    "message": f"File read error: {e}",
                }
            )
            continue

        entries = data.get("Entries", []) or []

        # Walk entries.
        seen_in_file: set[str] = set()
        for e in entries:
            key = e.get("Key", "")
            # Note: e.get("Text") returns None for both "Text": null and
            # missing Text field. e.get("Text", "") would conflate the two.
            has_text_field = "Text" in e
            text = e.get("Text")

            # 6. Key naming
            if not key:
                issues.append(
                    {
                        "severity": "ERROR",
                        "file": fname,
                        "key": key,
                        "message": "Empty or missing Key",
                    }
                )
                continue
            if any(c.isspace() for c in key):
                issues.append(
                    {
                        "severity": "WARNING",
                        "file": fname,
                        "key": key,
                        "message": "Key contains whitespace",
                    }
                )

            # 2. Duplicate within file
            if key in seen_in_file:
                issues.append(
                    {
                        "severity": "ERROR",
                        "file": fname,
                        "key": key,
                        "message": "Duplicate key within file",
                    }
                )
            else:
                seen_in_file.add(key)

            # 3. Duplicate across files
            if key in seen_keys_global:
                issues.append(
                    {
                        "severity": "WARNING",
                        "file": fname,
                        "key": key,
                        "message": f"Duplicate key across files (first in {seen_keys_global[key]})",
                    }
                )
            else:
                seen_keys_global[key] = fname

            stats["total"] += 1

            # 5. Text value checks
            if not has_text_field:
                # Missing Text field — treat like null (C# deserializes to null).
                stats["untranslated"] += 1
            elif text is None:
                # null = untranslated placeholder, normal and expected.
                stats["untranslated"] += 1
            elif text == "":
                # Empty string:
                #   - In the SOURCE language (English): valid — means
                #     "intentionally no text" (e.g. empty tooltip).
                #   - In non-source languages: ERROR — it does NOT trigger
                #     fallback (TryGetValue returns true) and shows blank.
                #     Should be null instead.
                if folder == SOURCE_LANGUAGE:
                    stats["translated"] += 1
                else:
                    stats["empty_string"] += 1
                    issues.append(
                        {
                            "severity": "ERROR",
                            "file": fname,
                            "key": key,
                            "message": "Empty-string Text (use null for untranslated, "
                            "so the UI falls back to English)",
                        }
                    )
            else:
                stats["translated"] += 1

    # 4. Parameter-slot checks against English (skip for Test Language).
    if not is_test_language and folder != SOURCE_LANGUAGE:
        en_merged = load_merged_entries(SOURCE_LANGUAGE)
        tgt_merged = load_merged_entries(folder)
        for key, en_text in en_merged.items():
            if key not in tgt_merged:
                continue
            tgt_text = tgt_merged[key]
            if not tgt_text:
                continue  # null or empty — skip (empty already reported)
            miss, extra = placeholder_mismatch(en_text, tgt_text)
            if miss or extra:
                tag = []
                if miss:
                    tag.append(f"missing {{{', '.join(miss)}}}")
                if extra:
                    tag.append(f"extra {{{', '.join(extra)}}}")
                issues.append(
                    {
                        "severity": "ERROR",
                        "file": "",
                        "key": key,
                        "message": f"Parameter-slot mismatch: {'; '.join(tag)}",
                    }
                )

    return issues, stats


def print_issues(folder: str, issues: list[dict], stats: dict) -> tuple[int, int]:
    errors = sum(1 for i in issues if i["severity"] == "ERROR")
    warnings = sum(1 for i in issues if i["severity"] == "WARNING")
    print(f"\n=== {folder} ===  ({errors} errors, {warnings} warnings)")
    print(
        f"  Stats: {stats['total']} total, "
        f"{stats['translated']} translated, "
        f"{stats['untranslated']} untranslated (null), "
        f"{stats['empty_string']} empty-string"
    )
    if not issues:
        print("  OK - no issues found.")
        return errors, warnings
    for i in issues:
        loc = i["file"] or "(cross-file)"
        print(f"  [{i['severity']}] {loc}: {i['key']}  -  {i['message']}")
    return errors, warnings


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument(
        "language",
        nargs="?",
        help="Validate only this language (Name or folder). Default: all.",
    )
    ap.add_argument(
        "--strict",
        action="store_true",
        help="Treat warnings as errors (non-zero exit if any warning).",
    )
    args = ap.parse_args(argv)

    # Build the list of language folders to validate.
    languages = load_languages()
    if args.language:
        lang = get_language(args.language)
        folder = lang.folder if lang else args.language
        targets = [(folder, folder == "Empty")]  # Empty == Test Language
    else:
        targets = [(l.folder, l.folder == "Empty") for l in languages]

    total_errors = 0
    total_warnings = 0
    for folder, is_test in targets:
        if not language_dir(folder).is_dir():
            print(f"\n=== {folder} ===  (folder missing, skipped)")
            continue
        issues, stats = validate_language(folder, is_test_language=is_test)
        e, w = print_issues(folder, issues, stats)
        total_errors += e
        total_warnings += w

    print()
    print(f"Total: {total_errors} errors, {total_warnings} warnings")
    if args.strict:
        total_errors += total_warnings
    return 1 if total_errors else 0


if __name__ == "__main__":
    sys.exit(main())
