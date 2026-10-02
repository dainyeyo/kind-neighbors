using System;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>
    /// 지정한 시간대에만 대상 오브젝트들을 켠다. 소품 세트, NPC 배치 교체에 쓴다.
    /// 이 컴포넌트 자신은 항상 켜져 있어야 하므로 대상의 부모가 아닌 별도 오브젝트에 둔다.
    /// </summary>
    public class PhaseObjectSet : MonoBehaviour
    {
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] GameObject[] targets;
        [SerializeField] TimeOfDay[] activeTimes = { TimeOfDay.Morning };
        [SerializeField] int fromDay = 1;
        [SerializeField] int toDay = 3;

        void OnEnable() => phaseChanged.Raised += OnPhaseChanged;
        void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void OnPhaseChanged(PhaseDefinition definition)
        {
            GamePhase phase = definition.phase;
            bool active = phase.day >= fromDay && phase.day <= toDay && Array.IndexOf(activeTimes, phase.time) >= 0;
            foreach (var target in targets)
                if (target != null)
                    target.SetActive(active);
        }
    }
}
