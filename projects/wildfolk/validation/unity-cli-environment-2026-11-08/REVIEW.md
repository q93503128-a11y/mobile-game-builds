# Unity CLI and Pipeline environment verification

- Date: 2026-11-08
- Unity project: D:\Lowfire-Workspace\mobile-game-builds\projects\wildfolk\client
- Git repository: q93503128-a11y/mobile-game-builds, branch main
- Editor: 6000.3.19f1

## Result

Unity CLI 1.0.0-beta.17 controls the open Unity 6000.3.19f1 Editor through Unity Pipeline 0.10.0-exp.1. The Pipeline package is installed in the canonical project and recognized by the Editor; its HTTP endpoint is reachable on port 7800. The existing CoplayDev MCP package remains installed and its endpoint on port 8080 is also reachable. Both ran concurrently.

The CLI, Editor status query, command discovery, and minimum focused EditMode/PlayMode regression checks passed. No Editor upgrade, gameplay source change, save migration, temporary branch, or Phase 4D work was performed.

## Starting state and preservation

- The Codex saved project root is the repository at D:\Lowfire-Workspace\mobile-game-builds; the Unity project root is its nested client directory above.
- Before package installation, an external recovery snapshot was made under %LOCALAPPDATA%\Temp\Lowfire-Unity-Recovery-20261108-172034. It contains the original Git patch, copies of the pre-existing changed files, ProjectSettings, and relevant scene files.
- At final verification, all 100 pre-existing modified/untracked files matched the recovery copies by SHA-256. The 88 dirty .meta files, DefaultNetworkPrefabs.asset, and 11 untracked files were not staged or committed.
- The .meta changes retain their GUIDs and show Unity importer serialization upgrades and Google Version Handler label/time normalization. Their timing and contents are consistent with Unity reserialization/importer activity; the exact operation that produced them is not recorded.
- DefaultNetworkPrefabs.asset adds 40 network prefab entries. Of 39 unique referenced GUIDs, 22 resolve to existing project prefabs and 17 do not resolve in the current Assets tree. The source of this edit is unknown, so it remains untouched and uncommitted.
- The 11 untracked files are ten Unity-generated IDE project/solution files and ProjectSettings/GvhProjectSettings.xml from External Dependency Manager. They are preserved and uncommitted.
- UserData was absent before and after testing. PlayMode tests used the isolated DevelopmentProfile directory %LOCALAPPDATA%\Temp\Lowfire-Pipeline-Regression-20261108-0858. The temporary SessionState override was erased after tests; SaveRepository.Default() again resolves to the project UserData path.

## Versions and compatibility

| Component | Verified value | Result |
|---|---|---|
| Unity Editor | 6000.3.19f1 (Unity 6.3 LTS) | Existing Editor still running the canonical project |
| Unity CLI | 1.0.0-beta.17, build dated 2026-11-05 | Matches the newest release listed by Unity on 2026-11-08 |
| Unity Pipeline | 0.10.0-exp.1 | Latest version reported by unity pipeline list-versions |
| Pipeline compatibility | Unity documentation requires Editor 6.0 or later | 6000.3.19f1 satisfies the documented minimum; package loaded and served commands in this Editor |

The installed Editor has Android, Android SDK/NDK, and OpenJDK modules. It does not have the iOS module. No Editor version or module upgrade was performed.

## Pipeline installation and project diff

The canonical install used the previously validated package install flow:

    unity pipeline install --project-path D:\Lowfire-Workspace\mobile-game-builds\projects\wildfolk\client --version 0.10.0-exp.1 --json --non-interactive

Unity Package Manager initially needed an Editor refresh/recompile before the server started. After import completed, the package assemblies were present, the Editor advertised Pipeline 0.10.0-exp.1, and the CLI found the server.

Only these project files changed for installation:

- Packages/manifest.json: added direct dependency com.unity.pipeline at 0.10.0-exp.1.
- Packages/packages-lock.json: added the corresponding lock entry. Its declared dependencies are Test Framework 1.1.33, Mono Cecil 1.11.6, UIElements/JSONSerialize/ScreenCapture modules 1.0.0, and Newtonsoft JSON 3.0.2. No other locked package versions changed.

The pre-existing com.coplaydev.unity-mcp Git dependency remains in the manifest. No temporary compatibility code or other project settings were added.

## CLI and Editor verification

Successful commands and observations:

- unity version --format json — reports 1.0.0-beta.17.
- unity editors --installed --format json and unity editors info 6000.3.19f1 --format json — confirm the current installed Editor and version metadata.
- unity pipeline list-versions --format json — reports 0.10.0-exp.1 as latest.
- unity pipeline install ... — success.
- unity status --project-path ... --format json — one ready Editor at the canonical project path, version 6000.3.19f1, PID 26584.
- unity pipeline editors --format json — one Editor with Pipeline 0.10.0-exp.1 and a reachable HTTP endpoint at 127.0.0.1:7800.
- unity command --project-path ... --format json — command groups are listed; the Editor exposes Pipeline commands.
- unity command --project-path ... --format json editor status — success: ready, not compiling, no domain reload pending, PlayMode stopped.
- Read-only console status/get and scene list commands succeeded.

