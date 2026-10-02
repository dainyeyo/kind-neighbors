using System;
using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>게임 전체 시간대 순서. Day1.Morning → Day1.Evening → Day1.Night → Day2.Morning …</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Flow/Game Flow Config", fileName = "GameFlow")]
    public class GameFlowConfig : ScriptableObject
    {
        public PhaseDefinition[] phases;

        public int IndexOf(GamePhase phase) =>
            Array.FindIndex(phases, p => p != null && p.phase.Matches(phase.day, phase.time));
    }
}
