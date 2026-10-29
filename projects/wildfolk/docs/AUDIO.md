# 음향 방향과 현재 상태

사용자가 지정한 원칙은 짧고 자연스럽고 물리적인 야외 생존 질감이다. OS 알림음/beep/단순 sine/디지털 뿅/밝은 퍼즐게임 UI음은 사용하지 않는다. 품질이 부족한 임시음보다 무음을 선택한다.

기존 직접 생성한 action.wav는 sine 기반 generic cue여서 제거했다. 당시 생성 도구 generate_audio_assets.py, Audio 폴더/meta, GameAudio.ActionClip 참조와 PlayerAction의 자동 재생도 제거했다. 이 이력은 유지하며 해당 tone/generator는 복구하지 않았다.

## Foundation5 중간 저장 — 2026-10-29

사용자 요청으로 음원 확보·선별·편집 단계에서 중단했다. **Full Audio Production은 아직 완료되지 않았다.** `Assets/Wildfolk/Audio/`에 Music6 / Ambience6 / SFX92, 총104개의 선별·편집 음원이 있다. 실제 런타임의 Music/SFX AudioSource에는 아직 clip이 배선되지 않아 현재 게임에서 새 소리가 재생되지는 않는다. 기존 GameAudio와 Master×Music/SFX 설정·저장은 그대로다.

Music 후보는 메뉴/낮/밤/동굴/묘지/보스용 composed bed6개다. 환경 후보는 낮 숲/밤 곤충/동굴/깊은 동굴/묘지 바람/모닥불6개다. 최종 상황 우선순위·Deep Cavern에서의 음악 공유·전환 시간은 실제 런타임 연결과 검수에서 확정한다. SFX는 재질별 채집/발소리/물리 UI/전투/적 계열/보스/보상용 원본을 선별했으며, 파일명이 곧 이벤트 연결 완료를 뜻하지 않는다.

전체 선별본은 **13,623,935 bytes**다. 원본은 Git/게임 배포 밖에 보관하며, 개별 source/download URL·작가·CC0·pinned mirror commit·SHA256·편집 과정·PCM peak/RMS/LUFS/DC/loop seam 측정은 [assets.json](../validation/audio-production/assets.json)에 기록한다. 짧은 효과음은 mono PCM16/32kHz, bed는 stereo Vorbis/32kHz로 편집했다. 원본 master와 mirror 파일이 byte-identical하다고 주장하지 않는다. wind1은 작가가 PureData로 제작한 바람 asset이며 자연 현장 녹음이라고 표시하지 않는다.

상업 이용·배포가 허용되는 CC0 source만 선별했다. [AUDIO_LICENSES](AUDIO_LICENSES.md)는 작가별 출처를, [REVIEW](../validation/audio-production/REVIEW.md)는 6개 게임 비교·기존 source 감사·중단 위치·실제 검증 범위와 다음 작업을 기록한다. 참조 게임의 OST/효과음을 추출하거나 복제하지 않았다.

이번 단계의 파일 분석/Unity import 검사는 실제 청취·플레이 검수와 구분한다. Mixer5개 bus, runtime event binding, 성공 거래/네트워크 dedup, 실제 Play/LAN/장시간/KO·EN settings/Android script compile 검증은 다음 회차에 남아 있다. Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3/IAPSchema1은 변경하지 않는다.
