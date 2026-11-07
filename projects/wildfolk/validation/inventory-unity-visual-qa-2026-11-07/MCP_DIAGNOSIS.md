# Unity MCP and Editor Diagnosis

## Verified environment facts

- The running Editor is Unity `6000.3.19f1`; its window title is `Wildfolk - MainMenu`.
- `Editor.log` identifies the open project as `C:\UnityProjects\Wildfolk`.
- The installed package is `com.coplaydev.unity-mcp`. The log reports the local HTTP server ready at `http://127.0.0.1:8080` and a connected session. The local TCP listener is present.
- Codex's existing `unityMCP` configuration points to `http://127.0.0.1:8080/mcp`. No config change was needed or made.
- Unity MCP tools/resources became visible during this pass. Read-only editor state and Console queries succeeded, confirming the MCP connection is now callable from Codex.
- The current project's Inventory UI source is stale relative to main: local `InventoryUiBuilder.cs` is 13,311 bytes versus 32,024 bytes in the canonical project. The five Phase 4A `inventory_*.png` assets are absent from the open project and present in the canonical project.
- The Editor log scan found no matching recent compiler-error or exception entries. This does not establish that the Unity Console is empty.

## Capture path evidence

The Unity MCP skill documents a Game View capture through `manage_camera` with `capture_source="game_view"` and `include_image=True`. Phase 2B's committed report records 18 Unity Game View PNGs and says the Editor remained unfocused; it does not record the exact MCP function call. The project helper `VisualGameView.Show(width,height)` changes the Game View size and sets `PlayUnfocused`; it does not bring the Editor forward.

Before the tools appeared, a read-only Computer Use capture attempt could not read Unity and returned the Codex window instead. After MCP connected, editor state and Console became readable, but `manage_camera` was not called because the active project is still the stale copy and would not show Phase 4A. No mouse, keyboard, Focus, or Show action was performed.

## Recovery path

Manual recovery: in Unity Hub, choose **Projects → Add → Add project from disk**, then select `C:\Users\User\Documents\ChatGPT\문승준 모바일\canonical\projects\wildfolk\client`. If Unity MCP reports multiple editor instances, select the instance whose project info points to this canonical directory; do not accept a screenshot until that identity check passes.

The user selected restarting Codex and opening the canonical Unity project. Unity tools are now registered, but the connected project is still `C:\UnityProjects\Wildfolk`. Once the project path changes to the canonical copy, use the existing `VisualGameView` helper to select each requested resolution and capture the actual Game View with Unity MCP while the Editor stays unfocused. Confirm the Editor is connected to the canonical project before accepting any screenshots. Do not repeat the 640×480 batch capture or the timed-out hidden Editor screenshot path.
