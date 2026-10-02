using System;
using System.Collections.Generic;
using KindNeighbors.Core.Flags;
using KindNeighbors.Flow;

namespace KindNeighbors.Save
{
    /// <summary>
    /// 세이브 파일 형식. JsonUtility는 Dictionary를 직렬화하지 못하므로 플래그는 key/value 목록으로 저장한다.
    /// 형식이 바뀌면 CurrentVersion을 올리고 SaveSystem.Migrate에 변환 코드를 추가한다.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        [Serializable]
        public struct FlagEntry
        {
            public string key;
            public int value;
        }

        public int version = CurrentVersion;
        public int day;
        public TimeOfDay time;
        public List<FlagEntry> flags = new();

        public GamePhase Phase => new(day, time);

        public static SaveData Capture(GamePhase phase, GameFlags gameFlags)
        {
            var data = new SaveData { day = phase.day, time = phase.time };
            foreach (var pair in gameFlags.All)
                data.flags.Add(new FlagEntry { key = pair.Key, value = pair.Value });
            return data;
        }

        public void ApplyFlags(GameFlags gameFlags)
        {
            gameFlags.Clear();
            foreach (var entry in flags)
                gameFlags.SetInt(entry.key, entry.value);
        }
    }
}
