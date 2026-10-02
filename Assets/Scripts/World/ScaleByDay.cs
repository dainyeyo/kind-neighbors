using KindNeighbors.Core.Flags;
using KindNeighbors.Flow;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// Day가 지날 때마다 조금씩 커진다 (조용한 동료). 시간대가 바뀌는 순간(화면이 가려진 동안)에만 크기를 바꾼다.
    /// 규칙을 어긴 사람은 커 보인다는 마을의 사정을, 아무도 설명하지 않은 채 보여준다.
    /// </summary>
    public class ScaleByDay : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] PhaseEventChannel phaseChanged;
        [Tooltip("Day 1 이후 하루마다 커지는 비율")]
        [SerializeField] float growthPerDay = 0.05f;

        Vector3 baseScale;

        void Awake() => baseScale = transform.localScale;

        void OnEnable() => phaseChanged.Raised += OnPhaseChanged;
        void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void Start() => Apply();

        void OnPhaseChanged(PhaseDefinition definition) => Apply();

        void Apply()
        {
            int daysPassed = Mathf.Max(0, flags.GetInt(FlagKeys.Day) - 1);
            float factor = 1f + growthPerDay * daysPassed;
            // 키만 더 크게, 몸통은 조금만 — 길쭉해지는 쪽으로
            transform.localScale = new Vector3(baseScale.x * (1f + (factor - 1f) * 0.3f), baseScale.y * factor, baseScale.z * (1f + (factor - 1f) * 0.3f));
        }
    }
}
