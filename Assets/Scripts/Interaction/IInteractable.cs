namespace KindNeighbors.Interaction
{
    /// <summary>플레이어가 바라보고 상호작용 키를 눌러 사용할 수 있는 대상 (NPC, 배달물, 문, 창문, 우편함).</summary>
    public interface IInteractable
    {
        /// <summary>HUD에 표시할 행동 이름. 예: "주문 받기"</summary>
        string Prompt { get; }

        bool CanInteract(PlayerInteractor interactor);

        void Interact(PlayerInteractor interactor);
    }
}
