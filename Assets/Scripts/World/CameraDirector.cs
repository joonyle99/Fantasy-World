using VContainer;
using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스 추적 카메라. 구역별 프레이밍/경계 처리를 담당한다.
    /// </summary>
    public sealed class CameraDirector : MonoBehaviour
    {
        private GooseController _goose;

        [Inject]
        public void Construct(GooseController goose)
        {
            _goose = goose;
        }

        // TODO: LateUpdate 추적, 구역 전환 시 프레이밍 조정
    }
}
