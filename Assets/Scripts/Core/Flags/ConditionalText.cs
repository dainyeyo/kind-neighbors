using System;
using System.Collections.Generic;
using System.Text;
using KindNeighbors.Core.Text;
using UnityEngine;

namespace KindNeighbors.Core.Flags
{
    /// <summary>조건이 참일 때만 보이는 문장 한 줄. 소식지, 일지처럼 상태에 따라 내용이 달라지는 글에 쓴다.</summary>
    [Serializable]
    public class ConditionalText
    {
        public FlagCondition[] conditions;
        [TextArea(1, 4)] public string text;

        public ConditionalText() { }

        public ConditionalText(string text, params FlagCondition[] conditions)
        {
            this.text = text;
            this.conditions = conditions;
        }

        /// <summary>조건이 참인 문장만 순서대로 이어 붙인다. 하나도 없으면 빈 문자열.</summary>
        public static string Compose(IReadOnlyList<ConditionalText> lines, GameFlags flags, string separator = "\n")
        {
            var builder = new StringBuilder();
            foreach (var line in lines)
            {
                if (!FlagCondition.All(line.conditions, flags))
                    continue;
                if (builder.Length > 0)
                    builder.Append(separator);
                builder.Append(TextFormatter.Format(line.text, flags));
            }
            return builder.ToString();
        }
    }
}
