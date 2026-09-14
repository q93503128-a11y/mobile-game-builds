#!/usr/bin/env python3
"""Stackfall Mobile gacha Monte Carlo simulator.

Pure standard-library utility. It models:
- base S probability
- hard pity
- featured probability on an S pull
- next-S featured guarantee after losing pickup
- fixed monthly/free pull budgets

This tool does not define production economy by itself. The design docs remain authoritative.
"""

from __future__ import annotations

import argparse
import json
import math
import random
from dataclasses import dataclass


@dataclass(frozen=True)
class Config:
    runs: int
    pulls: int
    s_rate: float
    pity: int
    featured_rate: float
    seed: int


def percentile(sorted_values: list[int], q: float) -> float:
    if not sorted_values:
        return 0.0
    if q <= 0:
        return float(sorted_values[0])
    if q >= 1:
        return float(sorted_values[-1])
    pos = (len(sorted_values) - 1) * q
    lo = math.floor(pos)
    hi = math.ceil(pos)
    if lo == hi:
        return float(sorted_values[lo])
    frac = pos - lo
    return sorted_values[lo] * (1 - frac) + sorted_values[hi] * frac


def draw_until_s(rng: random.Random, s_rate: float, pity: int) -> int:
    for pull in range(1, pity + 1):
        if pull == pity or rng.random() < s_rate:
            return pull
    raise RuntimeError("unreachable")


def simulate(config: Config) -> dict:
    rng = random.Random(config.seed)

    s_waits: list[int] = []
    featured_waits: list[int] = []
    s_counts: list[int] = []

    for _ in range(config.runs):
        first = draw_until_s(rng, config.s_rate, config.pity)
        s_waits.append(first)

        featured_total = first
        if rng.random() >= config.featured_rate:
            featured_total += draw_until_s(rng, config.s_rate, config.pity)
        featured_waits.append(featured_total)

        since_s = 0
        s_count = 0
        for _pull in range(config.pulls):
            since_s += 1
            if since_s == config.pity or rng.random() < config.s_rate:
                s_count += 1
                since_s = 0
        s_counts.append(s_count)

    s_waits.sort()
    featured_waits.sort()
    s_counts.sort()

    def mean(values: list[int]) -> float:
        return sum(values) / len(values)

    return {
        "config": {
            "runs": config.runs,
            "pull_budget": config.pulls,
            "s_rate": config.s_rate,
            "hard_pity": config.pity,
            "featured_rate_given_s": config.featured_rate,
            "featured_guarantee_after_loss": True,
            "seed": config.seed,
        },
        "s_wait": {
            "mean": mean(s_waits),
            "median": percentile(s_waits, 0.50),
            "p90": percentile(s_waits, 0.90),
            "p95": percentile(s_waits, 0.95),
            "max": s_waits[-1],
            "chance_before_pity": sum(v < config.pity for v in s_waits) / config.runs,
        },
        "featured_wait": {
            "mean": mean(featured_waits),
            "median": percentile(featured_waits, 0.50),
            "p75": percentile(featured_waits, 0.75),
            "p90": percentile(featured_waits, 0.90),
            "p95": percentile(featured_waits, 0.95),
            "max": featured_waits[-1],
            "chance_within_50": sum(v <= 50 for v in featured_waits) / config.runs,
            "chance_within_75": sum(v <= 75 for v in featured_waits) / config.runs,
            "chance_within_100": sum(v <= 100 for v in featured_waits) / config.runs,
        },
        "fixed_budget": {
            "pulls": config.pulls,
            "mean_s": mean(s_counts),
            "chance_at_least_1_s": sum(v >= 1 for v in s_counts) / config.runs,
            "chance_at_least_2_s": sum(v >= 2 for v in s_counts) / config.runs,
            "median_s": percentile(s_counts, 0.50),
        },
    }


def parse_args() -> Config:
    parser = argparse.ArgumentParser()
    parser.add_argument("--runs", type=int, default=200_000)
    parser.add_argument("--pulls", type=int, default=60)
    parser.add_argument("--s-rate", type=float, default=0.015)
    parser.add_argument("--pity", type=int, default=50)
    parser.add_argument("--featured-rate", type=float, default=0.60)
    parser.add_argument("--seed", type=int, default=20260914)
    args = parser.parse_args()

    if args.runs <= 0 or args.pulls < 0 or args.pity <= 0:
        parser.error("runs/pity must be positive and pulls must be non-negative")
    if not 0 < args.s_rate <= 1:
        parser.error("s-rate must be in (0, 1]")
    if not 0 <= args.featured_rate <= 1:
        parser.error("featured-rate must be in [0, 1]")

    return Config(
        runs=args.runs,
        pulls=args.pulls,
        s_rate=args.s_rate,
        pity=args.pity,
        featured_rate=args.featured_rate,
        seed=args.seed,
    )


if __name__ == "__main__":
    print(json.dumps(simulate(parse_args()), ensure_ascii=False, indent=2))
