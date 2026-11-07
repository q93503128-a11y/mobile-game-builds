# Lowfire Phase 4B — Unity Inventory Visual QA

## Result

**CAPTURE BLOCKED / VISUAL QA UNVERIFIED.** No Phase 4A Unity Game View PNG was captured in this pass. No static mockup or previous-phase image is substituted for a current Unity render. No game code, scene, save, or project setting was changed, and the seven passing Phase 4A PlayMode tests were not rerun.

## Source and project identity

- `mobile-game-builds` main at start: `2904becc94215f82bbf116cf50463f15139ec855`
- `dev-playbook` main at start: `e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7`
- Unity Editor process: Unity `6000.3.19f1`; window title identifies `MainMenu`.
- The running Editor project path is `C:\UnityProjects\Wildfolk`; active scene is `MainMenu`, Play Mode is off, and the Editor remains unfocused.
- That project copy does not contain Phase 4A: its `InventoryUiBuilder.cs` is 13,311 bytes and lacks the Phase 4A markers; its inventory art directory has zero `inventory_*.png` files. The canonical source copy is `C:\Users\User\Documents\ChatGPT\문승준 모바일\canonical\projects\wildfolk\client`, where the builder is 32,024 bytes and all five pilot PNGs exist.

## MCP and capture findings

- Unity MCP is configured in Codex for `http://127.0.0.1:8080/mcp`.
- The Unity Editor log reports the local server ready at `127.0.0.1:8080` and `Session connected`; the port is listening locally.
- Unity MCP tools and resources became available during this pass. The Editor state, project, instances, windows, and custom tools resources were read; the Unity Console returned zero error/warning entries. The connected instance is still the stale `C:\UnityProjects\Wildfolk` project.
- The Unity skill documents `manage_camera(action="screenshot", capture_source="game_view", include_image=True)` as the Game View capture path. The Phase 2B report confirms 18 actual Unity MCP captures with `editor_is_focused=false`, but does not record the exact tool call. The existing `VisualGameView.Show(width,height)` helper sets a Game View resolution and uses `PlayUnfocused` without focusing the Editor.
- A read-only Computer Use fallback could not read the Unity window: its accessibility lookup failed, and the screenshot fallback returned the Codex window. No input, Focus, or Show call was used.

## Required next step

To open the correct project manually, use Unity Hub's **Projects → Add → Add project from disk** and select `C:\Users\User\Documents\ChatGPT\문승준 모바일\canonical\projects\wildfolk\client`. Leave the current editor unfocused if desired; no GUI focus change is needed for this QA. Once it loads, the MCP project path must report that exact `client` directory. If both editors are listed, select the canonical instance through the available Unity MCP instance selector before capturing.

The user selected restarting Codex and opening the canonical Unity project. Unity tools have now appeared, but the connected instance still reports the stale project path. After the canonical project is open, verify the connected instance and project path before capturing. Do not capture from the stale `C:\UnityProjects\Wildfolk` copy as evidence for Phase 4A.

The seven Phase 4A PlayMode tests remain valid evidence for their functional scope; they were not repeated. Actual visual states, resolutions, language variants, Android touch sizing, and current Play Mode remain unverified. See [CAPTURE_INDEX.md](CAPTURE_INDEX.md), [MCP_DIAGNOSIS.md](MCP_DIAGNOSIS.md), and [screenshots/README.md](screenshots/README.md).
