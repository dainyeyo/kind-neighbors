using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using KindNeighbors.Flow;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>
    /// 특정 Day의 밤에 한 번 일어나는 이벤트의 공통 부분.
    /// 시간대가 해당 밤으로 바뀌면 준비(armed)되고, 끝나면 doneFlag를 켠다.
    /// 시간대의 완료 조건이 doneFlag를 보므로, 이벤트가 끝나야 잠자리에 들 수 있다.
    /// </summary>
    public abstract class NightEvent : MonoBehaviour
    {
        [SerializeField] protected GameFlags flags;
        [SerializeField] protected PhaseEventChannel phaseChanged;
        [SerializeField] protected StringEventChannel subtitle;
        [SerializeField] protected int day = 1;
        [SerializeField] protected string doneFlag;

        protected bool Armed { get; private set; }
        protected float ArmedTime { get; private set; }

        protected virtual void OnEnable() => phaseChanged.Raised += OnPhaseChanged;
        protected virtual void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void OnPhaseChanged(PhaseDefinition definition)
        {
            StopAllCoroutines();
            ResetEvent();
            Armed = definition.phase.day == day && definition.phase.time == TimeOfDay.Night && !flags.GetBool(doneFlag);
            ArmedTime = Time.time;
        }

        protected void Finish()
        {
            Armed = false;
            flags.SetBool(doneFlag, true);
            ResetEvent();
        }

        protected bool CurtainClosed => flags.GetBool(FlagKeys.CurtainClosed);
        protected bool LampOn => !flags.GetBool(FlagKeys.LampOff);

        /// <summary>연출용 오브젝트를 처음 상태로 되돌린다.</summary>
        protected abstract void ResetEvent();
    }
}
