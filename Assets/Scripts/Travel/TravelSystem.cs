using System.Collections;
using KindNeighbors.Core.Events;
using KindNeighbors.Player;
using KindNeighbors.UI;
using UnityEngine;

namespace KindNeighbors.Travel
{
    /// <summary>이동 요청을 받아 화면을 가린 채 플레이어를 SpawnPoint로 옮긴다. 전환 중에는 입력을 잠근다.</summary>
    public class TravelSystem : MonoBehaviour
    {
        [SerializeField] TravelEventChannel travelRequested;
        [SerializeField] BoolEventChannel inputLock;
        [SerializeField] PlayerController player;
        [SerializeField] ScreenFader fader;
        [SerializeField] float fadeOutTime = 0.35f;
        [SerializeField] float holdTime = 0.25f;
        [SerializeField] float fadeInTime = 0.6f;

        Coroutine running;

        void OnEnable() => travelRequested.Raised += OnTravelRequested;
        void OnDisable() => travelRequested.Raised -= OnTravelRequested;

        void OnTravelRequested(TravelRequest request)
        {
            SpawnPoint target = SpawnPoint.Find(request.spawnId);
            if (target == null)
            {
                Debug.LogWarning($"[Travel] SpawnPoint '{request.spawnId}' 를 찾을 수 없습니다.");
                return;
            }

            if (running != null)
            {
                StopCoroutine(running);
                inputLock.Raise(false);
            }
            running = StartCoroutine(Travel(target, request.cutToBlack));
        }

        IEnumerator Travel(SpawnPoint target, bool cutToBlack)
        {
            inputLock.Raise(true);

            if (cutToBlack)
                fader.Alpha = 1f;
            else
                yield return fader.Fade(1f, fadeOutTime);

            player.TeleportTo(target.transform.position, target.transform.eulerAngles.y);
            yield return new WaitForSecondsRealtime(holdTime);
            yield return fader.Fade(0f, fadeInTime);

            inputLock.Raise(false);
            running = null;
        }
    }
}
