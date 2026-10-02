using System.Collections.Generic;
using UnityEngine;

namespace KindNeighbors.Travel
{
    /// <summary>플레이어가 나타나는 지점. 방향은 transform의 forward.</summary>
    public class SpawnPoint : MonoBehaviour
    {
        static readonly List<SpawnPoint> active = new();

        [SerializeField] string id;

        public string Id => id;

        void OnEnable() => active.Add(this);
        void OnDisable() => active.Remove(this);

        public static SpawnPoint Find(string spawnId) => active.Find(s => s.id == spawnId);

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.up * 0.1f + transform.forward * 0.8f);
        }
    }
}
