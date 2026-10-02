using System;

namespace KindNeighbors.Travel
{
    /// <summary>플레이어를 SpawnPoint로 옮겨 달라는 요청.</summary>
    [Serializable]
    public struct TravelRequest
    {
        public string spawnId;

        /// <summary>
        /// true면 곧바로 화면을 검게 만든 뒤 옮기고 밝아진다 (시간대 전환처럼 조명이 즉시 바뀌는 경우).
        /// false면 서서히 어두워졌다가 옮기고 밝아진다 (문 출입).
        /// </summary>
        public bool cutToBlack;

        public TravelRequest(string spawnId, bool cutToBlack)
        {
            this.spawnId = spawnId;
            this.cutToBlack = cutToBlack;
        }
    }
}
