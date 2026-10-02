using KindNeighbors.Delivery;
using KindNeighbors.Flow;
using KindNeighbors.Interaction;
using KindNeighbors.Player;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>임시 HUD (IMGUI). UI 스타일이 정해지면 교체한다. 표시할 상태는 이벤트 채널로만 받는다.</summary>
    public class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] PlayerInteractor interactor;
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] DeliveryOrderEventChannel orderAccepted;
        [SerializeField] DeliveryOrderEventChannel orderCompleted;
        [SerializeField] float toastDuration = 2.5f;

        string phaseText = string.Empty;
        DeliveryOrder currentOrder;
        string toast;
        float toastUntil;
        GUIStyle labelStyle;
        GUIStyle centerStyle;
        GUIStyle toastStyle;

        void OnEnable()
        {
            phaseChanged.Raised += OnPhaseChanged;
            orderAccepted.Raised += OnOrderAccepted;
            orderCompleted.Raised += OnOrderCompleted;
        }

        void OnDisable()
        {
            phaseChanged.Raised -= OnPhaseChanged;
            orderAccepted.Raised -= OnOrderAccepted;
            orderCompleted.Raised -= OnOrderCompleted;
        }

        void OnPhaseChanged(PhaseDefinition definition)
        {
            phaseText = definition.phase.ToString();
            ShowToast(phaseText);
        }

        void OnOrderAccepted(DeliveryOrder order)
        {
            currentOrder = order;
            ShowToast($"{order.clientName}에게서 {order.itemName}을(를) 받았다");
        }

        void OnOrderCompleted(DeliveryOrder order)
        {
            currentOrder = null;
            ShowToast("배달 완료!");
        }

        void ShowToast(string message)
        {
            toast = message;
            toastUntil = Time.time + toastDuration;
        }

        void OnGUI()
        {
            labelStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };
            centerStyle ??= new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
            toastStyle ??= new GUIStyle(centerStyle) { fontSize = 26, fontStyle = FontStyle.Bold };

            float w = Screen.width, h = Screen.height;

            // 조준점
            GUI.Label(new Rect(w / 2 - 10, h / 2 - 12, 20, 24), "·", centerStyle);

            // 상호작용 안내
            if (interactor.Current != null)
                GUI.Label(new Rect(0, h / 2 + 30, w, 30), $"[E] {interactor.Current.Prompt}", centerStyle);

            // 시간대 + 현재 주문
            GUI.Label(new Rect(16, 12, 600, 28), phaseText, labelStyle);
            string orderText = currentOrder != null ? $"배달 중: {currentOrder.itemName} → {currentOrder.recipientName}" : "배달 없음";
            GUI.Label(new Rect(16, 38, 600, 28), orderText, labelStyle);

            // 알림
            if (!string.IsNullOrEmpty(toast) && Time.time < toastUntil)
                GUI.Label(new Rect(0, h * 0.22f, w, 40), toast, toastStyle);

            // 조작 안내
            string view = player.ViewMode == ViewMode.FirstPerson ? "1인칭" : "3인칭";
            GUI.Label(new Rect(16, h - 36, w, 28),
                $"WASD 이동 · Shift 달리기 · V 시점 전환({view}) · E 상호작용/계속 · 1~9 선택지 · Esc 커서 해제", labelStyle);
        }
    }
}
