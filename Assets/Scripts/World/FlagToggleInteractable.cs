using KindNeighbors.Core.Flags;
using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// 상호작용할 때마다 bool 플래그를 뒤집는다 (커튼 치기/걷기, 불 끄기/켜기, 문 잠그기/풀기).
    /// 보이는 변화는 FlagObjectToggle이, 결과는 그 플래그를 읽는 밤 이벤트가 맡는다.
    /// </summary>
    public class FlagToggleInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] GameFlags flags;
        [SerializeField] string flagKey;
        [Tooltip("플래그가 0일 때 문구 (예: 커튼 치기)")]
        [SerializeField] string promptWhenOff;
        [Tooltip("플래그가 1일 때 문구 (예: 커튼 걷기)")]
        [SerializeField] string promptWhenOn;
        [SerializeField] FlagCondition[] conditions;

        public string Prompt => flags.GetBool(flagKey) ? promptWhenOn : promptWhenOff;

        public bool CanInteract(PlayerInteractor interactor) => FlagCondition.All(conditions, flags);

        public void Interact(PlayerInteractor interactor) => flags.SetBool(flagKey, !flags.GetBool(flagKey));
    }
}
