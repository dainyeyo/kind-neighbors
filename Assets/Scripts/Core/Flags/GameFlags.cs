using System;
using System.Collections.Generic;
using UnityEngine;

namespace KindNeighbors.Core.Flags
{
    /// <summary>
    /// 게임 상태 전체를 담는 플래그 저장소. bool은 0/1 int로 저장하고, 이름처럼 숫자가 아닌 값은 텍스트로 따로 둔다.
    /// 런타임 데이터라 에셋에 직렬화되지 않으며, GameFlowController가 시작할 때 비우거나 세이브에서 채운다.
    /// </summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Game Flags", fileName = "GameFlags")]
    public class GameFlags : ScriptableObject
    {
        readonly Dictionary<string, int> values = new();
        readonly Dictionary<string, string> texts = new();

        /// <summary>(key, 새 값)</summary>
        public event Action<string, int> Changed;

        public IEnumerable<KeyValuePair<string, int>> All => values;
        public IEnumerable<KeyValuePair<string, string>> AllTexts => texts;

        public int GetInt(string key) => values.TryGetValue(key, out int value) ? value : 0;

        public bool GetBool(string key) => GetInt(key) != 0;

        public void SetInt(string key, int value)
        {
            if (values.TryGetValue(key, out int old) && old == value)
                return;
            values[key] = value;
            Changed?.Invoke(key, value);
        }

        public void SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);

        public void AddInt(string key, int delta) => SetInt(key, GetInt(key) + delta);

        public string GetText(string key) => texts.TryGetValue(key, out string value) ? value : string.Empty;

        public void SetText(string key, string value) => texts[key] = value;

        public void Clear()
        {
            values.Clear();
            texts.Clear();
        }
    }
}
