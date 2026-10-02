using System.Collections.Generic;
using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>
    /// 배달 흐름: 수령 → 목적지 표시 → 전달 → 완료 이벤트.
    /// 수령/완료 여부는 플래그(accepted.*, delivered.*)로 남겨 대화 조건, 시간대 완료 조건, 저장에서 그대로 쓴다.
    /// </summary>
    public class DeliveryManager : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] DeliveryOrderEventChannel orderRequested;
        [SerializeField] DeliveryOrderEventChannel orderAccepted;
        [SerializeField] DeliveryOrderEventChannel orderCompleted;

        readonly List<DeliveryDestination> destinations = new();

        public DeliveryOrder CurrentOrder { get; private set; }

        void OnEnable() => orderRequested.Raised += OnOrderRequested;
        void OnDisable() => orderRequested.Raised -= OnOrderRequested;

        public void Register(DeliveryDestination destination) => destinations.Add(destination);
        public void Unregister(DeliveryDestination destination) => destinations.Remove(destination);

        public bool IsCompleted(DeliveryOrder order) => flags.GetBool(FlagKeys.Delivered(order.Id));

        void OnOrderRequested(DeliveryOrder order) => TryAccept(order);

        public bool TryAccept(DeliveryOrder order)
        {
            if (CurrentOrder != null || order == null || IsCompleted(order))
                return false;

            CurrentOrder = order;
            flags.SetBool(FlagKeys.Accepted(order.Id), true);
            SetMarker(order.destinationId, true);
            orderAccepted.Raise(order);
            return true;
        }

        public bool TryDeliver(DeliveryDestination destination)
        {
            if (CurrentOrder == null || CurrentOrder.destinationId != destination.DestinationId)
                return false;

            DeliveryOrder order = CurrentOrder;
            CurrentOrder = null;
            flags.SetBool(FlagKeys.Delivered(order.Id), true);
            SetMarker(order.destinationId, false);
            orderCompleted.Raise(order);
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
