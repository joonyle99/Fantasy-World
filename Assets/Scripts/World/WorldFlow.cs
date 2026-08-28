using System.Threading;
using VContainer.Unity;
using FantasyWorld.Core;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.World
{
    /// <summary>
    /// World 세션 진입점. 전역 음악을 켜고 AreaTransition 으로 첫 구역을 로드한다.
    /// </summary>
    public sealed class WorldFlow : IAsyncStartable
    {
        private const string FIRST_AREA = "Area_Garden";

        private readonly AudioService _audioService;
        private readonly GameSounds _sounds;
        private readonly AreaTransition _areaTransition;

        public WorldFlow(AudioService audioService, GameSounds sounds, AreaTransition areaTransition)
        {
            _audioService = audioService;
            _sounds = sounds;
            _areaTransition = areaTransition;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            // 전역 음악 — 구역이 바뀌어도 유지된다 (같은 이벤트면 재시작 안 함)
            _audioService.PlayMusic(_sounds.Music);

            await _areaTransition.GoTo(FIRST_AREA);
        }
    }
}
