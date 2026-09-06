# Fantasy World

> VContainer DI · FMOD 오디오 기반 유니티 게임 아키텍처 레퍼런스

## 개요

Fantasy World는 하나의 완성된 게임이라기보다, **중소 규모 3D 게임을 어떻게 구조화할지**를
실제로 동작하는 코드로 보여 주는 유니티 프로젝트다. 플레이어는 구스(거위)를 조작해
구역을 돌아다니며 목표를 달성하고, 모든 필수 목표를 끝내면 다음 구역으로 넘어간다.

핵심 관심사는 게임 콘텐츠보다 **컴포지션 루트 설계, 의존성 주입, 씬·구역 생명주기,
오디오 라우팅, 목표 시스템의 분리**에 있다. 각 스크립트에는 왜 그렇게 구성했는지를
설명하는 주석이 함께 들어 있어, 아키텍처 학습·레퍼런스 용도로 읽을 수 있다.

## 주요 특징

- **계층형 LifetimeScope** — `Game → World → Area` 3계층 DI 컨테이너.
  자식 스코프는 없는 의존성을 부모에서 주입받고, 구역 전환마다 Area 스코프만 교체된다.
- **씬 구성** — 상주하는 World 씬 위에 Area 씬을 Additive 로 로드/언로드.
  Title·World 전환은 Single 로드.
- **목표 시스템** — `ObjectiveManager`가 구역별 목표 진행을 추적하고,
  씬에 배치된 `ObjectiveTrigger`(존 진입·울음·NPC 상태 등)가 이벤트 버스를 통해 판정에 연결된다.
- **FMOD 오디오** — 아래 별도 항목 참고.
- **뷰/로직 분리** — `GooseController`는 물리 이동·회전·애니메이션만 담당하고,
  상호작용 판정 등 게임 로직은 시스템 계층에 위임한다.

## 오디오 (FMOD)

사운드는 유니티 내장 오디오 대신 **FMOD Studio**로 처리한다. 게임플레이 코드가
FMOD API에 직접 묶이지 않도록 얇은 파사드를 두고, 그 뒤에서 재생을 라우팅한다.

- **`AudioService`** — 유일한 FMOD 접점. `PlayOneShot`(2D/3D), BGM·앰비언스 루프 재생만 노출한다.
  게임플레이 코드는 이 서비스와 "무엇을 재생할지"(`EventReference`)만 안다.
- **사운드 소유 위치** — 게임플레이 SFX는 `GameSounds`(ScriptableObject)에, 구역 앰비언스는
  각 Area 스코프에 두고, `GameplayAudioDirector`가 게임 이벤트를 듣고 알맞은 이벤트로 라우팅한다.
- **루프 관리** — `LoopChannel`이 BGM/앰비언스 인스턴스의 생성·페이드·정리를 담당하고,
  자연 종료된 이벤트를 재생 중으로 오인하지 않도록 상태를 추적한다.
- **뱅크 없이도 동작** — 넘어온 `EventReference`가 비어 있으면 조용히 무시하므로,
  FMOD 뱅크가 저장소에 없어도 프로젝트가 정상 실행된다.
- **참고 문서** — `Docs/fmod-unity-integration-guide.html`,
  `Docs/audio-middleware-fmod-vs-wwise.html`

## 기술 스택

| 항목 | 사용 |
| --- | --- |
| 엔진 | Unity 6000.3.6f1 (URP 17.3) |
| DI | [VContainer](https://github.com/hadashiA/VContainer) 1.19 |
| 비동기 | [UniTask](https://github.com/Cysharp/UniTask) |
| 오디오 | FMOD Studio |
| 입력 | Unity Input System |
| 내비게이션 | Unity AI Navigation (NavMesh) |
| 유틸리티 | [JoonyleGameDevKit](https://github.com/joonyle99/JoonyleGameDevKit) |

## 프로젝트 구조

```
Assets/Scripts/
├─ Core/      SceneLoader · AudioService · InputService · SaveSystem · SettingsService
├─ Game/      최상위 스코프 · GameFlow · GameState · TitleMenu · LoadingScreen
├─ World/     상주 World 스코프 · 목표 시스템 · GooseController · 상호작용 · NPC · 오디오 디렉터
└─ Area/      구역 스코프 · AreaFlow · 구역 경계 · 앰비언스 · NPC 컨트롤러
```

## 실행

Unity 6000.3.6f1로 프로젝트를 연 뒤 Title 씬에서 재생한다.
FMOD 뱅크는 저장소에 포함되지 않으며, 없어도 사운드만 재생되지 않고 정상 동작한다.
