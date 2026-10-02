using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>
    /// 구인 전단의 이름 칸을 채우는 화면 (임시 IMGUI). 프롤로그에서 전단을 손에 든 채 시작한다.
    /// 세이브에 이름이 있으면 표시되지 않는다.
    /// </summary>
    public class NameEntryUI : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] BoolEventChannel inputLockRequested;
        [SerializeField] string defaultName = "";
        [SerializeField] int maxLength = 8;
        [TextArea(3, 8)]
        [SerializeField] string flyerText =
            "숲가 빵집 배달원 구함\n\n" +
            "· 숙식 제공\n" +
            "· 경험 무관\n" +
            "· 아무것도 묻지 않습니다\n\n" +
            "이름만 적어 오세요.";

        string input;
        bool showing;
        GUIStyle paperStyle;
        GUIStyle bodyStyle;
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

            if (paperStyle == null)
            {
                var paper = new Texture2D(1, 1);
                paper.SetPixel(0, 0, new Color(0.95f, 0.92f, 0.82f));
                paper.Apply();
                var ink = new Color(0.22f, 0.18f, 0.15f);
                paperStyle = new GUIStyle(GUI.skin.box) { normal = { background = paper } };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true, alignment = TextAnchor.UpperCenter, normal = { textColor = ink } };
                fieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = 24, alignment = TextAnchor.MiddleCenter };
                hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(ink.r, ink.g, ink.b, 0.6f) } };
            }

            float w = Mathf.Min(Screen.width * 0.42f, 460f);
            float h = Mathf.Min(Screen.height * 0.72f, 560f);
            var area = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(area, GUIContent.none, paperStyle);
            GUI.Label(new Rect(area.x + 24, area.y + 28, area.width - 48, area.height * 0.6f), flyerText, bodyStyle);

            // 엔터 키는 텍스트 필드가 이벤트를 먹기 전에 확인한다
            Event e = Event.current;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                Confirm();
                e.Use();
                return;
            }

            float fieldY = area.y + area.height * 0.68f;
            GUI.Label(new Rect(area.x, fieldY - 30, area.width, 26), "이름:", hintStyle);
            GUI.SetNextControlName("NameField");
            input = GUI.TextField(new Rect(area.x + 50, fieldY, area.width - 100, 44), input, maxLength, fieldStyle);
            GUI.FocusControl("NameField");
            GUI.Label(new Rect(area.x, fieldY + 56, area.width, 24), "Enter 로 적기", hintStyle);
        }

        void Confirm()
        {
            string name = input.Trim();
            if (name.Length == 0)
                return;

            flags.SetText(FlagKeys.PlayerName, name);
            flags.SetBool(FlagKeys.HasName, true);
            showing = false;
            inputLockRequested.Raise(false);
        }
    }
}
