using KindNeighbors.Core.Flags;
using KindNeighbors.Interaction;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>읽을 수 있는 물건 (우편함의 소식지, 책상 위 일지, 가방 속 일지).</summary>
    public class ReadableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] DocumentAsset document;
        [SerializeField] string prompt = "읽기";
        [SerializeField] GameFlags flags;
        [SerializeField] FlagCondition[] conditions;
        [Tooltip("읽었을 때 적용 (예: read.news.d1 = 1)")]
        [SerializeField] FlagEffect[] onRead;
        [SerializeField] DocumentEventChannel documentRequested;

        public string Prompt => prompt;

        public bool CanInteract(PlayerInteractor interactor) => FlagCondition.All(conditions, flags);

        public void Interact(PlayerInteractor interactor)
        {
            documentRequested.Raise(document.Compose(flags));
            FlagEffect.ApplyAll(onRead, flags);
        }
    }
}
