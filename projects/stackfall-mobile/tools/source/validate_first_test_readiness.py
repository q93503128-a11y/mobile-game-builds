#!/usr/bin/env python3
"""Static preflight for the first Unity test gate.

This does not claim Unity import/compile or Play Mode success. It only verifies
source/package invariants that can be checked without the Unity Editor.
"""

from __future__ import annotations

import json
import re
import sys
from collections import Counter
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[2]
CLIENT_ROOT = PROJECT_ROOT / "client"
RUNTIME_ROOT = CLIENT_ROOT / "Assets" / "Scripts" / "Runtime"
UI_ROOT = RUNTIME_ROOT / "UI"
RESOURCE_ROOT = CLIENT_ROOT / "Assets" / "Resources" / "StackfallExternal"
EDITOR_BOOTSTRAP = CLIENT_ROOT / "Assets" / "Editor" / "StackfallProjectBootstrapEditor.cs"

EXPECTED_UNITY = "6000.3.19f1"
EXPECTED_REVISION = "7689f4515d75"
EXPECTED_ICONS = {
    "chat.png",
    "gear.png",
    "gift.png",
    "lock.png",
    "market.png",
    "news.png",
    "pouch.png",
    "trophy.png",
}
REQUIRED_SHELL_SOURCES = {
    "StackfallShellController.cs",
    "StackfallShellInteractionState.cs",
    "StackfallShellInteractions.cs",
    "StackfallShellLayout.cs",
    "StackfallShellMetaScreens.cs",
    "StackfallShellResultScreens.cs",
    "StackfallShellServiceScreens.cs",
    "StackfallShellStartup.cs",
    "StackfallShellWidgets.cs",
}


def fail(message: str, failures: list[str]) -> None:
    failures.append(message)


def read(path: Path) -> str:
    if not path.exists():
        return ""
    return path.read_text(encoding="utf-8")


