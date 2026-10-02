using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>바람에 흔들리는 빨래, 깃발, 꽃. 오브젝트마다 위상을 달리해 한꺼번에 흔들리지 않게 한다.</summary>
    public class Sway : MonoBehaviour
    {
        [SerializeField] Vector3 axis = Vector3.forward;
        [SerializeField] float angle = 6f;
        [SerializeField] float speed = 1.3f;

        Quaternion rest;
        float phase;

        void Awake()
        {
            rest = transform.localRotation;
            phase = Random.value * Mathf.PI * 2f;
        }

        void Update() =>
            transform.localRotation = rest * Quaternion.AngleAxis(Mathf.Sin(Time.time * speed + phase) * angle, axis);
    }
}
