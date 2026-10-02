using KindNeighbors.Dialogue;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>대화 중 NPC에게 물건을 직접 전달한다. 들고 있는 주문이 이 주문일 때만 완료된다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Dialogue/Actions/Deliver Order", fileName = "Action_DeliverOrder_")]
    public class DeliverOrderAction : DialogueAction
    {
        public DeliveryOrder order;
        public DeliveryOrderEventChannel deliverRequested;

        public override void Execute() => deliverRequested.Raise(order);
    }
}
