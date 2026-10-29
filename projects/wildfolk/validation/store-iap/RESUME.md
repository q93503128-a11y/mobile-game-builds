# Foundation4 — repository work complete / real billing OFF

2026-10-29: resumed work below is complete. Latest result: [completion/FINAL_REPORT](completion/FINAL_REPORT.md), [REVIEW final45subjects](REVIEW.md#2026-10-29-resumed-completion--repository-work-complete-real-sales-off). The historical checkpoint below records the interruption, not current unfinished work. No automatic Audio/Foundation5. Remaining work is real Play/Google/privacy/monitor/device setup in IAP.md and RELEASE_BLOCKERS; no activation occurred.

---

# Foundation4 checkpoint — not complete

User requested pause at 2026-10-28T12:49:47.552116+00:00. Work stays on main; no branch/PR. Starting main `0c1ea494cb757b8250f6f4bfdf446a179ac650c1`. The commit containing this file is the checkpoint (`git log --all --grep="Checkpoint Foundation4"`); final completion SHA does not exist yet.

## Completed and measured

- Official current technical research + seven games/6 actually opened public reference images; research decisions preceded code. Native Billing9.1.0 chosen, proposed24/72/144Gem, no actual Play registration or runtime fallback price. REVIEW/IAP contain comparison tables,30/90day model and P2W/sink limits.
- Canonical client/native/server/Store UI/operations foundation implemented OFF. Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3 unchanged; IAP/journal/receipt Schema1 separate.
- Full backend164/164 passed before the last economy-enabled/review guards; final related38/38 passed afterwards (IAP32+operations3+runtime3). Evidence: backend-tests.tap and backend-final-related.tap/json. Do not call this a final full166-test run.
- EditMode9 and related PlayMode8 passed; data validator and KO/EN1920×1080,1280×720,844×390 rendered state matrix passed. Direct PNG review caught status outside paper in the first render; corrected and re-rendered. Initial rejected evidence retained, accepted visual/ images are fixtures labelled no real payment.
- Final Android Alpha59/Production59 assembly scripts compile, fixture exclusion and standalone Billing9.1 Java/Gradle compile passed. Native API signature inspection and source freeze recorded. These are not actual Google/device tests.
- Development/Alpha seven scripts published, downloaded code hashes match local bundles; iap_config purchase/live/settlement/review OFF, Production untouched. An initial deployment helper mistakenly named LowfireMultiplayer, stopped before that nonexistent file, then corrected to LowfirePremiumAdmission; final seven-per-environment publication succeeded.
- User save fingerprint/hash/mtime/size unchanged (2 real files). Local Unity edit mode/Play OFF confirmed. Preflight found no conflicts, but live source/scene sync has NOT been performed. MCP namespace unavailable; no Windows input/Focus/Show used.

## Interrupted work / next steps

1. Fetch remote main first; read AGENTS/README/IAP/REVIEW/this file; inspect actual source and checkpoint diff. No new design/monetization/content/Audio. Do not re-run helpers with baseline-saving side effects.
2. The one final AAB attempt started 2026-10-28T12:22:24.938922+00:00, stopped 2026-10-28T12:47:11.414149+00:00, elapsed 1486.48s (24m46s), **user pause while native compilation still active**, no completed AAB or build failure observed. Preserve warm Q1 cache. Do not silently count the interrupted attempt as success or launch another expensive full build. Original request permits only one final full AAB; a continuation must explicitly reconcile that constraint with this user-requested interruption before a further attempt. Actual output path: `C:/UnityProjects/LowfireAndroidQa/iap-results`. Original artifacts/log/cache remain outside Git. AAB source snapshot precedes the latest small client journal cleanup changes; native freeze remains unchanged.
3. Latest Play regression adds deletion of a previous-session purchase journal. Two cheap Q3 QA launches stalled on IL post-processor IPC startup (not assertion failures); first stopped to recover IPC, second stopped by user pause. Latest StoreQa actual-negative method and deletion test were authored but did not finish compiling/running. Retry only related cheap QA after diagnosing orphan ILPP/pipe/resource contention. Do not touch the live editor/Hub or indiscriminately kill Unity processes. Final UI8-state service tests previously passed8; current suite should have9.
4. DevelopmentStoreTransport currently models24Gem verification only (chooses first catalog product); existing UI/service success QA purchases24 only. Correct its fixture-only mapping before claiming72/144 fixture fulfillment; production verifier already uses frozen canonical product amount. A shell quoting attempt to make this fix failed before changing the file. No false Google-success claim.
5. Run authored `StoreQa.ActualBatch` on Q3 with isolated Development settings: actual UGS anonymous GetState/Begin/Verify/Recover/Consume must reject purchase_account_required before Google. It records fresh player-id before calls. It has NOT been executed; no QA identity cleanup pending. If executed later, inspect that exact new ID's empty data in every real environment before deleting Authentication. Existing ads_cleanup.py references an OLD task ID — do not run it.
6. Audit final operations/deletion/recovery and necessary related tests; catalog mirror/bundle128KB checks. Existing global creation_gate intentionally remains locked on unknown write outcomes; monitor/release only after confirmed stopped invocation, never blind overwrite.
7. Current live Unity source was not synced. User confirmed edit mode for this task; on later resumed task read MCP state if available or obtain current state. Compare each changed file against current live/baseline before copy; protect unrelated live files and all UserData. Live preflight JSON is evidence from this task, not a future guarantee. External MainMenu change may prompt Reload; no unauthorized UI input.
8. Finish actual/fixture matrix, final source/secret/diff/save audit and final report28items, complete commit/push/remote equality/clean. Existing canonical docs state implementation and explicit external gates; checkpoint is NOT Foundation4 complete. No Production or paid cloud resource activation. Do not begin Foundation5 automatically.

## Local helpers and artifacts

Helpers outside Git in workspace parent: iap_work.py/iap_run.py/iap_deploy.py/iap_bundle.py/iap_docs.py/iap_finish_code.py/iap_live_audit.py/iap_checkpoint.py. Many are one-shot mutating generators, not idempotent. Read before execution. iap_bundle.py guard sees existing aab-runs.json; do not remove the guard/evidence to force a repeat. Q1 `C:/UnityProjects/LowfireAndroidQa/tests/artifacts/unity`; Q3 `C:/UnityProjects/LowfireRewardedQa/tests/artifacts/unity`. Generated managed binaries moved to `C:/UnityProjects/LowfireAndroidQa/iap-checkpoint-artifacts`, not committed.

Timing events/process measurements retained; overlaps must not be summed. Task-specific token usage not provided by tools, **null**. No secret/credential or real save contents committed.
