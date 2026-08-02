"""Synchronize a target language's entry files with the English source.

For every English entry file, ensures a matching file exists in the target
language folder and that every English key is present in it. Missing keys
are appended with Text = null (a placeholder to be filled in by a
translator). The C# loader skips null-Text entries, so the UI falls back
to the English source for those keys automatically.

Existing non-null translations are never overwritten.

Text value semantics:
    null            untranslated (UI shows English via fallback)
    "English text"  reviewed, intentionally kept as English
    "Chinese text"  translated

Options:
    --remove-extra   Also delete keys from target files that do not exist
                     in English (use with caution).
    --dry-run        Show what would change without writing anything.

Usage:
    python sync.py Chinese
    python sync.py Chinese --dry-run
    python sync.py Chinese --remove-extra
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from common import (
    SOURCE_LANGUAGE,
    EntryFile,
    get_language,
    iter_entry_files,
    language_dir,
    load_entry_file,
    load_languages,
    save_entry_file,
)


def sync_language(
    target_folder: str,
    remove_extra: bool = False,
    dry_run: bool = False,
) -> dict:
    """Synchronize target language files with English.

    Returns a summary dict with counts of actions taken.
    """
    summary = {
        "files_created": 0,
        "keys_added": 0,
        "keys_removed": 0,
        "files_untouched": 0,
        "details": [],
    }

    tgt_dir = language_dir(target_folder)
    if not tgt_dir.is_dir():
        if dry_run:
            summary["details"].append(f"WOULD CREATE directory: {tgt_dir}")
        else:
            tgt_dir.mkdir(parents=True)
            summary["details"].append(f"Created directory: {tgt_dir}")

    for en_path in iter_entry_files(SOURCE_LANGUAGE):
        en_ef = load_entry_file(en_path)
        en_keys = {e["Key"] for e in en_ef.entries if "Key" in e}

        tgt_path = tgt_dir / en_path.name
        if not tgt_path.exists():
            # Create a new file mirroring the English structure with null Text
            # (untranslated placeholder; C# loader skips null and falls back).
            new_entries = [
                {"Key": e["Key"], "Text": None} for e in en_ef.entries if "Key" in e
            ]
            if dry_run:
                summary["files_created"] += 1
                summary["keys_added"] += len(new_entries)
                summary["details"].append(
                    f"WOULD CREATE {en_path.name} with {len(new_entries)} null entries"
                )
            else:
                ef = EntryFile(path=tgt_path, entries=new_entries)
                save_entry_file(ef)
                summary["files_created"] += 1
                summary["keys_added"] += len(new_entries)
                summary["details"].append(
                    f"Created {en_path.name} with {len(new_entries)} null entries"
                )
            continue

        tgt_ef = load_entry_file(tgt_path)
        tgt_keys = {e["Key"] for e in tgt_ef.entries if "Key" in e}

        changed = False

        # Append missing keys with null Text (preserve English insertion order).
        for e in en_ef.entries:
            k = e.get("Key")
            if k is not None and k not in tgt_keys:
                tgt_ef.entries.append({"Key": k, "Text": None})
                summary["keys_added"] += 1
                changed = True

        # Optionally remove extra keys not present in English.
        removed_here = 0
        if remove_extra:
            new_entries = [
                e for e in tgt_ef.entries if e.get("Key") in en_keys or "Key" not in e
            ]
            removed_here = len(tgt_ef.entries) - len(new_entries)
            if removed_here:
                tgt_ef.entries = new_entries
                summary["keys_removed"] += removed_here
                changed = True

        if changed:
            if dry_run:
                summary["details"].append(
                    f"WOULD UPDATE {en_path.name}: "
                    f"+{sum(1 for e in en_ef.entries if e.get('Key') not in tgt_keys)} added"
                    + (f", -{removed_here} removed" if remove_extra and removed_here else "")
                )
            else:
                save_entry_file(tgt_ef)
                summary["details"].append(
                    f"Updated {en_path.name}: "
                    f"+{sum(1 for e in en_ef.entries if e.get('Key') not in tgt_keys)} added"
                    + (f", -{removed_here} removed" if remove_extra and removed_here else "")
                )
        else:
            summary["files_untouched"] += 1

    return summary


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument(
        "language",
        help="Target language Name (as in Languages.json) or folder name.",
    )
    ap.add_argument(
        "--remove-extra",
        action="store_true",
        help="Remove keys in target files that do not exist in English.",
    )
    ap.add_argument(
        "--dry-run",
        action="store_true",
        help="Report changes without writing any files.",
    )
    args = ap.parse_args(argv)

    folder = args.language
    lang = get_language(args.language)
    if lang is not None:
        folder = lang.folder
    else:
        for l in load_languages():
            if l.folder == args.language:
                folder = l.folder
                break

    summary = sync_language(folder, remove_extra=args.remove_extra, dry_run=args.dry_run)

    verb = "would be" if args.dry_run else "were"
    print(f"Sync ({'dry-run' if args.dry_run else 'live'}) for folder: {folder}")
    print(f"  Files {verb} created : {summary['files_created']}")
    print(f"  Keys  {verb} added   : {summary['keys_added']}")
    print(f"  Keys  {verb} removed : {summary['keys_removed']}")
    print(f"  Files untouched      : {summary['files_untouched']}")
    print()
    for d in summary["details"]:
        print(f"  - {d}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
