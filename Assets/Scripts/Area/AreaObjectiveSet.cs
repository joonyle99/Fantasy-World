using UnityEngine;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 한 구역의 목표 정의 데이터. 코드 수정 없이 구역·목표를 추가할 수 있도록 에셋으로 관리한다.
    /// </summary>
    [CreateAssetMenu(menuName = "FantasyWorld/Area Objective Set", fileName = "AreaObjectiveSet")]
    public sealed class AreaObjectiveSet : ScriptableObject
    {
        // TODO: 목표 항목 배열(설명, 완료 조건 식별자, 선택/필수 여부 등)
    }
}
