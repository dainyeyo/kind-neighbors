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

        public static string Accepted(string orderId) => $"accepted.{orderId}";
        public static string Delivered(string orderId) => $"delivered.{orderId}";
    }
}
