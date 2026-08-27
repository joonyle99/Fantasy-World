using UnityEngine;
using VContainer.Unity;
using FantasyWorld.Core;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스의 집기/물기/놓기 상호작용. Interact 를 누르고 있는 동안 부리 근처의 Grabbable 을 물고,
    /// 놓으면 앞으로 살짝 던진다. 결과를 GameplayEventBus 로 발행한다.
    /// </summary>
    public sealed class InteractionSystem : ITickable
    {
        private const float GrabRadius = 0.6f;
        private const float DropForwardSpeed = 1.5f;

        private static readonly Collider[] OverlapBuffer = new Collider[8];

        private readonly InputService _inputService;
        private readonly GooseController _goose;
        private readonly GameplayEventBus _eventBus;

        private Grabbable _held;

        public InteractionSystem(InputService inputService, GooseController goose, GameplayEventBus eventBus)
        {
            _inputService = inputService;
            _goose = goose;
            _eventBus = eventBus;
        }

        void ITickable.Tick()
        {
            var wantsHold = _inputService.InteractHeld;

            if (wantsHold && _held == null)
                TryGrab();
            else if (!wantsHold && _held != null)
                Drop();
        }

        private void TryGrab()
        {
            var beak = _goose.BeakPoint;
            var count = Physics.OverlapSphereNonAlloc(beak.position, GrabRadius, OverlapBuffer);

            Grabbable nearest = null;
            var nearestSqrDistance = float.MaxValue;

            for (var i = 0; i < count; i++)
            {
                if (!OverlapBuffer[i].TryGetComponent(out Grabbable grabbable) || grabbable.IsHeld)
                    continue;

                var sqrDistance = (grabbable.transform.position - beak.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = grabbable;
                }
            }

            if (nearest == null)
                return;

            nearest.AttachTo(beak);
            _held = nearest;
            _eventBus.Publish(new GooseGrabbed(nearest));
        }

        private void Drop()
        {
            var dropped = _held;
            _held = null;

            dropped.Release(_goose.transform.forward * DropForwardSpeed);
            _eventBus.Publish(new GooseDropped(dropped, dropped.transform.position));
        }
    }
}
