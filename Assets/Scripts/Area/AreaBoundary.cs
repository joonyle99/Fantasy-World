using VContainer;
using UnityEngine;
using FantasyWorld.Core;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 진입/이탈 감지. 구스가 경계를 넘으면 다음 구역 프리로드/언로드를 트리거한다.
    /// </summary>
    public sealed class AreaBoundary : MonoBehaviour
    {
        private SceneLoader _sceneLoader;

        [Inject]
        public void Construct(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        // TODO: OnTriggerEnter/Exit → 인접 구역 스트리밍 요청
    }
}
