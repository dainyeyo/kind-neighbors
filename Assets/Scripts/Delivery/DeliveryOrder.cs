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

        /// <summary>플래그 키에 쓰는 식별자. 에셋 이름을 그대로 쓰므로 에셋 이름을 바꾸면 세이브 호환이 깨진다.</summary>
        public string Id => name;
    }
}
