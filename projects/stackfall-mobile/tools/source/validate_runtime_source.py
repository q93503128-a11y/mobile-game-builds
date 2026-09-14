#!/usr/bin/env python3
"""Fail fast on runtime-source hygiene regressions before Unity import."""

from __future__ import annotations

import re
import sys
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[2]
RUNTIME_ROOT = PROJECT_ROOT / "client" / "Assets" / "Scripts" / "Runtime"

RULES: tuple[tuple[str, re.Pattern[str]], ...] = (
    ("temporary marker", re.compile(r"\b(TODO|FIXME|HACK|WIP)\b", re.IGNORECASE)),
    ("runtime debug logging", re.compile(r"\bDebug\.Log(?:Warning|Error|Exception)?\s*\(")),
    ("scene-wide name lookup", re.compile(r"\b(?:GameObject\.)?Find\s*\(")),
    ("legacy object lookup", re.compile(r"\bFindObjectOfType\s*<")),
    ("direct runtime instantiate", re.compile(r"\bInstantiate\s*\(")),
    (
        "player-facing development wording",
        re.compile(r'"[^"\n]*(?:개발\s*중|미구현|테스트용|디버그|WIP|placeholder)[^"\n]*"', re.IGNORECASE),
    ),
)


def main() -> int:
    if not RUNTIME_ROOT.exists():
        print(f"runtime source root missing: {RUNTIME_ROOT}", file=sys.stderr)
        return 2

    failures: list[str] = []
    files = sorted(RUNTIME_ROOT.rglob("*.cs"))
    if not files:
        print("no runtime C# sources found", file=sys.stderr)
        return 2

    for path in files:
        text = path.read_text(encoding="utf-8")
        for line_number, line in enumerate(text.splitlines(), start=1):
            for rule_name, pattern in RULES:
                if pattern.search(line):
                    relative = path.relative_to(PROJECT_ROOT)
                    failures.append(f"{relative}:{line_number}: {rule_name}: {line.strip()}")

    if failures:
        print("runtime source hygiene validation failed:", file=sys.stderr)
        for failure in failures:
            print(f"- {failure}", file=sys.stderr)
        return 1

    print(f"runtime source hygiene OK ({len(files)} C# files)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
