#!/usr/bin/env python3
"""Stackfall Mobile static-data validator.

Dependency-free by design so CI and local development can run it before Unity import.
The validator is intentionally conservative: it reports structural mistakes that are
safe to detect without embedding gameplay balance decisions in code.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable

ID_PATTERN = re.compile(r"^[a-z0-9]+(?:[._][a-z0-9]+)*$")
PROBABILITY_KEYS = {"rate", "probability", "chance", "sRate", "epicRate", "featuredWeight"}
NON_NEGATIVE_HINTS = (
    "hp",
    "damage",
    "cost",
    "reward",
    "amount",
    "duration",
    "speed",
    "radius",
    "weight",
    "limit",
    "power",
)
REFERENCE_SUFFIXES = ("Id", "Ids")


class ValidationError(Exception):
    pass


def iter_json_files(root: Path) -> Iterable[Path]:
    for path in sorted(root.rglob("*.json")):
        if path.is_file():
            yield path


def load_json(path: Path) -> Any:
    try:
        with path.open("r", encoding="utf-8") as handle:
            return json.load(handle)
    except json.JSONDecodeError as exc:
        raise ValidationError(
            f"{path}: JSON parse error at line {exc.lineno}, column {exc.colno}: {exc.msg}"
        ) from exc


def walk(value: Any, path: str = "$") -> Iterable[tuple[str, Any]]:
    yield path, value
    if isinstance(value, dict):
        for key, child in value.items():
            yield from walk(child, f"{path}.{key}")
    elif isinstance(value, list):
        for index, child in enumerate(value):
            yield from walk(child, f"{path}[{index}]")


def validate_ids(document: Any, source: Path, seen: dict[str, Path], errors: list[str]) -> None:
    for node_path, value in walk(document):
        if not isinstance(value, dict):
            continue
        item_id = value.get("id")
        if item_id is None:
            continue
        if not isinstance(item_id, str) or not item_id:
            errors.append(f"{source}:{node_path}.id must be a non-empty string")
            continue
        if not ID_PATTERN.fullmatch(item_id):
            errors.append(f"{source}:{node_path}.id has invalid format: {item_id!r}")
            continue
        previous = seen.get(item_id)
        if previous is not None:
            errors.append(f"duplicate id {item_id!r}: {previous} and {source}")
        else:
            seen[item_id] = source


def validate_numbers(document: Any, source: Path, errors: list[str]) -> None:
    for node_path, value in walk(document):
        if not isinstance(value, dict):
            continue
        for key, child in value.items():
            if isinstance(child, bool) or not isinstance(child, (int, float)):
                continue
            key_lower = key.lower()
            if any(hint in key_lower for hint in NON_NEGATIVE_HINTS) and child < 0:
                errors.append(f"{source}:{node_path}.{key} must not be negative: {child}")
            if key in PROBABILITY_KEYS or key_lower.endswith(("rate", "chance", "probability")):
                if child < 0:
                    errors.append(f"{source}:{node_path}.{key} probability must be >= 0: {child}")
                if child > 100 and key_lower not in {"weight", "featuredweight"}:
                    errors.append(f"{source}:{node_path}.{key} probability exceeds 100: {child}")


def validate_non_empty_arrays(document: Any, source: Path, errors: list[str]) -> None:
    required_non_empty = {"entries", "memberPartIds", "rankEffects", "phaseThresholds"}
    for node_path, value in walk(document):
        if not isinstance(value, dict):
            continue
        for key in required_non_empty:
            if key in value and isinstance(value[key], list) and not value[key]:
                errors.append(f"{source}:{node_path}.{key} must not be empty")


def collect_references(document: Any) -> list[tuple[str, str]]:
    refs: list[tuple[str, str]] = []
    for node_path, value in walk(document):
        if not isinstance(value, dict):
            continue
        for key, child in value.items():
            if key == "id" or not key.endswith(REFERENCE_SUFFIXES):
                continue
            field_path = f"{node_path}.{key}"
            if key.endswith("Ids"):
                if isinstance(child, list):
                    refs.extend((field_path, item) for item in child if isinstance(item, str) and item)
            elif isinstance(child, str) and child:
                refs.append((field_path, child))
    return refs


def validate_probability_entries(document: Any, source: Path, errors: list[str]) -> None:
    for node_path, value in walk(document):
        if not isinstance(value, dict):
            continue
        entries = value.get("entries")
        if not isinstance(entries, list) or not entries:
            continue
        rates = []
        for entry in entries:
            if not isinstance(entry, dict):
                rates = []
                break
            rate = entry.get("rate")
            if rate is None or isinstance(rate, bool) or not isinstance(rate, (int, float)):
                rates = []
                break
            rates.append(float(rate))
        if rates:
            total = sum(rates)
            if not (abs(total - 1.0) <= 1e-6 or abs(total - 100.0) <= 1e-6):
                errors.append(
                    f"{source}:{node_path}.entries probability sum must be 1.0 or 100.0; got {total}"
                )


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Stackfall Mobile static JSON data")
    parser.add_argument(
        "root",
        nargs="?",
        default=str(Path(__file__).resolve().parents[2] / "data"),
        help="static-data root directory",
    )
    args = parser.parse_args()

    root = Path(args.root).resolve()
    if not root.exists():
        print(f"ERROR: data root does not exist: {root}", file=sys.stderr)
        return 2

    files = list(iter_json_files(root))
    if not files:
        print(f"OK: no JSON data files yet under {root}")
        return 0

    documents: list[tuple[Path, Any]] = []
    errors: list[str] = []
    seen_ids: dict[str, Path] = {}

    for source in files:
        try:
            document = load_json(source)
        except ValidationError as exc:
            errors.append(str(exc))
            continue
        documents.append((source, document))
        validate_ids(document, source, seen_ids, errors)
        validate_numbers(document, source, errors)
        validate_non_empty_arrays(document, source, errors)
        validate_probability_entries(document, source, errors)

    known_ids = set(seen_ids)
    for source, document in documents:
        for field_path, ref in collect_references(document):
            # External/service IDs may be explicitly prefixed and are not static-data refs.
            if ref.startswith(("store.", "server.", "external.")):
                continue
            if ref not in known_ids:
                errors.append(f"{source}:{field_path} references unknown id {ref!r}")

    if errors:
        print("Static-data validation failed:", file=sys.stderr)
        for error in errors:
            print(f"- {error}", file=sys.stderr)
        return 1

    print(f"OK: validated {len(files)} JSON files and {len(known_ids)} unique ids")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
