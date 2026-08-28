using FMODUnity;
using VContainer;
using UnityEngine;
using VContainer.Unity;
using FantasyWorld.World;

// ============================================================
//  3계층 스코프 — 2계층에서 추가되는 개념
// ============================================================
//
//  [ 스코프 체인 ]
//    Area -> World -> Game 3단.
//    AreaFlow 는 자기 스코프(AreaObjectiveSet), World 스코프(ObjectiveManager,
//    NpcRegistry), 필요하면 Game 스코프(SceneLoader)까지 한 생성자에서 받는다.
//
//  [ 스코프 파기 = 자동 정리 ]
//    구역 씬은 Additive 로 로드되고, 나갈 때 씬째 Unload 된다.
//    그때 이 LifetimeScope 도 파괴되면서:
//      - 이 스코프에서 만든 인스턴스의 IDisposable.Dispose() 호출
//      - 등록/구독한 것들이 함께 정리됨
//    구역별 리소스·이벤트 구독을 수동 해제할 필요가 없다.
//
//  [ [SerializeField] + builder.RegisterInstance<T>() ]
//    - 인스펙터에서 이 스코프에 ScriptableObject 를 꽂아둔다.
//    - RegisterInstance 는 "이미 만들어진 객체"를 그대로 등록한다.
//      VContainer 가 생성하지 않고 이 인스턴스를 그대로 주입한다.
//    - 구역마다 다른 에셋을 꽂으면, 같은 AreaFlow 코드가 구역별 데이터를 받는다.
//
// ============================================================

namespace FantasyWorld.Area
{
    /// <summary>
    /// 각 구역(Area) 씬의 컴포지션 루트. WorldLifetimeScope 를 부모로 두며,
    /// 구역이 언로드되면 스코프째 정리되어 이벤트 구독/리소스가 자동 해제된다.
    /// </summary>
    public sealed class AreaLifetimeScope : LifetimeScope
    {
        [SerializeField] private AreaObjectiveSet _objectiveSet;

        [Tooltip("이 구역의 앰비언스 FMOD 이벤트. 비워두면 앰비언스 없음.")]
        [SerializeField] private EventReference _ambience;

        protected override void Configure(IContainerBuilder builder)
        {
            // 인스펙터에서 꽂은 구역별 데이터를 그대로 등록
            builder.RegisterInstance(_objectiveSet);
            builder.RegisterInstance(_ambience);

            // 씬에 배치된 구역 전용 컴포넌트
            builder.RegisterComponentInHierarchy<AreaNpcController>();
            builder.RegisterComponentInHierarchy<AreaBoundary>();

            // 구역 진입 흐름의 주체
            builder.RegisterEntryPoint<AreaFlow>();
            builder.RegisterEntryPoint<AreaAmbience>();
        }
    }
}
