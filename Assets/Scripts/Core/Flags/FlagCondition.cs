using System;
using System.Collections.Generic;

namespace KindNeighbors.Core.Flags
{
    public enum Comparison { Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual }

    /// <summary>"플래그 key 가 value 와 비교해 참인가". 대화 시작 조건, 시간대 완료 조건 등에 쓴다.</summary>
    [Serializable]
    public struct FlagCondition
    {
        public string key;
        public Comparison comparison;
        public int value;

        public FlagCondition(string key, Comparison comparison, int value)
        {
            this.key = key;
            this.comparison = comparison;
            this.value = value;
        }

        public bool Evaluate(GameFlags flags)
        {
            int current = flags.GetInt(key);
            return comparison switch
            {
                Comparison.Equal => current == value,
                Comparison.NotEqual => current != value,
                Comparison.Greater => current > value,
                Comparison.GreaterOrEqual => current >= value,
                Comparison.Less => current < value,
                Comparison.LessOrEqual => current <= value,
                _ => false,
            };
        }

        /// <summary>조건이 없으면 참. 모든 조건이 참이어야 참 (AND).</summary>
        public static bool All(IReadOnlyList<FlagCondition> conditions, GameFlags flags)
        {
            if (conditions == null)
                return true;
            foreach (var condition in conditions)
                if (!condition.Evaluate(flags))
                    return false;
            return true;
        }
    }

    public enum FlagOperation { Set, Add }

    /// <summary>플래그 변경. 대화 노드나 선택지의 결과로 적용한다.</summary>
    [Serializable]
    public struct FlagEffect
    {
        public string key;
        public FlagOperation operation;
        public int value;

        public FlagEffect(string key, FlagOperation operation, int value)
        {
            this.key = key;
            this.operation = operation;
            this.value = value;
        }

        public void Apply(GameFlags flags)
        {
            if (operation == FlagOperation.Add)
                flags.AddInt(key, value);
            else
                flags.SetInt(key, value);
        }

        public static void ApplyAll(IReadOnlyList<FlagEffect> effects, GameFlags flags)
        {
            if (effects == null)
                return;
            foreach (var effect in effects)
                effect.Apply(flags);
        }
    }
}
