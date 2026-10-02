using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>
    /// 플레이어가 직접 시간대를 넘기는 곳 (집 문 → 밤, 침대 → 다음 날).
    /// conditions로 어디서 넘길 수 있는지 제한한다 (예: 침대는 밤에만, 바깥 문은 밤이 아닐 때만).
    /// 넘길 수 있는지 판단하려고 GameFlowController를 읽지만, 넘기는 요청 자체는 채널로 보낸다.
    /// </summary>
    public class PhaseAdvanceTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] GameFlowController flow;
        [SerializeField] VoidEventChannel advanceRequested;
        [SerializeField] GameFlags flags;
        [SerializeField] FlagCondition[] conditions;

        public string Prompt => flow.AdvancePrompt;

        public bool CanInteract(PlayerInteractor interactor) => flow.CanAdvanceByRequest && FlagCondition.All(conditions, flags);

        public void Interact(PlayerInteractor interactor) => advanceRequested.Raise();
    }
}
