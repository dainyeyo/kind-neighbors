using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>배달을 맡기는 쪽 (빵집 주인 등). 아직 끝나지 않은 첫 주문을 넘겨준다.</summary>
    public class DeliveryClient : MonoBehaviour, IInteractable
    {
        [SerializeField] DeliveryManager manager;
        [SerializeField] DeliveryOrder[] orders;

        public string Prompt => "주문 받기";

        public bool CanInteract(PlayerInteractor interactor) => manager.CurrentOrder == null && NextOrder() != null;

        public void Interact(PlayerInteractor interactor) => manager.TryAccept(NextOrder());

        DeliveryOrder NextOrder()
        {
            foreach (var order in orders)
                if (!manager.IsCompleted(order))
                    return order;
            return null;
        }
    }
}
