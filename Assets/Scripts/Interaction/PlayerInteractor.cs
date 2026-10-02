using KindNeighbors.Player;
using UnityEngine;

namespace KindNeighbors.Interaction
{
    /// <summary>
    /// 카메라 중앙에서 Raycast로 IInteractable을 찾는다.
    /// 3인칭에서도 거리 기준은 플레이어 몸이므로, 카메라→플레이어 거리만큼 레이를 늘리고 맞은 지점을 플레이어 기준으로 다시 검사한다.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] float interactRange = 2.5f;
        [SerializeField] KeyCode interactKey = KeyCode.E;

        public IInteractable Current { get; private set; }

        void Update()
        {
            Current = player.InputEnabled ? FindTarget() : null;

            if (Current != null && Input.GetKeyDown(interactKey))
                Current.Interact(this);
        }

        IInteractable FindTarget()
        {
            Transform cam = player.Camera.transform;
            Vector3 eye = player.CameraPivot.position;
            float maxDistance = Vector3.Distance(cam.position, eye) + interactRange;

            if (!Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return null;

            if (Vector3.Distance(eye, hit.point) > interactRange)
                return null;

            var target = hit.collider.GetComponentInParent<IInteractable>();
            return target != null && target.CanInteract(this) ? target : null;
        }
    }
}