The command catalog includes Editor and PlayMode controls, scene and asset inspection, console access, scripting, and test commands. PlayMode tests must use the asynchronous Pipeline runner because synchronous HTTP requests cannot survive Unity's PlayMode domain reload.

## Scene and Phase 4C regression

The open scene after verification is MainMenu (Assets/Wildfolk/Scenes/MainMenu.unity), active and clean, with six root objects. Build Settings places MainMenu at index 0; Wildfolk and Multiplayer are the gameplay scenes used for Phase 4C inventory layout checks. Phase 4C is a UI scope across those two scenes, not a separate scene. Existing visual/design approval and physical-device checks remain separate.

Post-install focused EditMode run: 4/4 passed.

- FrontendContractTests.AppStartsAtHomeAndGameplayRemainsEnabled
- FrontendContractTests.SaveRulesReferenceGameplayContentAndHomeKeysHaveBothLanguages
- InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout for Wildfolk.unity
- InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout for Multiplayer.unity

Post-install focused PlayMode run: 4/4 passed.

- EcologyPlayTests.ActualPointerArmorDragSwapUnequipAndUse
- GameplayPlayTests.InventoryButtonsOpenCloseAndModalBlocksGameplayInput
- StabilityPlayTests.LongPressSplitHoldsChosenAmountUntilExplicitPlacementOrCancel
- WorldPassPlayTests.SplitHalfViaTouchButtonAndExplicitDropZonePreserveItems

The full 634-test project catalog and Android device/build paths were not run. The PlayMode tests completed asynchronously; a transient endpoint gap during domain reload was followed by completed PASS results from the test status record. No tests were reported as passed without a result.

## Console and Android impact

At the initial Console review there were two errors. At the final live Console ground-truth sample, Unity reported 1 error, 0 warnings, and 7 logs, with compilationFailed=false. The retained Pipeline event buffer also contains historical compile warnings (32 warning entries and 56 logs); those buffer totals are not the current live warning count.

1. Google.IOSResolver.dll cannot resolve UnityEditor.iOS.Extensions.Xcode. The current Editor installation has no iOS module. This blocks the iOS resolver assembly only; it is not an Android runtime or Android build compile error. Android package code was not built in this verification.
2. Assets/Wildfolk/Art/dash-source.svg uses currentColor for stroke, which Unity's SVG importer does not support. No project scene/asset reference was found by source search. The importer issue was not seen in the final live error list; the source remains and could log again if reimported. It does not affect Android C# compilation.

The retained warning entries group into Google Play Games obsolete Editor APIs, AdMob's deprecated age-treatment request properties, and two Editor QA static-analysis warnings (PoiContentQa.cs CS0252 and DeathFoundationQa.cs CS0414). These did not set the Editor compilation-failed flag. The AdMob warnings identify a future API migration point; changing it without reviewing age/consent semantics would be inappropriate.

## CoplayDev MCP and Codex session state

The existing com.coplaydev.unity-mcp package was preserved. A direct JSON-RPC handshake to 127.0.0.1:8080/mcp returned HTTP 200 for initialize and tools/list; server mcp-for-unity-server 3.4.11 advertised 49 tools, including manage_scene, manage_asset, read_console, run_tests, and script management. This confirms that the Coplay server and tool catalog are running alongside Pipeline.

The current Codex conversation exposes no Unity-specific MCP tools in its tool registry. The global Codex configuration has unityMCP enabled with streamable HTTP transport at 127.0.0.1:8080/mcp, so the server URL/configuration is valid. A protocol-level reconnect and tools/list succeeded, but this already-running conversation's native tool registry cannot be refreshed from inside the conversation. Native tool exposure in a new Codex session remains to be checked.

The Codex saved project root is the outer repository and is trusted. The ignored client/.codex/config.toml is below that active working directory, so its local setting enabling unity@unity-agent-plugin is not loaded in this session. The Unity agent plugin is installed but globally disabled. This nested plugin setting is distinct from the globally enabled CoplayDev MCP server configuration.

## Recommended Lowfire tool setup

Use Unity CLI + Pipeline as the default development and verification path: it is scriptable, its commands and JSON status/results are directly verifiable, and the project now runs it on the current Editor. Keep CoplayDev MCP installed and configured for sessions where its native tools are exposed and for its additional MCP workflows. Avoid running concurrent scene mutations through both interfaces. Do not enable another overlapping Unity agent plugin by default.

## Sources

- Unity CLI release notes: https://docs.unity.com/en-us/unity-cli/release-notes
- Unity Pipeline package requirements and install: https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package
- Unity CLI and Pipeline responsibilities: https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-cli-pipeline-package
- Unity CLI as an in-Editor MCP alternative: https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli
- Codex project configuration layers and trust: https://learn.chatgpt.com/docs/config-file/config-basic

## Time log

All timestamps UTC. Work intervals include command and Editor waits.

| Work segment | Interval | Elapsed |
|---|---|---:|
| Baseline, recovery snapshot, scene review, pre-install regression, and Pipeline bring-up | 08:14:44–08:55:42 | 40m 58s |
| Post-install CLI regression, save isolation, and test cleanup | 08:55:42–09:02:02 | 6m 20s |
| Final MCP handshake, preservation audit, and report preparation | 09:02:02–09:08:51 | 6m 49s |

The completion message records the final commit/push interval and total elapsed time through post-push verification.
