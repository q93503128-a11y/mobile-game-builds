# Foundation4 Store / Google Play IAP

Start main `0c1ea494cb757b8250f6f4bfdf446a179ac650c1`, fetched before work; local/remote equal and clean. Work in progress, not a completion claim. Real Play products/merchant/API credentials are not configured. Production mutation and new paid cloud resources: none.

## Research before implementation · 2026-10-28

The official Unity5.4 documentation resolves to **IAP5.4.4 (2026-10-02)**. Google release notes report **Billing9.1.0 (2026-06-18)**. The official Maven9.1.0 AAR was downloaded and public signatures inspected with javap before writing the bridge. No guessed API.

|Option/system|Concrete official capability|Lowfire fit|Decision and alternative rejected|
|---|---|---|---|
|[Unity IAP5.4.4](https://docs.unity3d.com/Packages/com.unity.purchasing@5.4/changelog/CHANGELOG.html)|StoreController events, deferred payment versus paid/unfulfilled orders, explicit confirmation, catalog/deployment support|Good cross-store/iOS abstraction; larger surface than current Android-only three-consumable scope|Viable, not selected now. Avoid duplicate catalog/deployment and client confirmation ownership. Keep IStoreBillingClient for future iOS; Unity6 incompatibility is not the reason|
|[Native Billing9.1.0](https://developer.android.com/google/play/billing/release-notes)|ProductDetails, owned/pending queries, obfuscated account/profile IDs, formatted regional prices and offer tokens|Small Java bridge leaves consumption exclusively on backend|Selected. One Maven dependency, no Kotlin extensions. Cost: Lowfire owns connection/callback/recovery maintenance and upgrade testing|
|[UGS tokens](https://docs.unity.com/en-us/cloud-code/scripts/how-to-guides/token-support)|Authenticated player context and service-controlled Cloud Save writes|Reuse Authentication, Economy2 CAS/archive/global receipt|Separate purchase orchestration from Google/RSA adapter to respect128KB script limit. No second currency database or client verified bool|
|[Developer API v2](https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.productsv2)|Package/token query, product/quantity, account/profile binding, PURCHASED/PENDING/CANCELLED, consumption/test state|Verify actual Google response against frozen server intent|Adopt. Play Integrity is separate app/build trust, not payment proof|
|[Consumable lifecycle](https://developer.android.com/google/play/billing/integrate)|Verify→deliver→consume; backend consumption recommended; pending grants nothing; acknowledgement deadline|Private pending receipt plus same-CAS wallet/intent grant, then server consume|Adopt. No client consumption or frame polling. Unresolved purchases require monitored recovery before three-day acknowledgement deadline|
|[Security](https://developer.android.com/google/play/billing/security)|Token is financial identity; verify obfuscated binding and deduplicate permanently|Domain-separated account hash(project/environment/player) and intent profile hash|Adopt both. OperationId differs from token identity. Unknown already-consumed token cannot mint Gem|
|[RTDN](https://developer.android.com/google/play/billing/lifecycle/one-time)|Notification is a hint; query Google again for authoritative state|Pub/Sub not configured; no arbitrary cloud billing resource creation|Hosted RTDN deferred. Implement server recovery/operations adapters and release monitoring procedure; do not claim a running receiver|
|[Voided Purchases](https://developers.google.com/android-publisher/voided-purchases)|Revoked/refunded/chargeback feed,30-day lookback, pagination; refund without revoke may not appear|Daily overlapping scan and case-backed reconciliation|CLI/framework, not a running scheduler. Reclaim available Gem; shortfall UnderReview. No negative wallet, automatic ban, Engineer revoke or future free-grant offset|

## Seven-game public product research

Current US Apple publisher listings are real price examples, **not Android/KRW quotations or complete live catalogs**. Six published store images were freshly downloaded and actually opened. URLs/hashes/age limitations are in reference-images.json; external art stays outside Assets/Git. Some screens are historical. Current official notes take precedence. Login-only loading/payment/success flows were not exercised and are unknown. This is public-material research, not a live checkout audit. Native Lowfire before screenshots were freshly rendered in isolated Unity.

|Game/source|System and actual example|Advantage|Lowfire difference|Apply?|Reason|
|---|---|---|---|---|---|
|[Mine Survival](https://apps.apple.com/us/app/mine-survival/id1222898586)|Public list has16Ruby/$0.99,100/$4.99,240/$9.99,500/$19.99, at least4currency tiers|Small entry; displayed amount/value progression|Also paid modes; current Store screen not captured|Small entry only|No mode sales or copied Ruby valuation. Do not claim full live package count|
|[Last Day on Earth](https://apps.apple.com/us/app/last-day-on-earth-survival/id1241932094), [official webshop image](https://kefirgames.com/dev-blog/last-day-on-earth?id=webshop)|Current Pile of Coins/$1.99, Tin/$9.99. Official illustration separates Money from Season/Equipment/Settlement|Currency category and prices under contents|Many passes/items/sale tags; Lowfire sells only Gem|Category clarity|Reject gear bundles, scarcity and currency-to-world delivery|
|[Westland](https://apps.apple.com/us/app/westland-survival-cowboy-rpg/id1339238576), [official Hub](https://westlandhub.com/en/news/welcome-to-westland-hub)|Current specials/$0.99–19.99. Official illustrated100nuggets/$19.99,250+50/$49.99,500+250/$99.99|Clear amount/bonus/price|Large packs, VIP/points, much larger sink demand|Amount and price clarity|No whale anchor/points loop. Illustration prices are not current Android quotes|
|[Survivor.io](https://apps.apple.com/us/app/survivor-io/id1528941310)|Current Some Gems/$0.99, Pile/$4.99. Historical screen shows80/500/1200, first2×, bottom Shop, crates|Dedicated currency section and consistent cards|Portrait/gacha/energy and first-bonus state|Comparable cards|Reject first2× and crate/timer pressure. Historical screen is not current price proof|
|[Archero](https://apps.apple.com/us/app/archero/id1453651052)|Current Pile/$0.99, Heap/$4.99, first-purchase pack/$1.99. Historical image has6tiers80→14000/$0.99→99.99|Readable quantities/prices and separate Shop tab|Much larger sinks and first-purchase state|Simple tiers|No huge pack/first bonus; weak Lowfire demand and added refund complexity|
|[Brawl Stars](https://apps.apple.com/us/app/brawl-stars/id1229016807), [official bonus basis](https://support.supercell.com/supercell-store/en/articles/information-about-discounts-and-offers-of-virtual-items-in-brawl-stars-store.html)|Current30/$1.99,80/$4.99,170/$9.99,360/$19.99. Historical image six Gem tiers, distinct coin packs/Home|Quantity first, explicit close and cost currency; official bonus basis30/$1.99|Mature content/cosmetic sinks and web bonus channel|Hierarchy and obvious Back|No Popular/Best Value or region-wide percentage promise without platform-price proof|
|[Clash of Clans](https://apps.apple.com/us/app/clash-of-clans/id529479190), [Oct15,2026 UI note](https://supercell.com/en/games/clashofclans/blog/news/a-word-on-the-new-home-village-screen/)|Current Pile/$4.99, Bag/$9.99, Sack/$19.99, Pocketful/$0.99. Old Treasure image shows big cards/X. Current note separates buildings from Store and reports erroneous FREE badge|Core play separate from monetization, meaningful badges|Old screenshot partly superseded; huge building sinks|Play-first/no false badge|Do not infer current checkout from old image or copy large catalog|

|Published screen|Entry/category/close actually visible|Card hierarchy|Loading/success/help limits|
|---|---|---|---|
|LDoE|Money separate from Season/gear; illustration close cropped|4full cards plus partial edge, contents then price|No interactive loading/success observed|
|Westland|3card illustration, not full navigation|100/300/750nuggets; explicit20/50%bonus on larger cards|Public site has cart/contact/legal text; payment not exercised|
|Survivor|Shop bottom-left and gameplay tabs|3visible currency tiers below crates|Historical screen only; payment states unknown|
|Archero|Shop bottom-left, map/play separate|6tiers with explicit first labels|Historical screen only; first-bonus labels observed, not current backend logic|
|Brawl|Resources category, prominent Home|6Gem and4coin tiers with distinct costs|Logged-out official Store has Support/legal; offers need login|
|Clash|Treasure, Gems/Resources, visible X|5visible horizontal cards; partial edge suggests scroll|Latest official UI note governs current entry; loading/success unknown|

## Decisions after research, before code/catalog

- Native Billing9.1.0 Java bridge plus IStoreBillingClient; no dual IAP SDK. Server-only consumption with same authenticated-player verifier, separate from app/build Integrity.
- **Proposed/unregistered** IDs: lowfire_gems_24 / lowfire_gems_72 / lowfire_gems_144, granting24/72/144. Proposed USD setup tiers0.99/2.99/5.99 are owner planning, never runtime price strings. Play supplies formatted price/currency. No first bonus or value badge. Nominal quantity ratio1:3:6; rounding makes per-dollar value slightly unequal, so no exact global flat-rate/discount claim.
- Maximum144 covers Engineer+3revives or18revives, rather than thousands without useful sinks. Small24=3revives,72=9. Sink insufficiency after Engineer is explicit; no new sink or existing price/Class balance changes. Engineer remains utility/build/maintenance, but cash accelerates unlock/revive: do not claim zero gameplay advantage.
- Subordinate Store entry near Profile Gem balance, existing journal paper/font/Gem icon. Three equal cards, no default expensive selection, detail before Google confirmation, obvious Back, no auto popup/countdown/fake sale. Alpha explains unavailable and cannot initialize real billing. QA fixture is isolated and labelled in developer evidence, not shipped as Google success.
- Frozen intent→Google v2 verification→Private pending receipt/global token-hash owner→Economy2 CAS ledger+grant marker→server consume→complete and remove pending raw token. Settlement survives disabling new sales. Tokens only in Private server pending storage and scoped client journal; never ledger/player reply/UI/log. Permanent hash receipts survive archive. No world-save version change.
- Refund: reclaim min(balance,grant), record unrecovered shortfall for support review and block new paid starts only. No negative balance/future free-source offset/automatic ban/Engineer revoke. Permanent reversal source prevents duplicate clawback.
- Hosted RTDN deferred; daily overlapping Voided scan and pending monitor are mandatory release operations, not already running. Deletion disposition requires explicit reviewed owner retention-policy reference; no guessed statutory retention period. Public Privacy/Deletion URL, audience/Families and Data Safety review remain blockers.

Implementation, economic model and actual/fixture/static outcomes follow as performed. Research completed before source/catalog implementation.

## Economy model — desired budget versus actual nonnegative wallet

Assumptions are not telemetry: claimed achievements60/120 by30/90days, newcomer45 once, monthly40/month, optional ad1/day, one pack total, Engineer120 once, desired4/12 revives at8. Lifetime achievements213 remain unchanged. Negative desired surplus means an unaffordable revive, never negative wallet.

|Profile|Days|Free incl optional ad|Paid once|Desired spending|Desired surplus/deficit|Affordable revives after Engineer|Actual feasible closing|
|---|---:|---:|---:|---:|---:|---:|---:|
|No ads / no purchase|30|145|0|152|-7|3|1|
|Ads / no purchase|30|175|0|152|23|4|23|
|Small buyer (one24 pack)|30|175|24|152|47|4|47|
|Medium buyer (one72 pack)|30|175|72|152|95|4|95|
|Large buyer (one144 pack)|30|175|144|152|167|4|167|
|No ads / no purchase|90|285|0|216|69|12|69|
|Ads / no purchase|90|375|0|216|159|12|159|
|Small buyer (one24 pack)|90|375|24|216|183|12|183|
|Medium buyer (one72 pack)|90|375|72|216|231|12|231|
|Large buyer (one144 pack)|90|375|144|216|303|12|303|

No-ad30day assumed145 cannot buy Engineer plus4revives(152):3revives and1Gem remain. By90days no-ad assumed285 covers Engineer+12revives and69remain. Ads/purchases accelerate rather than create exclusive access. Post-Engineer repeat sink is weak;144max and no promotion/new sink. Repair/turret/overclock/overrepair and extra SP attempts are gameplay advantage, not zero-P2W. No free supply/cost/Class combat redesign.

## User-requested checkpoint, not Foundation4 completion

Implementation/verification state, interrupted AAB and exact remaining work are in [RESUME](RESUME.md) and checkpoint-status.json. Backend full164 passed before final small guards; final relevant38 passed after guards. Edit9/Play8, data, native compile, Android59/59 and rendered KO/EN viewport matrix passed. Final added deletion regression and actual UGS anonymous negative QA remain unexecuted because ILPP startup stalled and user requested pause. One AAB attempt was cancelled by user pause during native compilation after24m46s; no AAB success or actual Google payment claimed. The original single-final-build constraint must be reconciled before another attempt. Development/Alpha default-OFF publication of seven scripts each is verified; Production untouched. Live source sync not performed. Real save fingerprints unchanged. External Play/Google/privacy/operations/device setup in IAP.md remains required.

Actual native screenshots prove rendered layout, while money/prices/success in them are isolated fixtures. Seven public games/six images are research, not live checkout QA. Direct images checked: ready EN1920, history KO1280, pending KO844 and long-price EN844. No asset copying from references. Initial clipped status was rejected and corrected. Fixture fulfillment currently covers24Gem only;72/144 production amounts covered by server tests, fixture extension remains listed in RESUME.

No Audio/Foundation5 started. All45 requested review subjects are covered by research tables, IAP.md architecture/catalog/states/account/security/consumption/recovery/refund/RTDN/Voided/support/privacy/channel setup, economy model/P2W, measured evidence and RESUME remaining/modified/version/save/build/timing boundaries. End SHA is the checkpoint commit containing RESUME, not a final completion SHA.
