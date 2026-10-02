using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using KindNeighbors.Flow;
using KindNeighbors.Player;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// 마을의 배경 주민. 대화 상대가 아니라 생활하는 사람이다.
    /// - 낮: 제자리에서 일하거나(waypoints 없음) 몇 지점을 오가며 생활한다.
    /// - 플레이어가 가까이 지나가면 한 마디 한다 (자막). 대사는 Day·플래그 조건으로 고른다.
    /// - 저녁: 플레이어가 마을에 있으면 잠시 뒤 집으로 걸어 들어가 사라진다 (귀가 러시).
    /// </summary>
    public class AmbientResident : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] StringEventChannel subtitle;
        [SerializeField] string speakerName;
        [Tooltip("위에서부터 조건이 처음 맞는 한 줄을 말한다")]
        [SerializeField] ConditionalText[] barks;
        [SerializeField] Transform[] waypoints;
        [SerializeField] float walkSpeed = 1.2f;
        [SerializeField] float waitAtPoint = 2.5f;
        [Tooltip("저녁에 들어갈 문. 비어 있으면 저녁에도 그 자리에 있다.")]
        [SerializeField] Transform homeDoor;
        [SerializeField] Vector2 goHomeDelay = new(1f, 8f);
        [SerializeField] GameObject body;
        [SerializeField] float barkRadius = 3.5f;
        [SerializeField] float barkCooldown = 30f;

        Vector3 startPosition;
        Quaternion startRotation;
        int waypointIndex;
        float waitUntil;
        bool evening;
        float goHomeAt = -1f;
        float lastBark = -999f;
        Transform player;

        void Awake()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        void OnEnable()
        {
            phaseChanged.Raised += OnPhaseChanged;
            ApplyTimeOfDay();
        }

        void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void OnPhaseChanged(PhaseDefinition definition) => ApplyTimeOfDay();

        void ApplyTimeOfDay()
        {
            var time = (TimeOfDay)flags.GetInt(FlagKeys.TimeOfDay);
            evening = time == TimeOfDay.Evening;
            goHomeAt = -1f;
            if (time == TimeOfDay.Morning)
            {
                body.SetActive(true);
                transform.SetPositionAndRotation(startPosition, startRotation);
                waypointIndex = 0;
            }
        }

        void Update()
        {
            if (!body.activeSelf)
                return;

            if (evening && homeDoor != null && VillageArea.PlayerInside)
            {
                if (goHomeAt < 0f)
                    goHomeAt = Time.time + Random.Range(goHomeDelay.x, goHomeDelay.y);
                if (Time.time >= goHomeAt && WalkTo(homeDoor.position, walkSpeed * 1.2f))
                    body.SetActive(false); // 문 안으로 들어갔다
            }
            else if (waypoints.Length > 0 && Time.time >= waitUntil)
            {
                if (WalkTo(waypoints[waypointIndex].position, walkSpeed))
                {
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    waitUntil = Time.time + waitAtPoint;
                }
            }

            TryBark();
        }

        /// <returns>도착했으면 true</returns>
        bool WalkTo(Vector3 target, float speed)
        {
            target.y = transform.position.y;
            Vector3 toTarget = target - transform.position;
            if (toTarget.sqrMagnitude < 0.04f)
                return true;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), 8f * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            return false;
        }

        void TryBark()
        {
            if (Time.time - lastBark < barkCooldown)
                return;
            if (player == null)
            {
                var controller = FindFirstObjectByType<PlayerController>();
                if (controller == null)
                    return;
                player = controller.transform;
            }
            if ((player.position - transform.position).sqrMagnitude > barkRadius * barkRadius)
                return;

            string line = ConditionalText.FirstMatch(barks, flags);
            if (string.IsNullOrEmpty(line))
                return;
            lastBark = Time.time;
            subtitle.Raise($"{speakerName}: {line}");
        }
    }
}
