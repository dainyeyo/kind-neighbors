using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// 조건이 처음 참이 되면 잠시 뒤 자막을 한 번 띄운다 (예: 프롤로그에서 이름을 적은 뒤 "막차가 떠났다.").
    /// onceFlag가 켜져 있으면 다시 띄우지 않는다.
    /// </summary>
    public class ConditionalSubtitle : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] StringEventChannel subtitle;
        [SerializeField] FlagCondition[] conditions;
        [SerializeField] string text;
        [SerializeField] float delay = 0.8f;
        [Tooltip("띄운 뒤 켜는 플래그. 저장되므로 불러와도 다시 나오지 않는다.")]
        [SerializeField] string onceFlag;

        float conditionMetAt = -1f;

        void Update()
        {
            if (flags.GetBool(onceFlag))
                return;

            if (!FlagCondition.All(conditions, flags))
            {
                conditionMetAt = -1f;
                return;
            }

            if (conditionMetAt < 0f)
                conditionMetAt = Time.time;

            if (Time.time - conditionMetAt >= delay)
            {
                subtitle.Raise(text);
                flags.SetBool(onceFlag, true);
            }
        }
    }
}
