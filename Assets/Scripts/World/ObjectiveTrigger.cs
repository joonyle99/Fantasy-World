using System;
using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구역 씬에 배치되어 "완료 조건 하나"를 감지하는 트리거의 베이스.
    /// AreaFlow 가 씬의 모든 ObjectiveTrigger 를 찾아 Bind 로 런타임 참조를 주고,
    /// ConditionMet 을 ObjectiveManager 로 잇는다. 존 진입 · 울음 · NPC 상태 등 조건별로 파생한다.
    /// </summary>
    public abstract class ObjectiveTrigger : MonoBehaviour
    {
        [SerializeField] private string _completionKey;

        /// <summary>조건이 달성됐을 때 완료 키를 전달한다.</summary>
        public event Action<string> ConditionMet;

        /// <summary>
        /// AreaFlow 가 구역 로드 시 호출한다. 이벤트 기반 트리거가 버스 · 구스 참조를 받는 지점.
        /// 참조가 필요 없는 트리거(ObjectiveZone 등)는 오버라이드하지 않는다.
        /// </summary>
        public virtual void Bind(ObjectiveContext context) { }

        /// <summary>구역 언로드 시 AreaFlow 가 호출한다. Bind 에서 건 구독을 여기서 해제한다.</summary>
        public virtual void Unbind() { }

        /// <summary>파생 클래스가 조건 달성을 알릴 때 호출한다.</summary>
        protected void Complete()
        {
            ConditionMet?.Invoke(_completionKey);
        }
    }
}
