using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BubbleTown.Visuals
{
    /// <summary>
    /// Applies the shared Phase 2 toy-board lighting style at runtime.
    /// This keeps Battle readable while allowing Candy Park, Snowfield, and Jelly Maze to use different mood colors.
    /// </summary>
    public class BattleVisualStyleController : MonoBehaviour
    {
        private const string BattleBackgroundTexturePath = "UI/BattleHUD/BattleBottomBackground";

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

        [Header("Battle Background")]
        [SerializeField] private bool useIllustratedBackground = true;

        private Texture2D battleBackgroundTexture;
        private Camera backgroundCamera;
        private Canvas backgroundCanvas;
        private RawImage backgroundImage;

        private BattleMapType lastAppliedMapType = (BattleMapType)(-1);

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            ResolveReferences();
            EnsureBackgroundResources();
            ApplyCurrentStyle(true);
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            EnsureBackgroundResources();
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
            RefreshBackgroundPlacement();
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
            RefreshBackgroundAppearance(mapType);
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
        /// Purpose: Loads the illustrated battle background and creates a dedicated background camera/UI.
        /// Inputs: no direct parameters; reads Resources and current scene state.
        /// Output: no return value; prepares background rendering when enabled.
        /// </summary>
        private void EnsureBackgroundResources()
        {
            if (!useIllustratedBackground)
            {
                SetBackgroundActive(false);
                return;
            }

            if (battleBackgroundTexture == null)
            {
                battleBackgroundTexture = Resources.Load<Texture2D>(BattleBackgroundTexturePath);
                if (battleBackgroundTexture != null)
                {
                    battleBackgroundTexture.wrapMode = TextureWrapMode.Clamp;
                    battleBackgroundTexture.filterMode = FilterMode.Bilinear;
                    battleBackgroundTexture.anisoLevel = 0;
                }
            }

            if (battleBackgroundTexture == null || backgroundCanvas != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("BattleBackgroundCamera");
            cameraObject.transform.SetParent(transform, false);
            SetLayerRecursively(cameraObject, LayerMask.NameToLayer("UI"));
            backgroundCamera = cameraObject.AddComponent<Camera>();
            backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
            backgroundCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            backgroundCamera.orthographic = true;
            backgroundCamera.orthographicSize = 1f;
            backgroundCamera.nearClipPlane = -10f;
            backgroundCamera.farClipPlane = 10f;
            backgroundCamera.depth = targetCamera != null ? targetCamera.depth - 1f : -10f;

            GameObject canvasObject = new GameObject("BattleBackgroundCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(cameraObject.transform, false);
            SetLayerRecursively(canvasObject, LayerMask.NameToLayer("UI"));
            backgroundCanvas = canvasObject.AddComponent<Canvas>();
            backgroundCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            backgroundCanvas.worldCamera = backgroundCamera;
            backgroundCanvas.planeDistance = 1f;
            backgroundCanvas.sortingOrder = -1000;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject imageObject = new GameObject("BattleBackgroundImage", typeof(RectTransform));
            imageObject.transform.SetParent(canvasObject.transform, false);
            SetLayerRecursively(imageObject, LayerMask.NameToLayer("UI"));
            backgroundImage = imageObject.AddComponent<RawImage>();
            backgroundImage.texture = battleBackgroundTexture;
            backgroundImage.raycastTarget = false;
            backgroundImage.color = Color.white;
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

            bool shouldShowIllustratedBackground = useIllustratedBackground &&
                                                   battleBackgroundTexture != null &&
                                                   mapType == BattleMapType.Default;
            targetCamera.clearFlags = shouldShowIllustratedBackground
                ? CameraClearFlags.Depth
                : CameraClearFlags.SolidColor;
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

        /// <summary>
        /// Purpose: Shows the illustrated background for Candy Park style battles and hides it for other themes.
        /// Inputs: mapType selects the current battle theme.
        /// Output: no return value; updates quad visibility.
        /// </summary>
        /// <param name="mapType">Current battle map type.</param>
        private void RefreshBackgroundAppearance(BattleMapType mapType)
        {
            bool shouldShow = useIllustratedBackground &&
                              battleBackgroundTexture != null &&
                              mapType == BattleMapType.Default;

            if (backgroundCamera != null)
            {
                backgroundCamera.backgroundColor = candySkyColor;
            }

            SetBackgroundActive(shouldShow);
        }

        /// <summary>
        /// Purpose: Syncs the dedicated background camera and image to the full battle screen.
        /// Inputs: no direct parameters; reads the current screen size.
        /// Output: no return value; updates background camera properties and image placement.
        /// </summary>
        private void RefreshBackgroundPlacement()
        {
            if (targetCamera == null || backgroundCamera == null || backgroundCanvas == null || backgroundImage == null || !backgroundCanvas.gameObject.activeSelf)
            {
                return;
            }

            backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f);
            backgroundCamera.depth = targetCamera.depth - 1f;
            backgroundCamera.backgroundColor = targetCamera.backgroundColor;

            RectTransform canvasRect = backgroundCanvas.transform as RectTransform;
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }

            RectTransform imageRect = backgroundImage.rectTransform;
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Purpose: Shows or hides the runtime background quad safely.
        /// Inputs: active defines the desired state.
        /// Output: no return value; updates the quad when present.
        /// </summary>
        /// <param name="active">True to show the background quad.</param>
        private void SetBackgroundActive(bool active)
        {
            if (backgroundCamera != null)
            {
                backgroundCamera.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Purpose: Applies one layer value to a helper object and all of its children.
        /// Inputs: root is the hierarchy root and layer is the Unity layer index.
        /// Output: no return value; updates GameObject.layer recursively.
        /// </summary>
        /// <param name="root">Hierarchy root to update.</param>
        /// <param name="layer">Target Unity layer index.</param>
        private void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null || layer < 0)
            {
                return;
            }

            root.layer = layer;
            Transform rootTransform = root.transform;
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                SetLayerRecursively(rootTransform.GetChild(i).gameObject, layer);
            }
        }

        /// <summary>
        /// Purpose: Releases generated runtime objects.
        /// Inputs: no direct parameters.
        /// Output: no return value; prevents orphaned helper objects.
        /// </summary>
        private void OnDestroy()
        {
            if (backgroundCamera != null)
            {
                Destroy(backgroundCamera.gameObject);
            }
        }
    }
}
