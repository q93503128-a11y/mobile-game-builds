# 음향 방향과 현재 상태

사용자가 지정한 원칙은 짧고 자연스럽고 물리적인 야외 생존 질감이다. OS 알림음/beep/단순 sine/디지털 뿅/밝은 퍼즐게임 UI음은 사용하지 않는다. 품질이 부족한 임시음보다 무음을 선택한다.

기존 직접 생성한 action.wav는 sine 기반 generic cue여서 제거했다. 생성 도구 generate_audio_assets.py, Audio 폴더/meta, GameAudio.ActionClip 참조와 PlayerAction의 자동 재생도 제거했다. 현재 실제 SFX/BGM clip은 없다. AudioSource Music/SFX 및 Master×채널 음량 설정과 저장을 유지한다. 이후 적합한 원본/상업 이용 가능한 라이선스 SFX를 GameAudio.PlaySfx(AudioClip) 버스로 연결할 수 있다.

향후 wood hit/dry stone impact/물리적 pickup/절제된 click/짧은 sword swing/무거운 axe swing은 방향이며 현재 구현된 소리 목록이 아니다. 새 외부 음원을 확보하지 않아 출처/라이선스가 불명확한 음원도 없다. 임시 BGM은 생성하지 않는다. 제거한 cue는 이전 단계의 독립 생성물이며 현재 아트는 프로젝트 자체 Pillow 생성 소스다.
