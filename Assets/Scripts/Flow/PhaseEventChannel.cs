using KindNeighbors.Core.Events;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>시간대가 바뀔 때 발행된다. 조명, 소품 세트, NPC 배치, HUD가 구독한다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Events/Phase Event Channel", fileName = "Evt_")]
    public class PhaseEventChannel : EventChannel<PhaseDefinition> { }
}
