using KindNeighbors.Core.Events;
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
        [Tooltip("true가 오면 입력을 잠근다 (대화, 컷신)")]
        [SerializeField] BoolEventChannel inputLockRequested;

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
        [SerializeField] float shoulderOffset = 0.6f;
        [SerializeField] float cameraCollisionRadius = 0.2f;
        [SerializeField] LayerMask cameraCollisionMask = ~(1 << 2); // Ignore Raycast 레이어(플레이어) 제외
        [SerializeField] float hideBodyDistance = 1.2f;

        CharacterController controller;
        Renderer[] bodyRenderers;
        float yaw;
        float pitch;
        float verticalVelocity;
        bool bodyHidden;
        int lockCount;
        int unlockFrame = -1;

        public ViewMode ViewMode => viewMode;
        public Camera Camera => playerCamera;
        public Transform CameraPivot => cameraPivot;

        /// <summary>
        /// 이동/시점/상호작용 입력을 받을 수 있는가.
        /// 잠금이 풀린 프레임에는 아직 false라서, 대화를 끝낸 E 키가 같은 프레임에 다시 상호작용하지 않는다.
        /// </summary>
        public bool InputEnabled => lockCount == 0 && Time.frameCount > unlockFrame;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            bodyRenderers = body.GetComponentsInChildren<Renderer>();
            yaw = body.eulerAngles.y;
        }

        void OnEnable()
        {
            if (inputLockRequested != null)
                inputLockRequested.Raised += SetInputLocked;
        }

        void OnDisable()
        {
            if (inputLockRequested != null)
                inputLockRequested.Raised -= SetInputLocked;
        }

        /// <summary>
        /// 잠금 요청은 겹칠 수 있으므로(예: 화면 전환 중 대화) 개수로 센다. true와 false는 반드시 짝을 이뤄야 한다.
        /// </summary>
        public void SetInputLocked(bool locked)
        {
            lockCount = Mathf.Max(0, lockCount + (locked ? 1 : -1));
            if (lockCount == 0)
                unlockFrame = Time.frameCount;
        }

        /// <summary>순간이동 (집 안팎 전환). CharacterController가 켜져 있으면 위치 지정이 무시되므로 잠시 끈다.</summary>
        public void TeleportTo(Vector3 position, float facingYaw)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;

            yaw = facingYaw;
            pitch = 0f;
            verticalVelocity = 0f;
            body.rotation = Quaternion.Euler(0f, yaw, 0f);
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

        void ApplyViewMode() => SetBodyHidden(viewMode == ViewMode.FirstPerson);

        /// <summary>몸을 숨겨도 그림자는 남긴다 (규칙 위반 시 그림자 연출용).</summary>
        void SetBodyHidden(bool hidden)
        {
            if (bodyHidden == hidden)
                return;
            bodyHidden = hidden;
            var shadowMode = hidden ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
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

            // 어깨 너머 시점: 카메라를 옆으로 비켜 두어 화면 가운데(창문, NPC)를 몸이 가리지 않게 한다
            Vector3 desiredLocal = new(shoulderOffset, 0f, -thirdPersonDistance);
            Vector3 desired = cameraPivot.TransformPoint(desiredLocal);
            Vector3 toDesired = desired - cameraPivot.position;
            float fraction = 1f;
            if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, toDesired.normalized, out RaycastHit hit,
                    toDesired.magnitude, cameraCollisionMask, QueryTriggerInteraction.Ignore))
                fraction = hit.distance / toDesired.magnitude;

            playerCamera.transform.localPosition = desiredLocal * fraction;
            float distance = thirdPersonDistance * fraction;

            // 좁은 집 안처럼 벽에 밀려 카메라가 몸에 붙으면 몸이 화면을 가리므로 숨긴다
            SetBodyHidden(distance < hideBodyDistance);
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
