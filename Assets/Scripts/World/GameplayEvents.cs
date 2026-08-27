using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// GameplayEventBus 로 오가는 이벤트 타입들. 값 타입(struct)으로 두어 발행 비용을 낮춘다.
    /// 목표 판정·NPC 반응 등이 이 이벤트를 구독한다.
    /// </summary>
    public readonly struct GooseGrabbed
    {
        public readonly Grabbable Target;

        public GooseGrabbed(Grabbable target)
        {
            Target = target;
        }
    }

    public readonly struct GooseDropped
    {
        public readonly Grabbable Target;
        public readonly Vector3 Position;

        public GooseDropped(Grabbable target, Vector3 position)
        {
            Target = target;
            Position = position;
        }
    }

    public readonly struct GooseHonked
    {
        public readonly Vector3 Position;

        public GooseHonked(Vector3 position)
        {
            Position = position;
        }
    }
}
