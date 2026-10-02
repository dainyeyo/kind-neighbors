using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>배달을 받는 곳 (우편함, 문). 현재 주문의 목적지일 때만 상호작용할 수 있다.</summary>
    public class DeliveryDestination : MonoBehaviour, IInteractable
    {
        [SerializeField] DeliveryManager manager;
        [SerializeField] string destinationId;
        [SerializeField] GameObject marker;
        [SerializeField] float markerSpinSpeed = 90f;

        public string DestinationId => destinationId;

        public string Prompt => manager.CurrentOrder != null ? $"{manager.CurrentOrder.itemName} 배달하기" : string.Empty;

        void OnEnable() => manager.Register(this);
        void OnDisable() => manager.Unregister(this);

        void Start() => SetMarkerVisible(false);

        void Update()
        {
            if (marker != null && marker.activeSelf)
                marker.transform.Rotate(0f, markerSpinSpeed * Time.deltaTime, 0f, Space.World);
        }

        public void SetMarkerVisible(bool visible)
        {
            if (marker != null)
                marker.SetActive(visible);
        }

        public bool CanInteract(PlayerInteractor interactor) =>
            manager.CurrentOrder != null && manager.CurrentOrder.destinationId == destinationId;

        public void Interact(PlayerInteractor interactor) => manager.TryDeliver(this);
    }
}
