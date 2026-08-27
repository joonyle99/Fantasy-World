using System;
using VContainer.Unity;

namespace FantasyWorld.Core
{
    /// <summary>
    /// Input System 액션을 게임 의미 단위(이동, 상호작용, 울음 등)로 추상화한다.
    /// EntryPoint 로 등록되어 컨테이너 생성 시 활성화, 파기 시 해제된다.
    /// </summary>
    public sealed class InputService : IInitializable, IDisposable
    {
        // TODO: 생성한 InputActions 자산 참조, 액션별 값/이벤트 노출

        public void Initialize()
        {
            // TODO: Enable
        }

        public void Dispose()
        {
            // TODO: Disable / Dispose
        }
    }
}
