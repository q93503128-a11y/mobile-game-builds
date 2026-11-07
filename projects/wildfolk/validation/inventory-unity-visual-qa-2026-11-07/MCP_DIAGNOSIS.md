# Unity MCP and Editor Diagnosis

## Current resumed session — verified identity and capture

- Read mcpforunity://instances, mcpforunity://project/info, mcpforunity://editor/state, and mcpforunity://custom-tools before acting.
- Connected instance: client@114cecc6a8d1b6b9, Unity 6000.3.19f1.
- project/info returned C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical/projects/wildfolk/client, exactly the requested canonical project. The stale C:\UnityProjects\Wildfolk copy was not modified or used for evidence.
- Editor focus remained false in editor/state. No Windows input, Unity Focus, or Unity Show call was used.
- Play Mode was used only for the isolated visual QA profile. SceneFlow.Repository.Path pointed to a unique AppData/Local/Temp/Lowfire-Phase4B-QA-... path; autosave was disabled. Screenshots were captured through UnityEngine.ScreenCapture.CaptureScreenshot while the actual Unity Game View rendered at the target dimensions.
- The 13 retained PNGs were read back and confirmed as native 844×390, 1280×720, or 1920×1080 files. Inspection used the raw images at original pixel dimensions.
- Final Console MCP query: 0 error/warning entries. The changed C# source compiled before live visual checks. The seven already-passing Phase 4A PlayMode tests were not rerun.

## Capture and recovery attempts

- An initial manage_camera capture routed through a world camera and omitted the Screen Space Overlay inventory UI, so that image was rejected as inventory evidence. Direct ScreenCapture.CaptureScreenshot from the running Game View captured the overlays and became the accepted path.
- A source hot reload while Play Mode was active triggered a transient domain reload. During that unstable interval Unity logged 12 NullReferenceExceptions across gameplay/UI/audio tickers. The QA run was stopped, the canonical project refreshed in Edit Mode, and a fresh isolated world was started. Those transient logs were cleared after being recorded; the later fresh Play session had no remaining Console errors/warnings. They are not hidden as product defects because they were observed during an unstable editor reload, but the Editor hot-reload path is documented as a QA hazard.
- Two exploratory in-memory MCP code snippets failed to compile due to incorrect QA type/member names; no project source was created by them. After correcting the queries, capture and state inspection succeeded.
- During QA, the Unity importer rewrote third-party .meta metadata in the local worktree. Those unrelated metadata changes are excluded from the commit and restored to the fetched main baseline after stopping Play Mode.
- A previous checkpoint stale-project finding remains in the historical section of README.md; it was accurate before the user reopened the canonical project. It is superseded by this successful exact-path check.

## Outcome and remaining platform check

Actual Game View capture and visual inspection succeeded for the listed screens. The equipment label overlap was corrected and re-captured at 1280×720 and 1920×1080. Runtime equipment selection was corrected and checked live at 844×390. Android device safe area, physical touch dimensions, and on-device input remain unverified and require a separate device pass.
