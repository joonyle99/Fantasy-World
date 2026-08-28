using VContainer;
using UnityEngine;
using VContainer.Unity;
using FantasyWorld.Core;
using UnityEngine.InputSystem;

// ============================================================
//  VContainer 요약 — 이 파일에서 쓰는 개념 위주
// ============================================================
//
//  [ LifetimeScope ]
//    - DI 컨테이너를 들고 있는 MonoBehaviour. 씬 GameObject에 붙인다.
//    - 컨테이너 = "타입 -> 인스턴스" 매핑 + 생성 규칙 저장소.
//    - Game / World / Area 계층마다 하나씩 두고 부모-자식으로 연결한다.
//      자식 스코프는 자기 컨테이너에 없는 타입을 부모에서 찾아 주입받는다.
//
//  [ Awake() / base.Awake() ]
//    - base.Awake() 안에서 컨테이너가 빌드된다:
//        1) Configure() 를 호출해 등록 목록(설계도)을 수집
//        2) 설계도로 컨테이너를 완성
//        3) EntryPoint 를 생성하고 Start() 등 생명주기 콜백을 호출
//    - 따라서 base.Awake() 를 반드시 먼저 호출해야 한다.
//
//  [ Configure(IContainerBuilder builder) ]
//    - 빌드 직전에 호출된다. builder 에 "이 타입은 이렇게 만들어라"만 적는다.
//    - 이 시점에 인스턴스는 생기지 않는다. (builder = 주문서, 요리는 나중에)
//
//  [ builder.Register<T>(Lifetime) ]
//    - T 가 요청되면 생성자를 호출해 만든다. 생성자 파라미터도 컨테이너가 주입한다.
//    - 생성은 lazy: 실제로 누가 요청할 때 만들어진다.
//    - Lifetime:
//        Singleton  : 이 컨테이너에서 1개. 처음 요청 시 생성 후 재사용.
//        Scoped     : 자식 스코프마다 1개.
//        Transient  : 요청할 때마다 새로.
//
//  [ builder.RegisterEntryPoint<T>() ]
//    - Register 와 달리 컨테이너 빌드 시 "즉시" 생성된다. 수명은 항상 Singleton.
//    - T 가 구현한 인터페이스에 따라 VContainer 가 콜백을 자동 호출한다:
//        IInitializable.Initialize()  -> 빌드 시
//        IStartable.Start()           -> 빌드 완료 후
//        ITickable.Tick()             -> 매 프레임 (Update 타이밍)
//        IFixedTickable.FixedTick()   -> FixedUpdate 타이밍
//        IDisposable.Dispose()        -> 스코프 파괴 시
//    - MonoBehaviour 가 아닌 순수 C# 클래스에서 Update/Start 흐름을 쓰는 방법.
//
//  [ .AsSelf() ]
//    - RegisterEntryPoint 는 기본적으로 "구현한 인터페이스"로만 등록된다.
//    - .AsSelf() 를 붙이면 구체 타입(GameFlow 등)으로도 주입받을 수 있다.
//    - 관련: .As<IFoo>() (특정 인터페이스만), .AsImplementedInterfaces() (전부)
//
// ------------------------------------------------------------
//  이 파일의 실행 흐름
// ------------------------------------------------------------
//    앱 시작
//      -> Awake -> base.Awake
//           -> Configure(builder): 6개 등록 (설계도만)
//           -> 컨테이너 빌드
//                InputService, GameFlow 즉시 생성 (+ 생성자 주입)
//                GameFlow <- SceneLoader, AudioService, InputService
//                InputService.Initialize() / GameFlow.StartAsync() 호출
//      -> DontDestroyOnLoad (씬이 바뀌어도 유지)
//      ...
//    스코프 파괴 -> GameFlow.Dispose(), InputService.Dispose()
//
// ============================================================

// ============================================================
//  namespace 를 쓰는 이유
// ============================================================
//    - 이름 충돌 방지: 패키지/플러그인에도 GameFlow, AudioService 같은
//      흔한 이름이 있을 수 있다. FantasyWorld.* 로 감싸 우리 것과 구분한다.
//    - 논리적 묶음: 폴더 구조와 1:1 로 맞춘다.
//        Assets/Scripts/Game   -> FantasyWorld.Game   (앱 루트 / 전역 흐름)
//        Assets/Scripts/Core   -> FantasyWorld.Core   (전역 인프라 서비스)
//        Assets/Scripts/World  -> FantasyWorld.World  (게임플레이 코어)
//        Assets/Scripts/Area   -> FantasyWorld.Area   (구역 단위)
//    - 의존 방향이 눈에 보인다: 이 파일이 FantasyWorld.Core 를 using 한다
//      = Game 계층이 Core 계층에 의존한다는 뜻.
//    - asmdef 의 "rootNamespace": "FantasyWorld" 와도 일치시킨다.
// ============================================================
namespace FantasyWorld.Game
{
    /// <summary>
    /// 앱 전역 컴포지션 루트. 부트스트랩 씬에 하나만 배치하며, 씬이 전환되어도 파괴되지 않는다.
    /// 여기 등록한 서비스는 앱 생명주기 내내 유지되고, 하위 씬/구역 스코프가 부모로 참조한다.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputActionAsset _inputActions;

        protected override void Awake()
        {
            base.Awake();

            // 씬을 넘나들며 유지되어야 하는 전역 스코프이므로 파괴를 막는다
            DontDestroyOnLoad(gameObject);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            // 앱이 살아있는 동안 계속 필요한 인프라 서비스
            builder.Register<SaveSystem>(Lifetime.Singleton);
            builder.Register<SettingsService>(Lifetime.Singleton);
            builder.Register<AudioService>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);

            // 인스펙터에서 꽂은 Input Actions 자산을 그대로 주입 대상으로 등록
            builder.RegisterInstance(_inputActions);

            // Input System 활성/해제 타이밍을 컨테이너 생명주기에 맞춘다
            builder.RegisterEntryPoint<InputService>().AsSelf();

            // 앱 전역 상태 전환의 주체
            builder.RegisterEntryPoint<GameFlow>().AsSelf();
        }
    }
}
