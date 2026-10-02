using KindNeighbors.Dialogue;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>M1용 임시 대화창 (IMGUI). UI 스타일이 정해지면 교체한다.</summary>
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] DialogueRunner runner;

        GUIStyle boxStyle;
        GUIStyle speakerStyle;
        GUIStyle textStyle;

        void OnGUI()
        {
            if (!runner.IsRunning)
                return;

            if (boxStyle == null)
            {
                var background = new Texture2D(1, 1);
                background.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
                background.Apply();
                boxStyle = new GUIStyle(GUI.skin.box) { normal = { background = background } };
                speakerStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.85f, 0.6f) } };
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, normal = { textColor = Color.white } };
            }

            float w = Screen.width, h = Screen.height;
            var area = new Rect(w * 0.1f, h * 0.68f, w * 0.8f, h * 0.28f);
            GUI.Box(area, GUIContent.none, boxStyle);

            GUILayout.BeginArea(new Rect(area.x + 20, area.y + 14, area.width - 40, area.height - 28));
            GUILayout.Label(runner.CurrentSpeaker, speakerStyle);
            GUILayout.Label(runner.Current.text, textStyle);
            GUILayout.FlexibleSpace();

            if (runner.Choices.Count > 0)
            {
                for (int i = 0; i < runner.Choices.Count; i++)
                    GUILayout.Label($"{i + 1}. {runner.Choices[i].text}", textStyle);
            }
            else
            {
                GUILayout.Label("[E] 계속", textStyle);
            }
            GUILayout.EndArea();
        }
    }
}
