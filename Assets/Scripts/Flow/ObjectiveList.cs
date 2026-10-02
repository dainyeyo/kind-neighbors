using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>
    /// HUD에 띄울 '지금 할 일'. 위에서부터 조건이 처음 맞는 한 줄을 보여준다.
    /// 진행이 더 된 상태를 위에 둔다 (대화 시작 지점과 같은 규칙).
    /// </summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Flow/Objective List", fileName = "Objectives")]
    public class ObjectiveList : ScriptableObject
    {
        public ConditionalText[] objectives;

        public string Current(GameFlags flags) => ConditionalText.FirstMatch(objectives, flags);
    }
}
