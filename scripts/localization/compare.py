"""Compare a target language against the English source and report coverage.

Reports, per file and overall:
    - missing keys (present in English, absent in target)
    - extra keys (present in target, absent in English)
    - parameter-slot mismatches ({0}, {1}, ...) for translated entries
    - coverage percentage

Usage:
    python compare.py                         # default: compare Chinese
    python compare.py Chinese                 # compare a named language
    python compare.py Chinese --csv out.csv   # also write a CSV report
    python compare.py Chinese --missing       # print every missing key
"""

from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path

from common import (
    SOURCE_LANGUAGE,
    extract_placeholders,
    get_language,
    load_entry_file,
    load_languages,
    load_merged_entries,
    placeholder_mismatch,
    iter_entry_files,
)


def compare_language(target_folder: str) -> dict:
    """Run the comparison and return a structured result dict.

    Per key, three states are tracked:
        missing       — key absent from target file
        untranslated  — key present but Text is null (UI falls back to English)
        translated    — key present and Text is non-null
    """
    en_files = {p.name: load_entry_file(p) for p in iter_entry_files(SOURCE_LANGUAGE)}
    tgt_files = {p.name: load_entry_file(p) for p in iter_entry_files(target_folder)}

    per_file: list[dict] = []
    total_en = 0
    total_translated = 0
    total_untranslated = 0
    total_missing = 0
    total_extra = 0
    total_param_issues = 0

    for fname in sorted(en_files):
        en_ef = en_files[fname]
        en_dict = en_ef.as_dict()
        tgt_ef = tgt_files.get(fname)
        tgt_dict = tgt_ef.as_dict() if tgt_ef else {}

        en_keys = set(en_dict)
        tgt_keys = set(tgt_dict)

        missing = sorted(en_keys - tgt_keys)
        extra = sorted(tgt_keys - en_keys)
        common = en_keys & tgt_keys

        # Split common keys into translated vs untranslated (null Text).
        translated_keys: list[str] = []
        untranslated_keys: list[str] = []
        for k in sorted(common):
            if tgt_dict[k] is None:
                untranslated_keys.append(k)
            else:
                translated_keys.append(k)

        # Parameter-slot checks only for translated keys (non-null Text).
        param_issues: list[dict] = []
        for k in translated_keys:
            miss, extra_p = placeholder_mismatch(en_dict[k], tgt_dict[k])
            if miss or extra_p:
                param_issues.append(
                    {
                        "key": k,
                        "en_text": en_dict[k],
                        "tgt_text": tgt_dict[k],
                        "missing_slots": miss,
                        "extra_slots": extra_p,
                    }
                )

        per_file.append(
            {
                "file": fname,
                "en_count": len(en_keys),
                "translated": len(translated_keys),
                "untranslated": len(untranslated_keys),
                "missing": len(missing),
                "extra": len(extra),
                "param_issues": len(param_issues),
                "missing_keys": missing,
                "untranslated_keys": untranslated_keys,
                "extra_keys": extra,
                "param_issue_details": param_issues,
            }
        )
        total_en += len(en_keys)
        total_translated += len(translated_keys)
        total_untranslated += len(untranslated_keys)
        total_missing += len(missing)
        total_extra += len(extra)
        total_param_issues += len(param_issues)

    # Files that exist in target but not in English at all.
    target_only_files = sorted(set(tgt_files) - set(en_files))

    coverage = (total_translated / total_en * 100) if total_en else 0.0

    return {
        "target_folder": target_folder,
        "total_en_keys": total_en,
        "total_translated": total_translated,
        "total_untranslated": total_untranslated,
        "total_missing": total_missing,
        "total_extra": total_extra,
        "total_param_issues": total_param_issues,
        "coverage_pct": round(coverage, 2),
        "per_file": per_file,
        "target_only_files": target_only_files,
    }


