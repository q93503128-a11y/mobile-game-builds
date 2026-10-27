# Launch Economy Foundation2.1 · Premium MP / Ledger lifecycle

2026-10-27 시작 main `b9ce7c6ce63b698b7ec575faae0e3fde611f0fcf`를 fetch하고 local 일치·clean 확인. 제품 backend Enabled=false와 실제 Google/PGS/Integrity/공개URL 미설정은 유지한다. 기존 데이터/코드/문서가 정본이며 원장과 원격 premium admission만 변경한다. Ads/IAP/Store/Audio/콘텐츠/가격 변경은 없다. 사용자 요청으로 최종 회귀 전 checkpoint에서 중단한다. Foundation2.1 전체 완료 선언이 아니다.

## 구현 전 공식 자료와 선택

| 항목 | 실제 공식 근거 | 현재 문제 | 선택 | 배제한 대안과 이유 |
|---|---|---|---|---|
| Cloud Save 크기 | [Game Data](https://docs.unity.com/en-us/cloud-save/concepts/game-data): Private는 서버만 읽기/쓰기, Custom Item별2000key/5MiB, Custom Item 수 제한 없음 | Player Protected 총5MiB에 모든 원장/부활 기록을 유지하면 행 cap만 늘려도 실패 | 사용자·환경별 Private archive segment 및 SHA256 source index shard, Player root는 balance/최근행/checkpoint | 같은 Player에 key만 추가: 총용량 제한 동일. 오래된 행 삭제: 금융 증거/중복 방지 손실 |
| 원자성과 복구 | [write locks](https://docs.unity.com/en-us/cloud-save/concepts/write-locks): 항목별 낙관적 writeLock, 여러 key 원자성 없음 | archive쓰기와 root truncate의 중간 실패 | 확정된 prefix만 immutable archive→검증→source index CAS→root CAS. root의 checkpoint/count가 authoritative index. speculative 새 거래는 archive하지 않음 | 먼저 truncate: 손실. 원격 multi-key atomic transaction 주장: API가 제공하지 않음 |
| 실행 비용 | [Cloud Code script cost](https://docs.unity.com/en-us/cloud-code/scripts/reference/cost):15초 제한, 명령 batch와 최소 응답 권장 | archive 때 수백 개 직렬 key조회는 timeout 위험 | 한 invocation에서 한 segment만, index16 shard를 bounded parallel4·CAS3retry. 실제 서비스에서 제한 확인 | 전체 archive login download/매 frame lookup/무한 retry 금지 |
| Premium 증명 | [NGO approval](https://docs-multiplayer.unity3d.com/netcode/1.4.0/basics/connection-approval/):ConnectionData는 인증/암호화되지 않으며 token 전송 금지 권고. 설치된 NGO2.13의 named messaging API를 기준으로 구현 | local participant GUID/owned bool은 PGS PlayerId 증명이 아님 | 인증 Client가 session/Host/connection admission에 bound된 서버 snapshot을 발급, 인증 Host가 동일 backend에서 다시 조회. NGO에는 bearer token/secret을 보내지 않음 | PlayerId만 trusted lookup: 다른 계정 ID 도용. 새 JWT/서명키: 필요 없는 key배포/회전 체계. snapshot JSON만 믿기: client 위조 가능 |
| 연결 경계 | 현재 Relay는 DTLS, LAN은 UTP. 둘 다 NetworkSession/ClassAdmissionLocks를 사용 | transport별 별도 bool 신뢰 경계는 우회 가능 | spawn 이전 공통 challenge/검증 gate, 매 reconnect nonce 재검증. timeout/unavailable은 Survivor와 짧은 번역 상태 | Relay만 검증/LAN cached owned 허용: 일관된 권위 아님. 게임 전체 접속 거절은 불필요 |

서버 증명은 느린 modified Host gameplay 위조를 막는 anti-cheat나 LAN의 능동 MITM 방어가 아니다. bearer credential을 네트워크 payload에 싣지 않는다. 실제 Google 성공/공개Production 검증은 외부 설정 전 주장하지 않는다. 코드/실제 serialized 크기/수치/검증 결과와 최종 한계는 아래 후속 기록으로 추가한다.

## 색인 비용 검토 후 확정

초기16개 map 후보는10만건에서 shard전체를 거래마다 읽게 되어 배제했다. 최종 source SHA256 앞2hex의256 Custom Item에 각각 독립 immutable receipt key를 사용한다. lookup은 해당key 하나만 읽고 archive row 하나를 대조한다. segment256행은 최대256회의 작은 index write를 concurrency16으로 처리하고, 성공 응답 뒤에만 root를 compact한다. 초기 gate는 이 immutable committed-prefix 쓰기에 필요하지 않으며 새 글로벌 provider receipt의 소유자 bind에만 기존 creation gate를 쓴다. Custom Item당2000key/5MiB, 운영 hard2000key/4MiB와75/90% 경고를 적용하고 root에256개 이하 bucket 통계만 둔다. 실제 요청 수/시간과 serialized byte는 후속 model·서비스 측정으로 기록한다.

## 현재 정본과 검증 · 중간 완료 checkpoint

초기 조사 표의16-map/concurrency4는 당시 후보 기록이다. 최종256 shard 개별 immutable key/concurrency16과 root CAS를 아래 실제 결과로 검증했다. 과거 Foundation1/1.5/2 REVIEW는 수정하지 않았다.

|검토 항목|최종 선택/구체적 근거|검증 또는 한계|
|---|---|---|
|증명/위조/replay|authenticated owner Issue + authenticated Host Verify, 서버 저장 proof,90초|bool/local JSON/receipt claim 거절; bearer payload 없음; modified Host anti-cheat 아님|
|session binding|PlayerId/HostPlayerId/session/admission/profile/Engineer/operation/revision|wrong player/session/admission/proof 실제 HTTP 거절, expiry/revision 등 Node fixture|
|Host/remote|둘 다 같은 ownership 경계; remoteXP0/nodeempty|actual LAN/Relay Host 자기 Engineer와 remote 모두 성공|
|reconnect/새계정|새 admission마다 verify; 같은계정+세션 XP8 유지; reHost 새 sessionXP0|actual LAN/Relay; 새 미소유 계정 localOwned 위조도 Survivor|
|Gold/offline|Gold 가격·정책 유지, unavailable Survivor+번역 상태; 영구선택 보존|MCP Play1/1, 실제 offline Join 성공|
|비용/latency|admission 때만2 Cloud Code,20초 join gate,16 active/90sec,500 mutation/day|입장 전체 process 시간은 cloud latency 아님; 실기기/동시부하 미측정|
|지원/의심|last admission/result/reason/revisions, audit64/day, suspicious bounded, 자동ban 없음|실제 reject와 Node fixture; 운영 alert 전달 job 미구현|
|Schema1→2|잔액/전체행/claim 보존, Engineer.Proof 복사|실제 legacy512→archive256/hot256, 모든 금융행 content hash 동일|
|archive/hot|immutable256행, trigger512/hard768,root3MiB, 이전8192 점진31회|legacy8192 Node 테스트 성공; 실제512 maintenance1745ms|
|index/조회|256 bucket, SHA256 full base64url45자, source→seq→segment|actual SDK50자 경계 내,100k모델중복 및 fullchain reconcile|
|hash/충돌|정렬 JSON HashVersion1; full scope/source 검증|실제 Cloud Save key 순서 변화 문제 수정; SHA256 cryptographic collision 잔여 가정|
|financial/gameplay|금융 source 영구, bounded gameplay operation/audit; claim identity불변|achievement/attendance archive후 중복 방지 Node fixture|
|Ads/IAP 준비|내부 verified receipt contract+global owner binding; rawtoken금지|duplicate/cross-player fixture만; 실제 provider/SDK/restore/refund·retention 정책 없음|
|crash/CAS|확정 prefix archive→검증→index→rootCAS; unknown은 same prefix retry|각 경계 injected fault/응답유실 Node검증; 원격 multi-key atomic 주장 없음|
|revive|Reserved 유지, terminal최근32+영구resolution|archive후 중복 spend/refund 방지 Node검증; 새Schema2 SDK revive회귀는 다음|
|admin|history/source/transaction/capacity/reconcile/maintain, 기본dry-run|actual source/reconcile/Engineerprojection 성공; 긴삭제 비용 증가|
|privacy/delete|private archive/index 먼저, playerdata→Auth 마지막|이번 isolated QA 정리는 qa-cleanup.json 결과 참조; 실제사용자save untouched|
|capacity|serialized bytes, root/index75/90% warning/critical, hardfailclosed|아래10k/50k/100k 및30/90/365/1095 model; 서비스비용/SLA 아님|

### 실행 증거

* Node backend95/95: server-tests.tap. 실제 서비스20/20: actual-service.json. 실제 archive 원본내용 보존: actual-archive.json.
* Hidden EditMode97/97: edit.xml. Hidden broad PlayMode30개 중29통과,1실패: play.xml. 실패는 Explicit `DeathFoundationPlayTests.ActualSeparateLanClientLifecycle`을 ready Host 없이 직접 filter에 포함한 precondition 오류다. 이를 전체 통과로 보고하지 않는다. 해당 수동2process death 회귀는 다음에 정확한 Host orchestration으로 실행한다.
* 실제 LAN 두 Unity 프로세스+실제 Development Cloud proof: LocalNetwork-process.json, 두 process exit0,138.919초. 실제 Relay: Internet-process.json, 두 process exit0,164.224초. anonymous이고 Google이 아니다. ownership fixture를 사용하지 않았다.
* Relay 앞선 실행의 마지막 QA phase 파일 sharing violation은 atomic replace/shared-read로 수정했다. 첫 compile/API/QA route 오류와 서비스 ID길이/hash순서 오류를 prior-attempts/service-retry 등에 보존한다. 이후 성공 결과로 구분한다.
* source-validator.txt: Unity source/landscape/GUID1626/sprite/text PASS. 사용자 Live Unity MCP EditMode13/13, PlayMode1/1: live-mcp-edit/play.json. offline cached Engineer의 Host fallback/저장선택 보존을 실제 Play로 확인했다. source25개는 baseline 비교·backup 후 반영했고 Focus/Show/Windows입력은 사용하지 않았다.
* Unity6000.3.19f1, 실제 SDK/Android 최종 compile·SDK 경제 회귀는 **미실행**이다. 사용자 중단 요청 후 새 검증 묶음을 시작하지 않았다.

### 용량 모델과 운영 비용

|금융행|hot|segment|root bytes|archive+index+root bytes|Private Custom Items|최대bucket keys|
|---:|---:|---:|---:|---:|---:|---:|
|10000|272|38|135750|5832068|294|54|
|50000|336|194|162173|29465175|450|234|
|100000|416|389|193217|59006349|645|450|

실제100k fixture 평균 row387.007B/최대390B이며 provider overhead/가격은 제외한다. Custom Item당2000key/5MiB, 운영4MiB이며 item 수 무제한이 무료 저장/무제한 안전성을 뜻하지 않는다. index lookup은key1+segment1;256행 archive는 source256writes+segmentwrite/read+rootCAS,concurrency16이다. 정상 Get의 maintenance root read 추가 비용이 있으며 전체 원장 login download는 없다. history/reconcile은 bounded cursor, 삭제 작업은 전체 private key 수에 비례한다. 실제512행 서버1745ms를 대량 DAU 성능으로 일반화하지 않는다.

30/90/365/1095일 × Ads0/3/5/10회/일 × IAP0/1/3회/월, 월Gem4행·업적58·신규출석7·Engineer1·부활주1회/환급5% 가정의48개 projection은 ledger-model.json에 보존한다. 미래 광고 정책/상품/금액 결정이 아닌 receipt수 모델이다. root·index 경고 후 운영자 조사와 향후 shard migration이 필요하며 무기한 무한용량을 주장하지 않는다.

Save14/Profile2/CloudProfile1/generation3 유지, serverEconomy2/client snapshot1, Protocol23. 아이템·레시피·가격·보상 설계 변화 없음. 사용자 요청에 따라 여기서 중단하며 완료 조건과 다음 실행 순서는 RESUME.md를 따른다.

### Projection 비교표

|일수|Ads/일|IAP/월|행수|segment/hot|ledger bytes|index bytes|
|---:|---:|---:|---:|---:|---:|---:|
|30|0|0|75|0/75|29026|0|
|30|0|1|76|0/76|29413|0|
|30|0|3|78|0/78|30187|0|
|30|3|0|165|0/165|63857|0|
|30|3|1|166|0/166|64244|0|
|30|3|3|168|0/168|65018|0|
|30|5|0|225|0/225|87077|0|
|30|5|1|226|0/226|87464|0|
|30|5|3|228|0/228|88238|0|
|30|10|0|375|0/375|145128|0|
|30|10|1|376|0/376|145515|0|
|30|10|3|378|0/378|146289|0|
|90|0|0|91|0/91|35218|0|
|90|0|1|94|0/94|36379|0|
|90|0|3|100|0/100|38701|0|
|90|3|0|361|0/361|139710|0|
|90|3|1|364|0/364|140871|0|
|90|3|3|370|0/370|143193|0|
|90|5|0|541|1/285|209371|76800|
|90|5|1|544|1/288|210532|76800|
|90|5|3|550|1/294|212854|76800|
|90|10|0|991|2/479|383525|153600|
|90|10|1|994|2/482|384686|153600|
|90|10|3|1000|2/488|387008|153600|
|365|0|0|173|0/173|66953|0|
|365|0|1|186|0/186|71984|0|
|365|0|3|212|0/212|82046|0|
|365|3|0|1268|3/500|490726|230400|
|365|3|1|1281|4/257|495757|307200|
|365|3|3|1307|4/283|505819|307200|
|365|5|0|1998|6/462|773241|460800|
|365|5|1|2011|6/475|778272|460800|
|365|5|3|2037|6/501|788334|460800|
|365|10|0|3823|13/495|1479529|998400|
|365|10|1|3836|13/508|1484560|998400|
|365|10|3|3862|14/278|1494622|1075200|
|1095|0|0|378|0/378|146289|0|
|1095|0|1|415|0/415|160608|0|
|1095|0|3|489|0/489|189247|0|
|1095|3|0|3663|13/335|1417608|998400|
|1095|3|1|3700|13/372|1431927|998400|
|1095|3|3|3774|13/446|1460566|998400|
|1095|5|0|5853|21/477|2265154|1612800|
|1095|5|1|5890|22/258|2279473|1689600|
|1095|5|3|5964|22/332|2308112|1689600|
|1095|10|0|11328|43/320|4384018|3302400|
|1095|10|1|11365|43/357|4398337|3302400|
|1095|10|3|11439|43/431|4426976|3302400|
