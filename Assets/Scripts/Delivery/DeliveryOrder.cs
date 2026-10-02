using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>배달 주문 데이터. 특별 배달도 같은 구조에 플래그만 다르게 둔다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Delivery Order", fileName = "Order_")]
    public class DeliveryOrder : ScriptableObject
    {
        public string itemName;
        public string clientName;
        public string recipientName;
        [Tooltip("DeliveryDestination.destinationId 와 일치해야 한다")]
        public string destinationId;
        public bool isSpecial;
    }
}
