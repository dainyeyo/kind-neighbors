using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>시간대 하나의 데이터. 언제 끝나는지, 어떻게 다음으로 넘어가는지, 어떤 조명을 쓰는지.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Flow/Phase Definition", fileName = "Phase_")]
    public class PhaseDefinition : ScriptableObject
    {
        public GamePhase phase;
        public LightingPreset lighting;

        [Tooltip("모두 참이어야 이 시간대를 끝낼 수 있다. 비어 있으면 항상 참.")]
        public FlagCondition[] completeWhen;

        [Tooltip("비어 있으면 조건이 충족되는 즉시 자동으로 넘어간다.\n값이 있으면 PhaseAdvanceTrigger(예: 집 문)에서 이 문구로 플레이어가 직접 넘긴다.")]
        public string advancePrompt;

        public bool AutoAdvance => string.IsNullOrEmpty(advancePrompt);
    }
}
