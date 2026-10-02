using UnityEngine;

namespace KindNeighbors.Flow
{
    /// <summary>시간대가 바뀌면 해당 LightingPreset을 적용한다.</summary>
    public class LightingController : MonoBehaviour
    {
        [SerializeField] PhaseEventChannel phaseChanged;
        [SerializeField] Light sun;

        void OnEnable() => phaseChanged.Raised += OnPhaseChanged;
        void OnDisable() => phaseChanged.Raised -= OnPhaseChanged;

        void OnPhaseChanged(PhaseDefinition definition)
        {
            if (definition.lighting != null)
                Apply(definition.lighting);
        }

        public void Apply(LightingPreset preset)
        {
            sun.color = preset.sunColor;
            sun.intensity = preset.sunIntensity;
            sun.transform.rotation = Quaternion.Euler(preset.sunRotation);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = preset.ambientSky;
            RenderSettings.ambientEquatorColor = preset.ambientEquator;
            RenderSettings.ambientGroundColor = preset.ambientGround;

            RenderSettings.fog = preset.fog;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = preset.fogColor;
            RenderSettings.fogDensity = preset.fogDensity;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = preset.useSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                cam.backgroundColor = preset.backgroundColor;
            }
        }
    }
}
