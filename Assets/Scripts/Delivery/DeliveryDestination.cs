using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>
    /// 배달 목적지. 현재 주문의 목적지일 때 마커를 띄운다.
    /// dropOffHere가 켜져 있으면(우편함, 문) 직접 상호작용해서 넣을 수 있고,
    /// 꺼져 있으면(NPC) 마커만 띄우고 전달은 대화의 DeliverOrderAction이 맡는다.
    /// </summary>
    public class DeliveryDestination : MonoBehaviour, IInteractable
    {
        [SerializeField] DeliveryManager manager;
        [SerializeField] string destinationId;
        [SerializeField] bool dropOffHere = true;
        [SerializeField] GameObject marker;
        [SerializeField] float markerSpinSpeed = 90f;

        public string DestinationId => destinationId;

        public string Prompt => manager.CurrentOrder != null ? $"{manager.CurrentOrder.itemName} 넣기" : string.Empty;

        void OnEnable() => manager.Register(this);
        void OnDisable() => manager.Unregister(this);

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
            dropOffHere && manager.CurrentOrder != null && manager.CurrentOrder.destinationId == destinationId;

        public void Interact(PlayerInteractor interactor) => manager.TryDeliver(this);
    }
}
