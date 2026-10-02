using KindNeighbors.Delivery;
using KindNeighbors.Interaction;
using KindNeighbors.Player;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>M0용 임시 HUD (IMGUI). UI 스타일이 정해지면 교체한다.</summary>
    public class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] PlayerInteractor interactor;
        [SerializeField] DeliveryManager deliveryManager;
        [SerializeField] float toastDuration = 2.5f;

        string toast;
        float toastUntil;
        GUIStyle labelStyle;
        GUIStyle centerStyle;

        void OnEnable()
        {
            deliveryManager.OrderAccepted += OnOrderAccepted;
            deliveryManager.OrderCompleted += OnOrderCompleted;
        }

        void OnDisable()
        {
            deliveryManager.OrderAccepted -= OnOrderAccepted;
            deliveryManager.OrderCompleted -= OnOrderCompleted;
        }

        void OnOrderAccepted(DeliveryOrder order) => ShowToast($"{order.clientName}에게서 {order.itemName}을(를) 받았다");
        void OnOrderCompleted(DeliveryOrder order) => ShowToast($"배달 완료! ({deliveryManager.Completed.Count}건)");

        void ShowToast(string message)
        {
            toast = message;
            toastUntil = Time.time + toastDuration;
        }

        void OnGUI()
        {
            labelStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };
            centerStyle ??= new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };

            float w = Screen.width, h = Screen.height;

            // 조준점
            GUI.Label(new Rect(w / 2 - 10, h / 2 - 12, 20, 24), "·", centerStyle);

            // 상호작용 안내
            if (interactor.Current != null)
                GUI.Label(new Rect(0, h / 2 + 30, w, 30), $"[E] {interactor.Current.Prompt}", centerStyle);

            // 현재 주문
            DeliveryOrder order = deliveryManager.CurrentOrder;
            string orderText = order != null
                ? $"배달 중: {order.itemName} → {order.recipientName}"
                : "배달 없음";
            GUI.Label(new Rect(16, 12, 600, 28), orderText, labelStyle);
            GUI.Label(new Rect(16, 38, 600, 28), $"완료: {deliveryManager.Completed.Count}건", labelStyle);

            // 알림
            if (!string.IsNullOrEmpty(toast) && Time.time < toastUntil)
                GUI.Label(new Rect(0, h * 0.25f, w, 30), toast, centerStyle);

            // 조작 안내
            string view = player.ViewMode == ViewMode.FirstPerson ? "1인칭" : "3인칭";
            GUI.Label(new Rect(16, h - 36, w, 28),
                $"WASD 이동 · Shift 달리기 · V 시점 전환({view}) · E 상호작용 · Esc 커서 해제", labelStyle);
        }
    }
}
