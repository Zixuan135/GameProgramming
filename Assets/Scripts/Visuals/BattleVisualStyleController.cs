using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace BubbleTown.Visuals
{
    /// <summary>
    /// Applies the shared Phase 2 toy-board lighting style at runtime.
    /// This keeps Battle readable while allowing Candy Park, Snowfield, and Jelly Maze to use different mood colors.
    /// </summary>
    public class BattleVisualStyleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Light directionalLight;
        [SerializeField] private bool autoFindReferences = true;

        [Header("Common Light")]
        [SerializeField] private Vector3 directionalLightEuler = new Vector3(50f, -35f, 0f);
        [SerializeField] private Color directionalLightColor = new Color(1f, 0.96f, 0.86f);
        [SerializeField, Range(0f, 2f)] private float directionalLightIntensity = 1.18f;
        [SerializeField, Range(0f, 1f)] private float shadowStrength = 0.42f;

        [Header("Candy Park Mood")]
        [SerializeField] private Color candySkyColor = new Color(0.74f, 0.93f, 1f);
        [SerializeField] private Color candyAmbientSky = new Color(0.77f, 0.94f, 1f);
        [SerializeField] private Color candyAmbientEquator = new Color(1f, 0.92f, 0.76f);
        [SerializeField] private Color candyAmbientGround = new Color(0.62f, 0.72f, 0.62f);

        [Header("Jelly Maze Mood")]
        [SerializeField] private Color jellySkyColor = new Color(0.18f, 0.2f, 0.42f);
        [SerializeField] private Color jellyAmbientSky = new Color(0.35f, 0.38f, 0.72f);
        [SerializeField] private Color jellyAmbientEquator = new Color(0.28f, 0.62f, 0.78f);
        [SerializeField] private Color jellyAmbientGround = new Color(0.15f, 0.12f, 0.26f);

        [Header("Snowfield Mood")]
        [SerializeField] private Color snowSkyColor = new Color(0.7f, 0.94f, 1f);
        [SerializeField] private Color snowAmbientSky = new Color(0.86f, 0.98f, 1f);
        [SerializeField] private Color snowAmbientEquator = new Color(0.76f, 0.9f, 1f);
        [SerializeField] private Color snowAmbientGround = new Color(0.72f, 0.82f, 0.9f);

        private BattleMapType lastAppliedMapType = (BattleMapType)(-1);

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            ResolveReferences();
            ApplyCurrentStyle(true);
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            ApplyCurrentStyle(true);
        }

        /// <summary>
        /// Purpose: Runs camera or visual follow-up logic after regular Update calls.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void LateUpdate()
        {
            ApplyCurrentStyle(false);
        }

        /// <summary>
        /// Purpose: Applies current style to the current object or scene.
        /// Inputs: `force`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="force">Input value used by this method.</param>
        public void ApplyCurrentStyle(bool force)
        {
            BattleMapType mapType = GameManager.Instance != null
                ? GameManager.Instance.CurrentMapType
                : BattleMapType.Default;

            if (!force && mapType == lastAppliedMapType)
            {
                return;
            }

            ResolveReferences();
            ApplyLighting(mapType);
            ApplyCameraBackground(mapType);
            lastAppliedMapType = mapType;
        }

        /// <summary>
        /// Purpose: Resolves references from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResolveReferences()
        {
            if (!autoFindReferences)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            }

            if (directionalLight == null)
            {
                directionalLight = RenderSettings.sun != null ? RenderSettings.sun : FindDirectionalLight();
            }
        }

        /// <summary>
        /// Purpose: Finds directional light from scene objects or cached data.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Light` value.
        /// </summary>
        /// <returns>a `Light` value.</returns>
        private Light FindDirectionalLight()
        {
            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    return lights[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Purpose: Applies lighting to the current object or scene.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        private void ApplyLighting(BattleMapType mapType)
        {
            Color skyAmbient = candyAmbientSky;
            Color equatorAmbient = candyAmbientEquator;
            Color groundAmbient = candyAmbientGround;

            if (mapType == BattleMapType.OpenField)
            {
                skyAmbient = snowAmbientSky;
                equatorAmbient = snowAmbientEquator;
                groundAmbient = snowAmbientGround;
            }
            else if (mapType == BattleMapType.Maze)
            {
                skyAmbient = jellyAmbientSky;
                equatorAmbient = jellyAmbientEquator;
                groundAmbient = jellyAmbientGround;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = skyAmbient;
            RenderSettings.ambientEquatorColor = equatorAmbient;
            RenderSettings.ambientGroundColor = groundAmbient;
            RenderSettings.ambientIntensity = 1f;

            if (directionalLight == null)
            {
                return;
            }

            directionalLight.type = LightType.Directional;
            directionalLight.transform.rotation = Quaternion.Euler(directionalLightEuler);
            directionalLight.color = directionalLightColor;
            directionalLight.intensity = directionalLightIntensity;
            directionalLight.shadows = LightShadows.Soft;
            directionalLight.shadowStrength = shadowStrength;
            RenderSettings.sun = directionalLight;
        }

        /// <summary>
        /// Purpose: Applies camera background to the current object or scene.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        private void ApplyCameraBackground(BattleMapType mapType)
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            if (mapType == BattleMapType.OpenField)
            {
                targetCamera.backgroundColor = snowSkyColor;
            }
            else if (mapType == BattleMapType.Maze)
            {
                targetCamera.backgroundColor = jellySkyColor;
            }
            else
            {
                targetCamera.backgroundColor = candySkyColor;
            }
        }
    }
}
