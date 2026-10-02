using KindNeighbors.Core;
using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using KindNeighbors.Save;
using KindNeighbors.Travel;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>
    /// Day/시간대 상태 머신을 돌린다. 시간은 실시간으로 흐르지 않고, 조건 충족(자동) 또는 플레이어 요청(문, 침대)으로 넘어간다.
    /// 각 Day의 낮에 들어갈 때 자동 저장한다.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] GameFlowConfig config;
        [SerializeField] GameFlags flags;
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] VoidEventChannel advanceRequested;
        [SerializeField] TravelEventChannel travelRequested;
        [SerializeField] bool loadSaveOnStart = true;
        [Tooltip("플레이어 이름이 정해질 때까지 첫 시간대를 시작하지 않는다 (이름 입력 화면)")]
        [SerializeField] bool requirePlayerName = true;

        readonly StateMachine machine = new();
        int phaseIndex = -1;
        int pendingStartIndex;

        public bool HasStarted => phaseIndex >= 0;

        PhaseState CurrentState => machine.Current as PhaseState;

        public PhaseDefinition CurrentPhase => CurrentState?.Definition;

        /// <summary>플레이어가 직접 넘길 수 있는 시간대이고, 완료 조건도 충족했는가.</summary>
        public bool CanAdvanceByRequest => CurrentState != null && !CurrentState.Definition.AutoAdvance && CurrentState.IsComplete;

        public string AdvancePrompt => CurrentState != null ? CurrentState.Definition.advancePrompt : string.Empty;

        void Awake() => flags.Clear();

        void OnEnable() => advanceRequested.Raised += OnAdvanceRequested;
        void OnDisable() => advanceRequested.Raised -= OnAdvanceRequested;

        void Start()
        {
            pendingStartIndex = 0;
            if (loadSaveOnStart && SaveSystem.TryLoad(out SaveData save))
            {
                int savedIndex = config.IndexOf(save.Phase);
                if (savedIndex >= 0)
                {
                    save.ApplyFlags(flags);
                    pendingStartIndex = savedIndex;
                    Debug.Log($"[GameFlow] 세이브 불러옴: {save.Phase}");
                }
            }
        }

        void Update()
        {
            if (!HasStarted)
            {
                // 이름이 정해진 뒤에 시작해야 Day 시작 자동 저장에 이름이 포함된다
                if (!requirePlayerName || !string.IsNullOrEmpty(flags.GetText(FlagKeys.PlayerName)))
                    EnterPhase(pendingStartIndex);
                return;
            }
            machine.Tick();
        }

        void OnAdvanceRequested()
        {
            if (CanAdvanceByRequest)
                Advance();
        }

        public void Advance()
        {
            if (phaseIndex + 1 >= config.phases.Length)
            {
                machine.ChangeState(null);
                phaseIndex = config.phases.Length;
                Debug.Log("[GameFlow] 마지막 시간대 종료 — 엔딩 판정은 M4에서 연결");
                return;
            }
            EnterPhase(phaseIndex + 1);
        }

        void EnterPhase(int index)
        {
            phaseIndex = index;
            PhaseDefinition definition = config.phases[index];
            machine.ChangeState(new PhaseState(definition, flags, phaseChanged, Advance));

            // 조명이 이미 바뀌었으므로 곧바로 화면을 가리고 옮긴다
            if (!string.IsNullOrEmpty(definition.spawnOnEnter))
                travelRequested.Raise(new TravelRequest(definition.spawnOnEnter, true));

            if (definition.phase.time == TimeOfDay.Morning)
                SaveSystem.Save(SaveData.Capture(definition.phase, flags));
        }
    }
}
