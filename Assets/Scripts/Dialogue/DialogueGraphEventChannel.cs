using KindNeighbors.Core.Events;
using UnityEngine;

namespace KindNeighbors.Dialogue
{
    /// <summary>대화 시작 요청. NPC가 발행하고 DialogueRunner가 구독한다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Events/Dialogue Event Channel", fileName = "Evt_")]
    public class DialogueGraphEventChannel : EventChannel<DialogueGraph> { }
}
