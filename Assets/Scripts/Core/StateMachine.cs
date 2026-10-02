namespace KindNeighbors.Core
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }

    /// <summary>현재 상태 하나만 가지는 단순한 유한 상태 머신. 전이 시 Exit → Enter 순서를 보장한다.</summary>
    public class StateMachine
    {
        public IState Current { get; private set; }

        public void ChangeState(IState next)
        {
            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick() => Current?.Tick();
    }
}
