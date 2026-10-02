using KindNeighbors.Core.Flags;
using KindNeighbors.Flow;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// 지정한 시간대가 되면 대상들을 하나씩 시간차를 두고 켠다 (저녁에 덧창이 하나둘 닫히고 좌판 덮개가 덮이는 연출).
    /// 저녁이 시작될 때 플레이어가 빵집 안에 있으면, 마을로 나온 뒤부터 시작한다.
    /// 다른 시간대가 되면 즉시 모두 끈다.
    /// </summary>
    public class PhaseDelayedToggle : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] GameObject[] targets;
        [Tooltip("밤에는 기다리지 않고 바로 켠다")]
        [SerializeField] TimeOfDay[] staggeredTimes = { TimeOfDay.Evening };
        [SerializeField] TimeOfDay[] instantTimes = { TimeOfDay.Night };
        [SerializeField] Vector2 delayRange = new(2f, 20f);

        float[] showAt;
        bool staggering;

        void Awake() => showAt = new float[targets.Length];

        void OnEnable()
        {
            phaseChanged.Raised += OnPhaseChanged;
            Apply();
        }

        void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void OnPhaseChanged(PhaseDefinition definition) => Apply();

        void Apply()
        {
            var time = (TimeOfDay)flags.GetInt(FlagKeys.TimeOfDay);
            bool instant = System.Array.IndexOf(instantTimes, time) >= 0;
            staggering = System.Array.IndexOf(staggeredTimes, time) >= 0;

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].SetActive(instant);
                showAt[i] = -1f;
            }
        }

        void Update()
        {
            if (!staggering || !VillageArea.PlayerInside)
                return;

            bool allShown = true;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].activeSelf)
                    continue;
                allShown = false;
                if (showAt[i] < 0f)
                    showAt[i] = Time.time + Random.Range(delayRange.x, delayRange.y);
                else if (Time.time >= showAt[i])
                    targets[i].SetActive(true);
            }
            if (allShown)
                staggering = false;
        }
    }
}
