using System;

namespace KindNeighbors.Flow
{
    public enum TimeOfDay { Morning, Evening, Night }

    /// <summary>Day + 시간대. 예: DAY 1 · 낮</summary>
    [Serializable]
    public struct GamePhase
    {
        public int day;
        public TimeOfDay time;

        public GamePhase(int day, TimeOfDay time)
        {
            this.day = day;
            this.time = time;
        }

        public bool Matches(int otherDay, TimeOfDay otherTime) => day == otherDay && time == otherTime;

        public string TimeLabel => time switch
        {
            TimeOfDay.Morning => "낮",
            TimeOfDay.Evening => "저녁",
            _ => "밤",
        };

        /// <summary>Day 0은 프롤로그(마을에 도착한 날 저녁).</summary>
        public override string ToString() => day == 0 ? "프롤로그" : $"DAY {day} · {TimeLabel}";
    }
}
