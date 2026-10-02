using KindNeighbors.Core.Events;
using KindNeighbors.Home;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>소식지·일지를 띄우는 임시 읽기 화면 (IMGUI). 여는 동안 입력을 잠근다.</summary>
    public class ReaderUI : MonoBehaviour
    {
        [SerializeField] DocumentEventChannel documentRequested;
        [SerializeField] BoolEventChannel inputLock;
        [SerializeField] KeyCode closeKey = KeyCode.E;

        ReadableDocument current;
        bool open;
        int openedFrame;
        Vector2 scroll;
        GUIStyle paperStyle;
        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle hintStyle;

        void OnEnable() => documentRequested.Raised += Open;
        void OnDisable() => documentRequested.Raised -= Open;

        void Open(ReadableDocument document)
        {
            if (open)
                return;
            current = document;
            open = true;
            openedFrame = Time.frameCount;
            scroll = Vector2.zero;
            inputLock.Raise(true);
        }

        void Update()
        {
            // 연 키 입력이 같은 프레임에 바로 닫지 않도록 한다
            if (!open || Time.frameCount == openedFrame)
                return;

            if (Input.GetKeyDown(closeKey) || Input.GetKeyDown(KeyCode.Space))
            {
                open = false;
                inputLock.Raise(false);
            }
        }

        void OnGUI()
        {
            if (!open)
                return;

            if (paperStyle == null)
            {
                var paper = new Texture2D(1, 1);
                paper.SetPixel(0, 0, new Color(0.96f, 0.93f, 0.85f, 0.97f));
                paper.Apply();
                paperStyle = new GUIStyle(GUI.skin.box) { normal = { background = paper } };
                var ink = new Color(0.2f, 0.17f, 0.15f);
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = ink } };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true, normal = { textColor = ink } };
                hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleRight, normal = { textColor = new Color(ink.r, ink.g, ink.b, 0.6f) } };
            }

            float w = Mathf.Min(Screen.width * 0.6f, 720f);
            float h = Screen.height * 0.75f;
            var area = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(area, GUIContent.none, paperStyle);

            GUILayout.BeginArea(new Rect(area.x + 32, area.y + 24, area.width - 64, area.height - 48));
            GUILayout.Label(current.title, titleStyle);
            GUILayout.Space(16);
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label(current.body, bodyStyle);
            GUILayout.EndScrollView();
            GUILayout.Label("[E] 닫기", hintStyle);
            GUILayout.EndArea();
        }
    }
}
