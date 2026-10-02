using KindNeighbors.Player;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// 마을 영역. 저녁 귀가 러시처럼 "플레이어가 마을에 있을 때 보여야 하는" 연출이
    /// 플레이어가 빵집 안에 있는 동안 끝나 버리지 않도록 판단에 쓴다.
    /// </summary>
    public class VillageArea : MonoBehaviour
    {
        [SerializeField] float radius = 50f;

        static VillageArea instance;
        static Transform player;

        void OnEnable() => instance = this;
        void OnDisable()
        {
            if (instance == this)
                instance = null;
        }

        public static bool PlayerInside
        {
            get
            {
                if (instance == null)
                    return false;
                if (player == null)
                {
                    var controller = FindFirstObjectByType<PlayerController>();
                    if (controller == null)
                        return false;
                    player = controller.transform;
                }
                Vector3 offset = player.position - instance.transform.position;
                offset.y = 0f;
                return offset.sqrMagnitude <= instance.radius * instance.radius;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.9f, 0.5f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
