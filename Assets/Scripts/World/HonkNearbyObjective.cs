using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 이 지점 근처에서 구스가 (지정 횟수만큼) 울면 완료. "특정 장소에서 울기" 류 목표.
    /// </summary>
    public sealed class HonkNearbyObjective : ObjectiveTrigger
    {
        [SerializeField] private float _radius = 3f;
        [Tooltip("완료까지 필요한 울음 횟수. 1 이면 한 번.")]
        [SerializeField] private int _requiredHonks = 1;

        private GameplayEventBus _eventBus;
        private int _honkCount;

        public override void Bind(ObjectiveContext context)
        {
            _eventBus = context.EventBus;
            _eventBus.Subscribe<GooseHonked>(OnHonked);
        }

        public override void Unbind()
        {
            _eventBus?.Unsubscribe<GooseHonked>(OnHonked);
        }

        private void OnHonked(GooseHonked honk)
        {
            if ((honk.Position - transform.position).sqrMagnitude > _radius * _radius)
                return;

            _honkCount++;
            if (_honkCount >= _requiredHonks)
                Complete();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
