using KindNeighbors.Core.Events;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    [CreateAssetMenu(menuName = "Kind Neighbors/Events/Delivery Order Event Channel", fileName = "Evt_")]
    public class DeliveryOrderEventChannel : EventChannel<DeliveryOrder> { }
}
