using KindNeighbors.Core.Flags;

namespace KindNeighbors.Core.Text
{
    /// <summary>데이터 문장 안의 토큰을 현재 상태로 바꾼다. {player} → 플레이어 이름</summary>
    public static class TextFormatter
    {
        public static string Format(string text, GameFlags flags) =>
            string.IsNullOrEmpty(text) ? string.Empty : text.Replace("{player}", flags.GetText(FlagKeys.PlayerName));
    }
}
