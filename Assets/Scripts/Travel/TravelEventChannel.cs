using KindNeighbors.Core.Events;
using UnityEngine;

namespace KindNeighbors.Travel
{
    [CreateAssetMenu(menuName = "Kind Neighbors/Events/Travel Event Channel", fileName = "Evt_")]
    public class TravelEventChannel : EventChannel<TravelRequest> { }
}