def main() -> int:
    failures: list[str] = []

    manifest_file = CLIENT_ROOT / "Packages" / "manifest.json"
    try:
        manifest = json.loads(read(manifest_file))
    except json.JSONDecodeError as exc:
        fail(f"invalid Packages/manifest.json: {exc}", failures)
        manifest = {}
    dependencies = manifest.get("dependencies", {}) if isinstance(manifest, dict) else {}
    if dependencies.get("com.unity.modules.uielements") != "1.0.0":
        fail("Unity UIElements built-in module dependency is missing", failures)

    version_file = CLIENT_ROOT / "ProjectSettings" / "ProjectVersion.txt"
    version_text = read(version_file)
    if f"m_EditorVersion: {EXPECTED_UNITY}" not in version_text:
        fail(f"Unity editor version must be {EXPECTED_UNITY}", failures)
    if EXPECTED_REVISION not in version_text:
        fail(f"Unity changeset must include {EXPECTED_REVISION}", failures)

    editor_bootstrap_text = read(EDITOR_BOOTSTRAP)
    editor_bootstrap_fragments = (
        'private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";',
        "[InitializeOnLoadMethod]",
        "EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive)",
        "EditorBuildSettings.scenes = remaining.ToArray();",
    )
    for fragment in editor_bootstrap_fragments:
        if fragment not in editor_bootstrap_text:
            fail(f"editor bootstrap invariant missing: {fragment}", failures)

    existing_sources = {path.name for path in UI_ROOT.glob("*.cs")}
    missing_sources = sorted(REQUIRED_SHELL_SOURCES - existing_sources)
    if missing_sources:
        fail(f"missing shell sources: {', '.join(missing_sources)}", failures)

    app_text = read(RUNTIME_ROOT / "App" / "StackfallAppController.cs")
    required_app_fragments = (
        "public const int PlayableStageCap = 2;",
        "_shell.ShowStartup();",
        "StartCoroutine(FinishStartup())",
        "player.AddComponent<CorePulseWeapon>();",
        "player.AddComponent<PulseBladeWeapon>();",
        "player.AddComponent<GravityWellWeapon>();",
    )
    for fragment in required_app_fragments:
        if fragment not in app_text:
            fail(f"app invariant missing: {fragment}", failures)

    layout_text = read(UI_ROOT / "StackfallShellLayout.cs")
    if "Screen.width <= 390 || Screen.height <= 700" not in layout_text:
        fail("compact-device layout gate is missing", failures)
    if "BadgeText(_state.UnclaimedMailCount)" not in layout_text:
        fail("top-bar mail badge is not state-driven", failures)
    if "var panelScale = 1080f / width;" not in layout_text:
        fail("shell safe-area mapping is not aligned to width-match panel scale", failures)

    controller_text = read(UI_ROOT / "StackfallShellController.cs")
    state_driven_fragments = (
        "_state.HasAttendanceReward",
        "_state.ClaimableMissionCount",
        "_state.UnclaimedMailCount",
        "_state.HasFreeDailySupply",
    )
    for fragment in state_driven_fragments:
        if fragment not in controller_text:
            fail(f"home notification state missing: {fragment}", failures)
    panel_fragments = (
        "_panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;",
        "_panelSettings.match = 0f;",
        "_panelSettings.sortingOrder = 50f;",
        'SelectableTab("메인", _state.ActivePreset == 0',
    )
    for fragment in panel_fragments:
        if fragment not in controller_text:
            fail(f"shell runtime UI invariant missing: {fragment}", failures)

    hud_text = read(UI_ROOT / "PlayerHudController.cs")
    hud_fragments = (
        "_panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;",
        "_panelSettings.match = 0f;",
        "_panelSettings.sortingOrder = 100f;",
        "ApplySafeArea(root);",
    )
    for fragment in hud_fragments:
        if fragment not in hud_text:
            fail(f"combat HUD runtime UI invariant missing: {fragment}", failures)

    png_signature = b"\x89PNG\r\n\x1a\n"
    existing_icons = {path.name for path in RESOURCE_ROOT.glob("*.png")}
    missing_icons = sorted(EXPECTED_ICONS - existing_icons)
    if missing_icons:
        fail(f"missing external UI icons: {', '.join(missing_icons)}", failures)
    for icon_name in sorted(EXPECTED_ICONS & existing_icons):
        data = (RESOURCE_ROOT / icon_name).read_bytes()
        if not data.startswith(png_signature):
            fail(f"invalid PNG signature: {icon_name}", failures)

    home_doc = read(PROJECT_ROOT / "docs" / "HOME_HANGAR_UI.md")
    headings = re.findall(r"^##\s+(.+)$", home_doc, flags=re.MULTILINE)
    duplicates = sorted(name for name, count in Counter(headings).items() if count > 1)
    if duplicates:
        fail(f"duplicate HOME_HANGAR_UI sections: {', '.join(duplicates)}", failures)
    if "360×640" not in home_doc:
        fail("360×640 readability requirement missing from UI canon", failures)

    road_map = read(PROJECT_ROOT / "docs" / "ROADMAP.md")
    if "첫 사용자 테스트 게이트" not in road_map:
        fail("first user test gate is missing from roadmap", failures)
    if "Unity 6000.3.19f1 import/compile 오류 0" not in road_map:
        fail("Unity import/compile gate is missing from roadmap", failures)

    if failures:
        print("first-test source preflight failed:", file=sys.stderr)
        for item in failures:
            print(f"- {item}", file=sys.stderr)
        return 1

    print(
        "first-test source preflight OK "
        f"(Unity {EXPECTED_UNITY}, {len(REQUIRED_SHELL_SOURCES)} shell sources, "
        f"{len(EXPECTED_ICONS)} external icons, UIElements module + editor bootstrap + panel safety)"
    )
    print("Unity Editor import/compile and Play Mode verification are still required.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
