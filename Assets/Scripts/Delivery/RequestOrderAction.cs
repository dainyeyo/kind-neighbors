using KindNeighbors.Dialogue;
using UnityEngine;

namespace KindNeighbors.Delivery
{
    /// <summary>대화 중 주문을 넘겨준다. DeliveryManager를 직접 부르지 않고 요청 채널로 보낸다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Dialogue/Actions/Request Order", fileName = "Action_RequestOrder_")]
    public class RequestOrderAction : DialogueAction
    {
        public DeliveryOrder order;
        public DeliveryOrderEventChannel orderRequested;

        public override void Execute() => orderRequested.Raise(order);
    }
}
