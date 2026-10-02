using System;
using System.Collections.Generic;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>
    /// 배달 흐름: 수령 → 목적지 표시 → 전달 → 완료 이벤트.
    /// M1에서 C# 이벤트를 ScriptableObject 이벤트 채널로 교체할 예정.
    /// </summary>
    public class DeliveryManager : MonoBehaviour
    {
        readonly List<DeliveryDestination> destinations = new();
        readonly List<DeliveryOrder> completed = new();

        public DeliveryOrder CurrentOrder { get; private set; }
        public IReadOnlyList<DeliveryOrder> Completed => completed;

        public event Action<DeliveryOrder> OrderAccepted;
        public event Action<DeliveryOrder> OrderCompleted;

        public void Register(DeliveryDestination destination) => destinations.Add(destination);
        public void Unregister(DeliveryDestination destination) => destinations.Remove(destination);

        public bool IsCompleted(DeliveryOrder order) => completed.Contains(order);

        public bool TryAccept(DeliveryOrder order)
        {
            if (CurrentOrder != null || order == null || IsCompleted(order))
                return false;

            CurrentOrder = order;
            SetMarker(order.destinationId, true);
            OrderAccepted?.Invoke(order);
            return true;
        }

        public bool TryDeliver(DeliveryDestination destination)
        {
            if (CurrentOrder == null || CurrentOrder.destinationId != destination.DestinationId)
                return false;

            DeliveryOrder order = CurrentOrder;
            CurrentOrder = null;
            completed.Add(order);
            SetMarker(order.destinationId, false);
            OrderCompleted?.Invoke(order);
            return true;
        }

        void SetMarker(string destinationId, bool visible)
        {
            foreach (var d in destinations)
                if (d.DestinationId == destinationId)
                    d.SetMarkerVisible(visible);
        }
    }
}
