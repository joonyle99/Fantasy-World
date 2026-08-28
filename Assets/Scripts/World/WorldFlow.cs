using System.Threading;
using VContainer.Unity;
using FantasyWorld.Game;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.World
{
    /// <summary>
    /// World 세션 진입점. 첫 구역을 로드하고 GameFlow 에 "세션 준비 완료"를 알린다.
    /// (전역 음악은 GameplayAudioDirector 가 켠다)
    /// </summary>
    public sealed class WorldFlow : IAsyncStartable
    {
        private const string FIRST_AREA = "Area_Garden";

        private readonly AreaTransition _areaTransition;
        private readonly GameFlow _gameFlow;

        public WorldFlow(AreaTransition areaTransition, GameFlow gameFlow)
        {
            _areaTransition = areaTransition;
            _gameFlow = gameFlow;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _areaTransition.GoTo(FIRST_AREA);

            // 첫 구역까지 떴으니 이제 보여줘도 된다 — GameFlow 가 Playing 으로 전환
            _gameFlow.NotifyWorldReady();
        }
    }
}
