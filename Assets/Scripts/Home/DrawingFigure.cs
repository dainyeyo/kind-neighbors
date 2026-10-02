using KindNeighbors.Core.Flags;
using KindNeighbors.Flow;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>
    /// 아이의 그림 속 길쭉한 형체. 시간대가 바뀔 때(화면이 가려진 동안)만 자리를 옮기므로 움직이는 순간은 보이지 않는다.
    /// Day가 지날수록, 규칙을 어겼을수록 그림 속 배달원에게 다가온다.
    /// </summary>
    public class DrawingFigure : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] Transform figure;
        [Tooltip("숲 가장자리 → 마을 집들 사이 → 배달원 옆 순서")]
        [SerializeField] Transform[] stages;

        void OnEnable() => phaseChanged.Raised += OnPhaseChanged;
        void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void Start() => Place();

        void OnPhaseChanged(PhaseDefinition definition) => Place();

        void Place()
        {
            int day = Mathf.Max(1, flags.GetInt(FlagKeys.Day));
            int extra = flags.GetInt(FlagKeys.RuleViolations) > 0 ? 1 : 0;
            int stage = Mathf.Clamp(day - 1 + extra, 0, stages.Length - 1);
            figure.SetPositionAndRotation(stages[stage].position, stages[stage].rotation);
        }
    }
}
