# Foundation4 최종 결과 — 저장소 구현·관련 검증 완료 / 실제 구매 OFF

완료 기록 UTC: 2026-10-29T00:25:13.950936+00:00. 사용자 중단 체크포인트79ae04c에서 재개했다. 이 파일을 추가하는 main 커밋이 최종 전달 커밋이며, 정확한 SHA와 remote equality/clean은 최종 응답으로 확인한다. 자체 커밋 SHA는 동일 파일 내용에 자기참조로 저장하지 않는다. `git log --diff-filter=A --format=%H -- projects/wildfolk/validation/store-iap/completion/FINAL_REPORT.md`로 유일하게 찾을 수 있다.

1. 시작 SHA: 최초 `0c1ea494cb757b8250f6f4bfdf446a179ac650c1`. 재개 시 `79ae04ccb827e4ee78e24ea777b86c31e2917681`을 fetch 후 local/remote 동일·clean 확인.
2. 최종 SHA: 이 FINAL_REPORT를 추가하는 main 커밋. 위 Git 조회와 최종 응답 참조. Native/player source snapshot hash는 player-source-snapshot.json/native-source-freeze.json에 별도 기록.
3. Billing: native Java Billing9.1.0 + Android C# callback queue. Android consumable3개와 서버 consume 소유권을 명확히 하기 위해 선택. UnityIAP5.4.4는 가능한 대안이며 iOS seam은 IStoreBillingClient로 유지. SDK 유지보수 부담을 수용했다.
4. Products: `lowfire_gems_24`→24, `lowfire_gems_72`→72, `lowfire_gems_144`→144. 제안 USD0.99/2.99/5.99는 소유자 설정 기획이며 Play Console 미등록/Proposed=true. 실제 UI 가격은 ProductDetails 현지 가격만 사용, fallback 없음.
5. Store UX: Play 우선 MainMenu6개 유지, Profile 하위 진입. 동등3카드·선택/상세/Google 확인·Back·미완료 확인·기록·문의. loading/unavailable/account/pending/검증/소비/완료/취소/실패 KO/EN. 1920×1080/1280×720/844×390 native 렌더 검수, 첫 status clipping은 거절 후 교정.
6. Purchase: 연결계정/서버 build 신뢰/정책 확인→불변 상품·수량·binding intent→Google 구매, PENDING 무지급→Googlev2 서버 검증→Economy2 원장 CAS 지급→서버 consume/ack→완료. 한 호출에 Google단계1개로 제한한다.
7. Security: 서버 Authentication 재조회, Google account/profile binding, quantity1/고정 package/product/TestOnly 검증. token hash 전역 owner와 영구 source로 replay 방지. client 가격/Gem/verified bool은 지급 권위가 아니다. raw token은 Private pending/client scoped journal에만 임시 보관, terminal 제거, UI/log/feedback/ledger 미노출.
8. Economy2: 기존 Protected root CAS/영구 source/Private archive 사용. 잔액 음수 불가, 기존 무료 보상·Engineer120·SP revive8 유지. 새 wallet/통화/world 배송 없음.
9. Recovery: grant/consume/completion 응답 유실·앱 재시작·로컬 큐 유실/재설치·다른 device operation 모델과 archive dedup 테스트. 실제 Google 실기기 재설치/다기기 증거는 아직 없다.
10. Refund: 실제 검증된 취소/Voided만 영구 reversal로 회수. min(balance,grant), 부족분 review 및 새 구매 차단. 음수/자동ban/Engineer 회수/미래 무료Gem 차감 없음. 0회수도 검증된 reversal marker로 재처리 방지.
11. Account: 실제 UGS Google Play Games 연결계정만, 익명/오프라인/conflict/deletion/다른환경 거부. 실제 UGS 익명5경로 차단 PASS, fresh QA identity는3환경 데이터가 비어 있음을 확인한 후 삭제.
12. Alpha/Production: Alpha 실제 구매·license 구매 금지. Production client/backend/live OFF. Development/Alpha7script 배포 완료, 재개 시 두 purchase script hash와 OFF policy 재확인. Production policy/script/data 활성화 없음.
13. Economy: 업적 lifetime213/신규45/월40/선택광고1일1 유지. 모델30/90일 무광고 무료145/285는 활동 가정이며 실측 아님. Engineer+원하는4/12부활 예산152/216;30일은3부활+1Gem,90일12부활+69Gem. 선택광고+30/+90와 1회24/72/144 구매를 분리 계산. 무광고 신규45+월간2회80=125로 Engineer 가능.
14. P2W/sink: Engineer utility/수리·turret/overclock·overrepair 및 SP부활의 현금 가속은 실제 플레이 이점, zero-P2W 주장 없음. Engineer는1회 sink, 반복 revive 수요가 약하므로 최대144만 제안. 새 sink/프로모션/밸런스 변경 없음.
15. Admin/Support: 안전한 lookup/history/ref/phase/duplicate 조회, recover/Voided/resolve-review는 operator/case·기본dry-run·명시apply. public grant/refund API 없음. daily scan/pending monitor 운영과 RTDN은 아래 미설정 경계.
16. Privacy/Data Safety: purchase history/identifiers/third-party/목적·대상연령 검토, 실제 Privacy·삭제 URL 필요. 카드정보는 앱 수집 대상이 아니다. 금융 삭제는 owner policy/ReviewAt를 먼저 검토하고 PlayerId/token/order 없는 최소 hash tombstone을 보존. 법정 보존기간을 임의로 만들지 않았다. secret은 Dashboard/Secret Manager, Git/채팅 설정 금지.
17. Tests: final Node166/166, unchanged Edit9/9, latest Play10/10, data validator PASS, current Android Alpha59/Production59 assembly와 fixture exclusion PASS. Java/Maven/manifest standalone PASS. Node TAP 출력의 cp949 wrapper 오류는 Node exit0/166PASS 이후 출력만 실패했으며 재실행해 성공으로 꾸미지 않았다.
18. AAB: 총2회 시작,1개 완료. 첫 회는 사용자 pause로 native compile 중 중단(24분46.48초), 실패 빌드로 기록하지 않았다. 재개 성공 UTC2026-10-28T23:57:02.769719+00:00→2026-10-29T00:21:08.557511+00:00, 1445.79초. 이유: 같은 최종 Billing native gate를 보존한 cache로 완료. UI/docs/backend 때문에 다시 빌드하지 않았다. 실제 AAB **65,241,833bytes**. Unity BuildReport totalSize1,557,136,379는 실제 .aab 파일 크기가 아니다. 구조/개발서명/ARM64/IL2CPP/Billing9.1 permission·DEX/기존Ads/embeddedOFF 설정·621ZIP entry 자격증명 패턴 검사 PASS. artifact는 Git 밖 `C:/UnityProjects/LowfireAndroidQa/iap-results/Lowfire-development.aab`, SHA256 `d3eefe32316ef01abc2636b80d9aac3d5600704096fddc8d431582fa7782d769`.
19. Actual/fixture: 실제 UGS 익명 거부5경로·native compilation/렌더·local live MCP 격리Play는 실제 수행. 금전 성공·refund·device 모델은 fixture. 실제 Google 결제/consume/refund/Android 기기 실행은 미검증이다. 개발 AAB는 Production release signoff가 아니다.
20. Versions: Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3 유지. IAP metadata/journal/receipt Schema1만 별도 추가.
21. Save protection: 실제2파일 SHA256/mtime/bytes 전후 동일. MCP live Play는 임시 QA 경로, 종료 후 기존 QA 설정 복구. MainMenu clean/edit mode, Console error0. Windows 입력/Focus/Show 없음.
22. External setup: merchant/실제SKU·지역가격/API service account·UGS secret/PGS·OAuth·Integrity·실제서명·license tester·실기기/Privacy·삭제 URL/대상연령·금융보존/운영모니터가 남는다. [IAP12단계](../../../docs/IAP.md#external-activation-procedure--setup_required), SETUP_REQUIRED/RELEASE_BLOCKERS 참조. test track만으로 실제 청구를 방지하지 못한다.
23. Modified files: canonical native bridge/StoreService·journal/UI·catalog/서버 verifier·receipt·operations/테스트·문서·evidence. 재개 source 변경은 fixture mapping과 regression뿐. 전체 목록 modified-files.txt. PR/branch/patch/override 없음.
24. Limits: hosted RTDN/PubSub/상시daily Voided·pending alert 배포 없음. 실제 Google 거래·환불/물리기기/출시서명·배포/법률·개인정보 확정 없음. 기존 Ads/Backend 외부 gate도 남아 있다.
25. Next: 사용자 별도 지시로 Audio/최종polish 착수 가능. 공개 출시는 기존/새 외부 gate 해결 후. Foundation4에서 STOP, 자동 Foundation5 없음.
26. Local HEAD: 최종 main 커밋 후 확인하여 최종 응답에 정확한 SHA 보고.
27. Remote main: push 후 ls-remote 값과 local HEAD 일치 확인하여 최종 응답에 보고.
28. Git status: 최종 commit/push 이후 clean 확인. 이 문서가 그 확인 전 작성됐다는 순서를 유지한다.

## 측정 시간

아래 wall time에는 도구 시작/대기/컴파일이 포함되며 작업들이 겹친다. 합산하거나 사용자 pause 시간을 작업시간에 더하지 않는다. 전체 측정 이벤트는 ../timing.json, 개별 QA는 ../process-history.json과 이 디렉터리 JSON을 따른다. 작업별 token usage는 제공되지 않아 **null**이다.

|단계|시작 UTC|종료 UTC|경과 초|
|---|---|---|---:|
|최신 서버166검사|2026-10-29T00:02:16.552250+00:00|2026-10-29T00:02:35.990080+00:00|19.44|
|최신 Play10검사|2026-10-28T23:54:46.983892+00:00|2026-10-28T23:56:24.440961+00:00|97.46|
|실제 UGS 익명 차단|2026-10-28T23:56:39.206803+00:00|2026-10-28T23:57:38.856613+00:00|59.65|
|Android Alpha/Production compile|2026-10-29T00:01:37.647071+00:00|2026-10-29T00:15:54.863247+00:00|857.22|
|AAB 사용자 중단|2026-10-28T12:22:24.938922+00:00|2026-10-28T12:47:11.414149+00:00|1486.48|
|AAB 재개 성공|2026-10-28T23:57:02.769719+00:00|2026-10-29T00:21:08.557511+00:00|1445.79|

외부 공식 근거·7게임 비교·경제모델은 [REVIEW](../REVIEW.md),45개 검토항목은 review-subjects.json에 유지한다. 세부 미설정을 완료로 표시하지 않는다.
