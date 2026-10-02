using System;
using KindNeighbors.Core;
using KindNeighbors.Core.Flags;

namespace KindNeighbors.Flow
{
    /// <summary>시간대 하나를 나타내는 상태. 진입 시 Day/시간대 플래그를 갱신하고 변경을 알린다.</summary>
    public class PhaseState : IState
    {
        readonly GameFlags flags;
        readonly PhaseEventChannel phaseChanged;
        readonly Action onAutoComplete;

        public PhaseDefinition Definition { get; }

        public bool IsComplete => FlagCondition.All(Definition.completeWhen, flags);

        public PhaseState(PhaseDefinition definition, GameFlags flags, PhaseEventChannel phaseChanged, Action onAutoComplete)
        {
            Definition = definition;
            this.flags = flags;
            this.phaseChanged = phaseChanged;
            this.onAutoComplete = onAutoComplete;
        }

        public void Enter()
        {
            flags.SetInt(FlagKeys.Day, Definition.phase.day);
            flags.SetInt(FlagKeys.TimeOfDay, (int)Definition.phase.time);
            FlagEffect.ApplyAll(Definition.onEnter, flags);
            phaseChanged.Raise(Definition);
        }

        public void Tick()
        {
            if (Definition.AutoAdvance && IsComplete)
                onAutoComplete();
        }

        public void Exit() { }
    }
}
