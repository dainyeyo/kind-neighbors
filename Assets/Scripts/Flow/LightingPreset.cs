using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>시간대별 조명 설정. 맵은 하나, 낮과 밤은 조명·사운드·소품 세트로만 구분한다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Flow/Lighting Preset", fileName = "Light_")]
    public class LightingPreset : ScriptableObject
    {
        [Header("Sun / Moon")]
        public Color sunColor = Color.white;
        public float sunIntensity = 1f;
        public Vector3 sunRotation = new(45f, -35f, 0f);

        [Header("Ambient (Trilight)")]
        public Color ambientSky = Color.gray;
        public Color ambientEquator = Color.gray;
        public Color ambientGround = Color.black;

        [Header("Fog")]
        public bool fog;
        public Color fogColor = Color.gray;
        public float fogDensity = 0.02f;

        [Header("Background")]
        [Tooltip("끄면 스카이박스 대신 단색 배경을 쓴다 (밤)")]
        public bool useSkybox = true;
        public Color backgroundColor = Color.black;
    }
}
