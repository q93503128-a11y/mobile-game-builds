# Foundation2.1 최종 보고 · 2026-10-27

1. 시작 SHA: 전체 패스 `b9ce7c6ce63b698b7ec575faae0e3fde611f0fcf`, 재개 `e99436a8561ed9179cf9e879a6210a4dfd5e1f13`. 둘 다 fetch 후 HEAD 일치·main·clean 확인.
2. 최종 SHA: 이 보고서를 포함한 완료 main commit. 최종 사용자 응답에서 push 후 local/remote 전체 SHA를 제공한다. 자기 commit hash를 파일 내용에 순환 삽입하지 않는다.
3. Premium multiplayer: 실제 authenticated owner Issue→authenticated Host Verify, 서버 보관90초 proof. Player/Host/session/admission/profile/operation/revision binding. Host/remote 동일 경계, reconnect 새 admission·reHost 새 session, 입장 gate는 최대20초이며 unavailable은 Survivor·번역 메시지와 접속 유지. bearer credential 전송0.
4. Engineer: 가격120Gem 유지. SP local 기존 계정 cache는 편의용이며 MP Host/remote 모두 실제 서버 ownership 검증. remote XP0/node빈 값, 동일 계정/세션 재접속만 임시XP 보존, account 영구XP에 쓰지 않음. Gold4직업60과 기존 local Host/remote 정책 유지.
5. Ledger: serverSchema2/client snapshot1, hot512부터256행 segment로 compact, hot hard768/root3MiB. Private immutable archive+SHA256 chain/checkpoint,256 shard 개별 source/resolution 영구 index, full SHA256 base64url45자 IDs. Schema1의 잔액/claim/전체 금융행/Engineer증거 보존.
6. Idempotency: achievement/attendance immutable claim ID, 거래 source 영구보존. Ads/IAP는 내부 verified receipt 계약과 전역 owner binding fixture만; 실제SDK/provider verifier/지급endpoint 없음. rawtoken 저장0.
7. Capacity:100k 모델 root193217B/hot416/archive389/CustomItem645/최대bucket450key/전체59006349B. root3MiB와bucket2000key·4MiB에75/90% 운영 경고.30/90/365/1095일×Ads0/3/5/10×IAP월0/1/3의48 projection은 REVIEW 표와 ledger-model.json. 무료무제한/성능SLA 아님.
8. Crash recovery: 확정 prefix archive→재조회검증→index→rootCAS, unknown outcome은 같은prefix 재시도. speculative row archive 금지. 각경계 fault/CAS/응답유실 Node검증, 실제 legacy512행 내용 보존과1745ms 이관 확인. Reserved revive 유지/terminal최근32+영구resolution.
9. Admin/Support: source/transaction/history/capacity/reconcile/maintain, player/support scope. maintain/adjust 기본dry-run, 기존 reason/operator/case/CAS 유지. Engineer.Proof/last premium admission/reason/suspicious 조회. 삭제는 모든환경 private→playerdata→Auth 마지막, resumable operator journal.
10. Anti-cheat: malformed/expired/cross-player/Host/session/revision/unowned 거절, bounded audit/suspicious만 사용·자동ban 없음. active16/90초·mutation500/day·audit64/day. modified Host gameplay/느린offline가짜progress/능동LAN MITM 전체 방어는 범위 밖.
11. Multiplayer QA: 이전 실제2process LAN/Relay+실제 Development anonymous Cloud Code proof 성공(ownership fixture0). 이번 별도 death Host/Client NUnit과 Boss/resource LAN 성공. Gold Medic/Explorer 개발 fixture는 premium 실제서비스 증명과 구분. 실제Google/4인실기기Relay 성공 주장 없음.
12. Tests: Node95/95, Edit97/97, 고유 Play32 통과(이전MCP Play1 포함, 실패했던Explicit Client를 proper Host와 재실행해 해결). 이전Live MCP Edit13 재실행 포함 여부 구분. 실제 HTTP20, 실제SDK13+6, Android Alpha/Production57/57 fixture 제외, source validator1626GUID,10k/50k/100k model 성공. 이미통과한 Node/Edit/premium LAN/Relay는 이번 재개에 불필요 반복하지 않음.
13. Versions: Save14/Profile2/CloudProfile1/generation3 유지. serverEconomy1→2/Protocol22→23은 앞 checkpoint 변경; 이번 재개 추가상승0. client economy snapshot1 유지.
14. Save protection: 실제2파일의 SHA256/mtime/bytes 동일. 격리QA만 사용. Live 사용자 Editor 조작0, Runtime652파일 내용 동일(줄바꿈/기존metadata2개의 마지막newline 차이만 정규화). 이번4개의 Editor/Test QA변경은 canonical/isolated QA에만 반영.
15. Modified files: 전체 구현/검증 목록은 상위 modified-files.json·checkpoint commit, 이번은 completion/modified-files.json. 재개 source는 EconomyQa/BossLanQa/DeathFoundationQa/DeathFoundationPlayTests4개와 docs/evidence만. runtime/backend/config/가격 변경0.
16. Remaining limits: PGS/OAuth/PlayIntegrity/Privacy·삭제URL/실기기/Production 활성화/4인장시간검수 미완료. Backend Enabled=false. 실제Authentication429/Retry-After69초 관측 후 대기·성공; public workload/quota/alert 운영검수 필요. archive운영 비용/한계·provider receipt retention/restore/삭제정책 후속.
17. Foundation3 readiness: archive/영구financial idempotency/verified receipt owner-binding/지원 조회 기반 준비. 실제 Rewarded Ads SDK/SSV/provider callback/ad policy/실기기/개인정보 검증은 구현하지 않음. 사용자 지시 후 별도. IAP/Store/Audio/콘텐츠 자동시작0.
18. Local HEAD: 완료 commit 후 rev-parse로 확인하며 최종 사용자 응답에서 exact SHA 제공.
19. Remote HEAD: push 후 fetch/rev-parse로18과 일치 확인하며 최종 사용자 응답에서 exact SHA 제공.
20. git status: 완료 commit/push 뒤 porcelain empty로 확인. PR/임시branch 없음. main 직접 완료하고 STOP.

시간: 이번 재개 completion/TIMING.md와 timing.json, 이전 checkpoint TIMING.md. 각단계 한줄·벽시계·병렬중복합산 없음. 작업별 token 사용량은 측정 API가 없어 null이며 시간/테스트 수로 추정하지 않았다.
