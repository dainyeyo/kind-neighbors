using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>
    /// DAY 2 밤: 창문을 두 번씩 두드리고, 낮에 만난 주민의 말투를 흉내 내는 목소리가 들린다.
    /// 두드리는 동안 activeFlag가 켜져 있고, 창문의 대화(열기/열지 않기)가 이 플래그를 끈다.
    /// 끝까지 반응하지 않으면 시간이 지나 소리가 멎고 '무시'로 처리된다.
    /// </summary>
    public class WindowKnockEvent : NightEvent
    {
        [SerializeField] string activeFlag = "knock.d2";
        [SerializeField] string ignoredFlag = "window.ignored.d2";
        [Tooltip("창 바로 앞에 서 있는 모습. 커튼이 걷혀 있을 때만 보인다.")]
        [SerializeField] GameObject figure;
        [SerializeField] Transform windowVisual;
        [SerializeField] float startDelay = 5f;
        [SerializeField] float lineInterval = 4f;
        [SerializeField] float giveUpAfter = 40f;
        [SerializeField] string[] lines =
        {
            "똑똑. 똑똑.",
            "배달 왔어요~",
            "똑똑. 똑똑.",
            "따뜻한 빵이에요~ 창문 좀 열어 줄래요~?",
        };
        [SerializeField] string giveUpSubtitle = "…소리가 멎었다.";

        bool knocking;
        float knockStart;
        float nextLineTime;
        float lastKnockTime = -10f;
        int lineIndex;
        Vector3 windowRest;

        void Awake() => windowRest = windowVisual.localPosition;

        void Update()
        {
            if (!knocking)
            {
                if (Armed && Time.time - ArmedTime >= startDelay)
                    BeginKnocking();
                return;
            }

            // 창문 대화에서 열었거나 열지 않기로 했다
            if (!flags.GetBool(activeFlag))
            {
                Finish();
                return;
            }

            figure.SetActive(!CurtainClosed);

            if (Time.time >= nextLineTime)
            {
                string line = lines[lineIndex++ % lines.Length];
                subtitle.Raise(line);
                if (line.StartsWith("똑똑"))
                    lastKnockTime = Time.time;
                nextLineTime = Time.time + lineInterval;
            }

            // 두드릴 때 창이 살짝 흔들린다
            float shake = Time.time - lastKnockTime < 0.5f ? Mathf.Sin(Time.time * 60f) * 0.015f : 0f;
            windowVisual.localPosition = windowRest + new Vector3(0f, 0f, shake);

            if (Time.time - knockStart > giveUpAfter)
            {
                flags.SetBool(activeFlag, false);
                flags.SetBool(ignoredFlag, true);
                subtitle.Raise(giveUpSubtitle);
                Finish();
            }
        }

        void BeginKnocking()
        {
            knocking = true;
            knockStart = Time.time;
            nextLineTime = Time.time;
            lineIndex = 0;
            flags.SetBool(activeFlag, true);
        }

        protected override void ResetEvent()
        {
            knocking = false;
            if (figure != null)
                figure.SetActive(false);
            if (windowVisual != null)
                windowVisual.localPosition = windowRest;
        }
    }
}
