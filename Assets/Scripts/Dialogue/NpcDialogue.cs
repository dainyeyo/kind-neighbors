using KindNeighbors.Core.Flags;
using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Dialogue
{
    /// <summary>말을 걸 수 있는 NPC. 현재 플래그로 시작할 노드가 있을 때만 상호작용할 수 있다.</summary>
    public class NpcDialogue : MonoBehaviour, IInteractable
    {
        [SerializeField] DialogueGraph graph;
        [SerializeField] GameFlags flags;
        [SerializeField] DialogueGraphEventChannel dialogueRequested;

        public string Prompt => $"{graph.speakerName}에게 말 걸기";

        public bool CanInteract(PlayerInteractor interactor) => graph.FindStartNode(flags) != null;

        public void Interact(PlayerInteractor interactor) => dialogueRequested.Raise(graph);
    }
}
