#!/usr/bin/env python3
"""Stackfall Mobile target-part / long-term progression Monte Carlo.

Internal balance tool. This file is not player-facing content.
"""

from __future__ import annotations

import argparse
import random
from dataclasses import dataclass
from statistics import mean, median


@dataclass(frozen=True)
class Config:
    s_rate: float = 0.015
    s_pity: int = 50
    featured_rate: float = 0.60
    target_within_featured: float = 0.50
    calibration_needed: int = 3


@dataclass
class State:
    since_s: int = 0
    featured_guaranteed: bool = False
    calibration: int = 0
    target_copies: int = 0
    s_count: int = 0


def pull_once(rng: random.Random, cfg: Config, state: State) -> None:
    state.since_s += 1
    is_s = state.since_s >= cfg.s_pity or rng.random() < cfg.s_rate
    if not is_s:
        return

    state.since_s = 0
    state.s_count += 1

    if state.featured_guaranteed:
        featured = True
        state.featured_guaranteed = False
    else:
        featured = rng.random() < cfg.featured_rate
        if not featured:
            state.featured_guaranteed = True

    got_target = featured and rng.random() < cfg.target_within_featured
    if got_target:
        state.target_copies += 1
        return

    state.calibration += 1
    if state.calibration >= cfg.calibration_needed:
        state.calibration -= cfg.calibration_needed
        state.target_copies += 1


def pulls_until_target_copies(rng: random.Random, cfg: Config, copies: int) -> tuple[int, int]:
    state = State()
    pulls = 0
    while state.target_copies < copies:
        pulls += 1
        pull_once(rng, cfg, state)
    return pulls, state.s_count


def copies_with_budget(rng: random.Random, cfg: Config, pulls: int) -> tuple[int, int]:
    state = State()
    for _ in range(pulls):
        pull_once(rng, cfg, state)
    return state.target_copies, state.s_count


def percentile(values: list[int], q: float) -> int:
    ordered = sorted(values)
    index = max(0, min(len(ordered) - 1, int(q * len(ordered)) - 1))
    return ordered[index]


def run_trials(cfg: Config, trials: int, copies: int, seed: int) -> None:
    rng = random.Random(seed)
    pulls: list[int] = []
    s_counts: list[int] = []

    for _ in range(trials):
        p, s = pulls_until_target_copies(rng, cfg, copies)
        pulls.append(p)
        s_counts.append(s)

    print(f"target copies: {copies}")
    print(f"trials: {trials:,}")
    print(f"mean pulls: {mean(pulls):.2f}")
    print(f"median pulls: {median(pulls):.0f}")
    print(f"p90 pulls: {percentile(pulls, 0.90)}")
    print(f"p95 pulls: {percentile(pulls, 0.95)}")
    print(f"p99 pulls: {percentile(pulls, 0.99)}")
    print(f"mean S count: {mean(s_counts):.2f}")
    print(f"max observed pulls: {max(pulls)}")


def run_budget(cfg: Config, trials: int, budget: int, seed: int) -> None:
    rng = random.Random(seed)
    copies: list[int] = []
    s_counts: list[int] = []

    for _ in range(trials):
        c, s = copies_with_budget(rng, cfg, budget)
        copies.append(c)
        s_counts.append(s)

    print(f"pull budget: {budget}")
    print(f"trials: {trials:,}")
    print(f"mean target copies: {mean(copies):.3f}")
    print(f"median target copies: {median(copies):.0f}")
    print(f"P(at least 1 target): {sum(c >= 1 for c in copies) / trials:.4%}")
    print(f"P(at least 3 targets): {sum(c >= 3 for c in copies) / trials:.4%}")
    print(f"P(at least 9 targets): {sum(c >= 9 for c in copies) / trials:.4%}")
    print(f"mean S count: {mean(s_counts):.3f}")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--trials", type=int, default=200_000)
    parser.add_argument("--seed", type=int, default=20260914)
    parser.add_argument("--copies", type=int, default=1)
    parser.add_argument("--budget", type=int)
    parser.add_argument("--s-rate", type=float, default=0.015)
    parser.add_argument("--pity", type=int, default=50)
    parser.add_argument("--featured-rate", type=float, default=0.60)
    parser.add_argument("--target-rate", type=float, default=0.50)
    parser.add_argument("--calibration", type=int, default=3)
    args = parser.parse_args()

    cfg = Config(
        s_rate=args.s_rate,
        s_pity=args.pity,
        featured_rate=args.featured_rate,
        target_within_featured=args.target_rate,
        calibration_needed=args.calibration,
    )

    if args.budget is not None:
        run_budget(cfg, args.trials, args.budget, args.seed)
    else:
        run_trials(cfg, args.trials, args.copies, args.seed)


if __name__ == "__main__":
    main()
