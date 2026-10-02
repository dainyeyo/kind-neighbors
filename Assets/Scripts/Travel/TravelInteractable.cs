using KindNeighbors.Core.Flags;
using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Travel
{
    /// <summary>문처럼 다른 곳으로 이동하는 상호작용. 조건(예: 낮에만)이 참일 때만 쓸 수 있다.</summary>
    public class TravelInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "들어가기";
        [SerializeField] string targetSpawnId;
        [SerializeField] GameFlags flags;
        [SerializeField] FlagCondition[] conditions;
        [SerializeField] TravelEventChannel travelRequested;

        public string Prompt => prompt;

        public bool CanInteract(PlayerInteractor interactor) => FlagCondition.All(conditions, flags);

        public void Interact(PlayerInteractor interactor) => travelRequested.Raise(new TravelRequest(targetSpawnId, false));
    }
}
