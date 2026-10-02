namespace KindNeighbors.Core.Flags
{
    /// <summary>코드에서 쓰는 플래그 키. 데이터(대화 조건 등)에서도 같은 문자열을 써야 한다.</summary>
    public static class FlagKeys
    {
        /// <summary>현재 Day (1~3). GameFlowController가 갱신한다.</summary>
        public const string Day = "day";

        /// <summary>현재 시간대. 0 = 낮, 1 = 저녁, 2 = 밤 (TimeOfDay 값).</summary>
        public const string TimeOfDay = "time";

        /// <summary>규칙 위반 횟수. 엔딩 판정에 쓴다.</summary>
        public const string RuleViolations = "rule_violations";

        /// <summary>시작할 때 입력받는 플레이어 이름 (텍스트 플래그). 대사에서는 {player} 로 쓴다.</summary>
        public const string PlayerName = "player_name";

        /// <summary>이름을 적었는가 (조건은 숫자 플래그만 볼 수 있으므로 따로 둔다).</summary>
        public const string HasName = "has.name";

        // 잠들기 전 루틴. 규칙 위반으로 세지 않고, 밤 이벤트가 읽는다.
        public const string CurtainClosed = "curtain.closed";
        public const string LampOff = "lamp.off";
        public const string DoorLocked = "door.locked";

        public static string Accepted(string orderId) => $"accepted.{orderId}";
        public static string Delivered(string orderId) => $"delivered.{orderId}";
    }
}