def print_report(result: dict, show_missing: bool = False) -> None:
    cov = result["coverage_pct"]
    print(f"Language folder : {result['target_folder']}")
    print(f"English keys    : {result['total_en_keys']}")
    print(f"Translated      : {result['total_translated']}")
    print(f"Untranslated    : {result['total_untranslated']}  (null Text, UI shows English)")
    print(f"Missing         : {result['total_missing']}  (key absent from target)")
    print(f"Extra           : {result['total_extra']}")
    print(f"Param mismatches: {result['total_param_issues']}")
    print(f"Coverage        : {cov}%")
    print()

    if result["target_only_files"]:
        print("Files only in target (not in English):")
        for f in result["target_only_files"]:
            print(f"  {f}")
        print()

    # Per-file table, sorted by (missing + untranslated) desc.
    rows = sorted(
        result["per_file"],
        key=lambda r: r["missing"] + r["untranslated"],
        reverse=True,
    )
    print(
        f"{'File':<50} {'EN':>5} {'Done':>5} {'Null':>5} {'Miss':>5} {'Extra':>6} {'Params':>7}"
    )
    print("-" * 90)
    for r in rows:
        print(
            f"{r['file']:<50} {r['en_count']:>5} {r['translated']:>5} "
            f"{r['untranslated']:>5} {r['missing']:>5} {r['extra']:>6} {r['param_issues']:>7}"
        )
    print()

    # Parameter mismatch details (always shown if any).
    if result["total_param_issues"]:
        print("Parameter-slot mismatches:")
        for r in rows:
            for pi in r["param_issue_details"]:
                tag = []
                if pi["missing_slots"]:
                    tag.append(f"missing {{{', '.join(pi['missing_slots'])}}}")
                if pi["extra_slots"]:
                    tag.append(f"extra {{{', '.join(pi['extra_slots'])}}}")
                print(f"  [{r['file']}] {pi['key']}: {'; '.join(tag)}")
                print(f"      EN : {pi['en_text']!r}")
                print(f"      ZH : {pi['tgt_text']!r}")
        print()

    if show_missing and (result["total_missing"] or result["total_untranslated"]):
        print("All untranslated / missing keys:")
        for r in sorted(result["per_file"], key=lambda x: x["file"]):
            for k in r["missing_keys"]:
                print(f"  [{r['file']}] {k}  (missing)")
            for k in r["untranslated_keys"]:
                print(f"  [{r['file']}] {k}  (null)")
        print()


def write_csv(result: dict, out_path: Path) -> None:
    with out_path.open("w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(
            [
                "file",
                "en_count",
                "translated",
                "untranslated",
                "missing",
                "extra",
                "param_issues",
                "coverage_pct",
            ]
        )
        for r in sorted(result["per_file"], key=lambda x: x["file"]):
            file_cov = (r["translated"] / r["en_count"] * 100) if r["en_count"] else 0.0
            w.writerow(
                [
                    r["file"],
                    r["en_count"],
                    r["translated"],
                    r["untranslated"],
                    r["missing"],
                    r["extra"],
                    r["param_issues"],
                    f"{file_cov:.2f}",
                ]
            )
    print(f"CSV report written to: {out_path}")


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument(
        "language",
        nargs="?",
        default="Chinese",
        help="Target language Name (as in Languages.json) or folder name. "
        "Default: Chinese",
    )
    ap.add_argument("--csv", type=Path, help="Write a per-file CSV report to this path.")
    ap.add_argument(
        "--missing",
        action="store_true",
        help="Also print every missing key (verbose).",
    )
    args = ap.parse_args(argv)

    # Accept either a Name ('Chinese') or a folder name.
    folder = args.language
    lang = get_language(args.language)
    if lang is not None:
        folder = lang.folder
    else:
        # Treat the argument as a folder name directly.
        for l in load_languages():
            if l.folder == args.language:
                folder = l.folder
                break

    result = compare_language(folder)
    print_report(result, show_missing=args.missing)
    if args.csv:
        write_csv(result, args.csv)
    return 0


if __name__ == "__main__":
    sys.exit(main())
