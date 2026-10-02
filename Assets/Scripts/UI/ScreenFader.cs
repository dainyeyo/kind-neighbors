using System.Collections;
using UnityEngine;

namespace KindNeighbors.UI
{
    /// <summary>화면 전체를 검게 덮는 페이드 (IMGUI). 다른 UI보다 위에 그려지도록 depth를 낮게 둔다.</summary>
    public class ScreenFader : MonoBehaviour
    {
        public float Alpha { get; set; }

        public IEnumerator Fade(float target, float duration)
        {
            float start = Alpha;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                Alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            Alpha = target;
        }

        void OnGUI()
        {
            if (Alpha <= 0f)
                return;

            GUI.depth = -100;
            GUI.color = new Color(0f, 0f, 0f, Alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
