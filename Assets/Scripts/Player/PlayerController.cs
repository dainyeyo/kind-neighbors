using UnityEngine;
using UnityEngine.Rendering;

namespace KindNeighbors.Player
{
    public enum ViewMode { FirstPerson, ThirdPerson }

    /// <summary>
    /// 1인칭/3인칭 전환이 가능한 플레이어 이동 + 카메라.
    /// 루트는 회전하지 않고, 카메라 피벗(yaw/pitch)과 바디(시각 모델)가 각각 회전한다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform body;
        [SerializeField] Transform cameraPivot;
        [SerializeField] Camera playerCamera;

        [Header("Movement")]
        [SerializeField] float walkSpeed = 3.5f;
        [SerializeField] float runSpeed = 6f;
        [SerializeField] float gravity = -20f;
        [SerializeField] float bodyTurnSpeed = 12f;

        [Header("Look")]
        [SerializeField] float mouseSensitivity = 2f;
        [SerializeField] float minPitch = -70f;
        [SerializeField] float maxPitch = 80f;

        [Header("View")]
        [SerializeField] ViewMode viewMode = ViewMode.ThirdPerson;
        [SerializeField] KeyCode toggleViewKey = KeyCode.V;
        [SerializeField] float thirdPersonDistance = 4f;
        [SerializeField] float cameraCollisionRadius = 0.2f;
        [SerializeField] LayerMask cameraCollisionMask = ~(1 << 2); // Ignore Raycast 레이어(플레이어) 제외

        CharacterController controller;
        Renderer[] bodyRenderers;
        float yaw;
        float pitch;
        float verticalVelocity;

        public ViewMode ViewMode => viewMode;
        public Camera Camera => playerCamera;
        public Transform CameraPivot => cameraPivot;

        /// <summary>대화·컷신 중에는 false로 두어 이동/시점 입력을 막는다.</summary>
        public bool InputEnabled { get; set; } = true;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            bodyRenderers = body.GetComponentsInChildren<Renderer>();
            yaw = body.eulerAngles.y;
        }

        void Start()
        {
            LockCursor(true);
            ApplyViewMode();
        }

        void Update()
        {
            HandleCursor();

            if (InputEnabled && Input.GetKeyDown(toggleViewKey))
                SetViewMode(viewMode == ViewMode.FirstPerson ? ViewMode.ThirdPerson : ViewMode.FirstPerson);

            if (InputEnabled && Cursor.lockState == CursorLockMode.Locked)
                Look();

            Move();
        }

        void LateUpdate()
        {
            cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
            PositionCamera();
        }

        public void SetViewMode(ViewMode mode)
        {
            viewMode = mode;
            ApplyViewMode();
        }

        void ApplyViewMode()
        {
            // 1인칭에서는 몸이 안 보이게 하되 그림자는 남긴다 (규칙 위반 시 그림자 연출용)
            var shadowMode = viewMode == ViewMode.FirstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            foreach (var r in bodyRenderers)
                r.shadowCastingMode = shadowMode;
        }

        void Look()
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        void Move()
        {
            Vector3 input = Vector3.zero;
            if (InputEnabled)
                input = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);

            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 move = yawRotation * input * speed;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;

            controller.Move((move + Vector3.up * verticalVelocity) * Time.deltaTime);

            if (viewMode == ViewMode.FirstPerson)
                body.rotation = yawRotation;
            else if (move.sqrMagnitude > 0.01f)
                body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(move), bodyTurnSpeed * Time.deltaTime);
        }

        void PositionCamera()
        {
            if (viewMode == ViewMode.FirstPerson)
            {
                playerCamera.transform.localPosition = Vector3.zero;
                return;
            }

            float distance = thirdPersonDistance;
            if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, -cameraPivot.forward, out RaycastHit hit,
                    thirdPersonDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
                distance = hit.distance;

            playerCamera.transform.localPosition = new Vector3(0f, 0f, -distance);
        }

        void HandleCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                LockCursor(false);
            else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
                LockCursor(true);
        }

        static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
