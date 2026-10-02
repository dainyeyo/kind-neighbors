using System.Collections;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>
    /// DAY 1 밤: 창밖으로 길쭉한 실루엣이 한 번 지나간다. 상호작용은 없다.
    /// - 커튼을 걷어 두면 창 바로 앞을 지나가며 잠깐 멈춘다. 불을 켜 두면 더 오래 머문다.
    /// - 커튼을 쳐 두면 모습은 보이지 않고 발소리 자막만 나온다.
    /// 플레이어가 창 쪽을 볼 때 시작하고, 끝내 보지 않으면 일정 시간 뒤 그냥 지나간다.
    /// </summary>
    public class SilhouetteEvent : NightEvent
    {
        [SerializeField] GameObject figure;
        [SerializeField] Transform[] path;
        [Tooltip("path 중 창 앞에서 멈추는 지점")]
        [SerializeField] int stopIndex = 1;
        [SerializeField] Transform window;
        [SerializeField] float startDelay = 3f;
        [SerializeField] float forceStartAfter = 15f;
        [SerializeField] float lookAngle = 30f;
        [SerializeField] float walkSpeed = 1.1f;
        [SerializeField] float pauseWithLamp = 3.5f;
        [SerializeField] float pauseWithoutLamp = 1.2f;
        [SerializeField] string curtainClosedSubtitle = "…창밖에서 느린 발소리가 지나간다.";
        [Tooltip("모습이 창에 드러났을 때 켜는 플래그 (일지 문장이 달라진다)")]
        [SerializeField] string seenFlag = "seen.d1_silhouette";

        bool playing;

        void Update()
        {
            if (!Armed || playing)
                return;

            float elapsed = Time.time - ArmedTime;
            if (elapsed < startDelay)
                return;

            if (CurtainClosed || elapsed > forceStartAfter || IsLookingAtWindow())
                StartCoroutine(Play());
        }

        bool IsLookingAtWindow()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return false;
            Vector3 toWindow = window.position - cam.transform.position;
            return toWindow.magnitude < 10f && Vector3.Angle(cam.transform.forward, toWindow) < lookAngle;
        }

        IEnumerator Play()
        {
            playing = true;

            if (CurtainClosed)
            {
                subtitle.Raise(curtainClosedSubtitle);
                yield return new WaitForSeconds(4f);
                Finish();
                yield break;
            }

            figure.SetActive(true);
            figure.transform.position = path[0].position;
            flags.SetBool(seenFlag, true);
            for (int i = 1; i < path.Length; i++)
            {
                yield return WalkTo(path[i].position);
                if (i == stopIndex)
                {
                    // 창 쪽을 돌아본다
                    Vector3 look = window.position - figure.transform.position;
                    look.y = 0f;
                    figure.transform.rotation = Quaternion.LookRotation(look);
                    yield return new WaitForSeconds(LampOn ? pauseWithLamp : pauseWithoutLamp);
                }
            }
            Finish();
        }

        IEnumerator WalkTo(Vector3 target)
        {
            Transform t = figure.transform;
            Vector3 dir = target - t.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                t.rotation = Quaternion.LookRotation(dir);

            while ((t.position - target).sqrMagnitude > 0.0025f)
            {
                t.position = Vector3.MoveTowards(t.position, target, walkSpeed * Time.deltaTime);
                yield return null;
            }
        }

        protected override void ResetEvent()
        {
            playing = false;
            if (figure != null)
                figure.SetActive(false);
        }
    }
}
