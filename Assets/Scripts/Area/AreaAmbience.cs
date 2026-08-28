using FMODUnity;
using VContainer.Unity;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역의 앰비언스 베드를 GameplayAudioDirector 에 등록한다.
    /// 실제 재생 타이밍(Playing 상태일 때만)과 구역 전환 시 교차 페이드는 디렉터가 맡는다.
    /// 구역 씬이 언로드되면 이 스코프도 파괴되지만, 다음 구역이 곧바로 자기 앰비언스로
    /// 덮으므로 여기서 따로 정리하지 않는다.
    /// </summary>
    public sealed class AreaAmbience : IStartable
    {
        private readonly EventReference _ambience;
        private readonly GameplayAudioDirector _audioDirector;

        public AreaAmbience(EventReference ambience, GameplayAudioDirector audioDirector)
        {
            _ambience = ambience;
            _audioDirector = audioDirector;
        }

        void IStartable.Start()
        {
            _audioDirector.SetAreaAmbience(_ambience);
        }
    }
}
