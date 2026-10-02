using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>
    /// 새 게임에서 플레이어 이름을 입력받는다 (임시 IMGUI). 이름이 정해지면 GameFlowController가 첫 시간대를 시작한다.
    /// 세이브에 이름이 있으면 표시되지 않는다.
    /// </summary>
    public class NameEntryUI : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] BoolEventChannel inputLockRequested;
        [SerializeField] string defaultName = "배달원";
        [SerializeField] int maxLength = 8;

        string input;
        bool showing;
        GUIStyle titleStyle;
        GUIStyle fieldStyle;
        GUIStyle hintStyle;

        bool NeedsName => string.IsNullOrEmpty(flags.GetText(FlagKeys.PlayerName));

        void Start() => input = defaultName;

        void Update()
        {
            // GameFlowController.Start에서 세이브를 불러온 뒤에 판단해야 하므로 Start가 아닌 Update에서 확인한다
            if (!showing && NeedsName)
            {
                showing = true;
                inputLockRequested.Raise(true);
            }
        }

        void OnGUI()
        {
            if (!showing)
                return;

            titleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            fieldStyle ??= new GUIStyle(GUI.skin.textField) { fontSize = 24, alignment = TextAnchor.MiddleCenter };
            hintStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 1f, 1f, 0.7f) } };

            float w = Screen.width, h = Screen.height;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(0, h * 0.35f, w, 40), "새로 온 배달원의 이름은?", titleStyle);

            // 엔터 키는 텍스트 필드가 이벤트를 먹기 전에 확인한다
            Event e = Event.current;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                Confirm();
                e.Use();
                return;
            }

            GUI.SetNextControlName("NameField");
            input = GUI.TextField(new Rect(w / 2 - 150, h * 0.45f, 300, 44), input, maxLength, fieldStyle);
            GUI.FocusControl("NameField");

            GUI.Label(new Rect(0, h * 0.45f + 56, w, 30), "Enter 로 시작", hintStyle);
        }

        void Confirm()
        {
            string name = input.Trim();
            if (name.Length == 0)
                return;

            flags.SetText(FlagKeys.PlayerName, name);
            showing = false;
            inputLockRequested.Raise(false);
        }
    }
}
