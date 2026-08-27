using System;
using System.Collections.Generic;

namespace FantasyWorld.World
{
    /// <summary>
    /// 게임플레이 이벤트 전달용 경량 pub/sub 버스. 이벤트 타입별로 핸들러를 모아두고,
    /// 발행 시 해당 타입 구독자에게 전달한다. 상호작용/NPC 시스템이 발행하고
    /// ObjectiveManager 등이 구독한다.
    /// </summary>
    public sealed class GameplayEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            _handlers[type] = _handlers.TryGetValue(type, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var existing))
                return;

            var updated = Delegate.Remove(existing, handler);
            if (updated == null)
                _handlers.Remove(type);
            else
                _handlers[type] = updated;
        }

        public void Publish<T>(T evt) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var existing))
                (existing as Action<T>)?.Invoke(evt);
        }
    }
}
