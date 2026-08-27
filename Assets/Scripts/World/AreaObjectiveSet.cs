using System;
using UnityEngine;
using System.Collections.Generic;

namespace FantasyWorld.World
{
    /// <summary>
    /// 목표 한 항목의 정의. 완료 조건은 문자열 키로 표현한다 —
    /// ObjectiveZone 등이 같은 키로 "조건 달성"을 알리면 이 목표가 완료된다.
    /// </summary>
    [Serializable]
    public sealed class ObjectiveDefinition
    {
        [SerializeField] private string _description;
        [SerializeField] private string _completionKey;
        [SerializeField] private bool _optional;

        public string Description => _description;
        public string CompletionKey => _completionKey;
        public bool Optional => _optional;
    }

    /// <summary>
    /// 한 구역의 목표 목록. 코드 수정 없이 구역·목표를 추가할 수 있도록 에셋으로 관리한다.
    /// 구역 씬의 AreaLifetimeScope 에 꽂아두면 AreaFlow 가 ObjectiveManager 에 등록한다.
    /// </summary>
    /// <remarks>
    /// 이름은 "Area" 지만 파일은 World 계층에 둔다. 이 데이터를 소비하는 ObjectiveManager 가
    /// World 에 있어서, 데이터 타입도 같은 계층에 있어야 World -> Area 역방향 의존이 안 생긴다.
    /// (의존 방향: Area -> World -> Game, 한 방향만)
    /// </remarks>
    [CreateAssetMenu(menuName = "FantasyWorld/Area Objective Set", fileName = "AreaObjectiveSet")]
    public sealed class AreaObjectiveSet : ScriptableObject
    {
        [SerializeField] private string _areaName;
        [SerializeField] private ObjectiveDefinition[] _objectives = Array.Empty<ObjectiveDefinition>();

        public string AreaName => _areaName;
        public IReadOnlyList<ObjectiveDefinition> Objectives => _objectives;
    }
}
