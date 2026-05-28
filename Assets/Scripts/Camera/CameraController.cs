using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using BubbleTown.Map;
using UnityEngine;

namespace BubbleTown.CameraSystem
{
    /// <summary>
    /// Battle camera that keeps a readable angled 3D view over the grid.
    /// It preserves a HUD-safe arena overview and can frame both active players in versus modes.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform primaryTarget;
        [SerializeField] private Transform secondaryTarget;
        [SerializeField] private bool autoFindTargets = true;
        [SerializeField] private bool shareCameraInLocalVS = true;
        [SerializeField] private bool frameAIInAIBattle = true;
        [SerializeField, Min(0.05f)] private float targetRefreshInterval = 0.25f;

        [Header("Safe Viewport")]
        [SerializeField] private bool useHudSafeViewport = true;
        [SerializeField] private bool autoFitViewportToHud = true;
        [SerializeField, Range(0f, 0.6f)] private float leftHudSafeWidthNormalized = 0.32f;
        [SerializeField] private Rect gameplayViewport = new Rect(0.32f, 0.08f, 0.68f, 0.92f);

        [Header("View")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 11.25f, -8.35f);
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, -0.25f, 0.35f);
        [SerializeField, Min(0.01f)] private float focusSmoothTime = 0.1f;
        [SerializeField, Min(0.01f)] private float followSmoothTime = 0.18f;
        [SerializeField, Min(0f)] private float rotationLerpSpeed = 9f;
        [SerializeField, Min(0f)] private float minSharedDistanceForZoom = 2.5f;
        [SerializeField, Min(0f)] private float sharedZoomOutPerCell = 0.32f;
        [SerializeField, Min(0f)] private float maxSharedZoomOut = 5.5f;
        [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.18f;

        [Header("Framing")]
        [Tooltip("Keeps the arena at one visual size in every mode instead of zooming with player distance.")]
        [SerializeField] private bool keepConsistentMapScaleAcrossModes = true;
        [Tooltip("Extra overview distance applied when consistent map scale is enabled.")]
        [SerializeField, Min(0f)] private float overviewZoomOut = 0.55f;
        [SerializeField] private bool clampFocusInsideMap = true;
        [SerializeField] private Vector2 soloFocusPaddingCells = new Vector2(1.65f, 1.55f);
        [SerializeField] private Vector2 sharedFocusPaddingCells = new Vector2(1.3f, 1.15f);
        [SerializeField] private Vector3 soloFocusBias = new Vector3(0f, 0f, 0.25f);
        [SerializeField] private Vector3 sharedFocusBias = new Vector3(0f, 0f, 0.25f);
        [SerializeField, Range(0f, 1f)] private float soloTargetInfluence = 0f;
        [SerializeField, Range(0f, 1f)] private float sharedTargetInfluence = 0f;

        [Header("Lens")]
        [SerializeField] private Camera controlledCamera;
        [SerializeField, Min(1f)] private float defaultFieldOfView = 52f;
        [SerializeField, Min(1f)] private float minFieldOfView = 50.5f;
        [SerializeField, Min(0f)] private float sharedFieldOfViewBoost = 0.65f;
        [SerializeField, Min(1f)] private float maxFieldOfView = 62f;
        [SerializeField, Min(0.01f)] private float lensSmoothTime = 0.18f;

        [Header("Shake")]
        [SerializeField] private bool enableShake = true;
        [SerializeField, Min(0f)] private float defaultShakeDuration = 0.16f;
        [SerializeField, Min(0f)] private float defaultShakeMagnitude = 0.16f;
        [SerializeField, Min(0.1f)] private float shakeFrequency = 24f;
        [SerializeField, Min(0.01f)] private float shakeDamping = 7f;

        private static CameraController activeCamera;

        private Vector3 smoothedFocusPoint;
        private Vector3 focusVelocity;
        private Vector3 followVelocity;
        private Vector3 baseCameraPosition;
        private Vector3 lastShakeOffset;
        private bool hasFocusPoint;
        private bool hasBaseCameraPosition;
        private float currentZoomOut;
        private float zoomOutVelocity;
        private float currentFieldOfView;
        private float fieldOfViewVelocity;
        private float targetRefreshTimer;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeMagnitude;
        private float shakeSeedX;
        private float shakeSeedY;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private MapManager cachedMapManager;

        public static CameraController ActiveCamera => activeCamera;
        public Transform PrimaryTarget => primaryTarget;
        public Transform SecondaryTarget => secondaryTarget;
        public float CurrentFieldOfView => controlledCamera != null ? controlledCamera.fieldOfView : currentFieldOfView;
        public float CurrentZoomOut => currentZoomOut;
        public bool IsShaking => shakeTimer > 0f;

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            if (controlledCamera == null)
            {
                controlledCamera = GetComponent<Camera>();
            }

            if (controlledCamera != null)
            {
                ApplyCameraViewport();
                currentFieldOfView = controlledCamera.fieldOfView;
            }
            else
            {
                currentFieldOfView = defaultFieldOfView;
            }
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            activeCamera = this;
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            if (autoFindTargets)
            {
                FindSceneTargets();
            }

            RefreshMapManagerReference();
            ApplyCameraViewport();
            ApplyLens(defaultFieldOfView, true);
            SnapToTarget();
        }

        /// <summary>
        /// Purpose: Runs camera or visual follow-up logic after regular Update calls.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void LateUpdate()
        {
            if (autoFindTargets)
            {
                TickTargetRefresh();
            }

            RefreshCameraViewportIfNeeded();
            RefreshMapManagerReference();
            if (!IsValidTarget(primaryTarget))
            {
                return;
            }

            bool useSharedView = ShouldUseSharedView();
            float sharedDistance = useSharedView ? CalculateTargetDistanceXZ() : 0f;
            Vector3 targetFocusPoint = ResolveFocusPoint(useSharedView);
            Vector3 focusPoint = ResolveSmoothedFocusPoint(targetFocusPoint);
            float zoomOut = ResolveSmoothedZoomOut(useSharedView, sharedDistance);
            Vector3 desiredPosition = ResolveCameraPosition(focusPoint, zoomOut);

            if (!hasBaseCameraPosition)
            {
                baseCameraPosition = transform.position - lastShakeOffset;
                hasBaseCameraPosition = true;
            }

            baseCameraPosition = Vector3.SmoothDamp(
                baseCameraPosition,
                desiredPosition,
                ref followVelocity,
                followSmoothTime);

            Vector3 shakeOffset = ResolveShakeOffset();
            transform.position = baseCameraPosition + shakeOffset;
            lastShakeOffset = shakeOffset;

            RotateToward(focusPoint + lookAtOffset);
            UpdateLens(useSharedView, sharedDistance);
        }

        /// <summary>
        /// Purpose: Cleans up subscriptions or runtime state when this component becomes inactive.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnDisable()
        {
            if (activeCamera == this)
            {
                activeCamera = null;
            }
        }

        /// <summary>
        /// Purpose: Performs shake active camera for this component.
        /// Inputs: `duration`, `magnitude`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="duration">Input value used by this method.</param>
        /// <param name="magnitude">Input value used by this method.</param>
        public static void ShakeActiveCamera(float duration, float magnitude)
        {
            activeCamera?.Shake(duration, magnitude);
        }

        /// <summary>
        /// Purpose: Performs shake default for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void ShakeDefault()
        {
            Shake(defaultShakeDuration, defaultShakeMagnitude);
        }

        /// <summary>
        /// Purpose: Performs shake for this component.
        /// Inputs: `duration`, `magnitude`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="duration">Input value used by this method.</param>
        /// <param name="magnitude">Input value used by this method.</param>
        public void Shake(float duration, float magnitude)
        {
            if (!enableShake || !GameSettings.ScreenShakeEnabled || duration <= 0f || magnitude <= 0f)
            {
                return;
            }

            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeTimer = Mathf.Max(shakeTimer, duration);
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
            shakeSeedX = Random.Range(0f, 1000f);
            shakeSeedY = Random.Range(0f, 1000f);
        }

        /// <summary>
        /// Purpose: Sets target.
        /// Inputs: `newTarget`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newTarget">Input value used by this method.</param>
        public void SetTarget(Transform newTarget)
        {
            primaryTarget = newTarget;
            secondaryTarget = null;
            SnapToTarget();
        }

        /// <summary>
        /// Purpose: Sets shared targets.
        /// Inputs: `newPrimaryTarget`, `newSecondaryTarget`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newPrimaryTarget">Input value used by this method.</param>
        /// <param name="newSecondaryTarget">Input value used by this method.</param>
        public void SetSharedTargets(Transform newPrimaryTarget, Transform newSecondaryTarget)
        {
            primaryTarget = newPrimaryTarget;
            secondaryTarget = newSecondaryTarget;
            SnapToTarget();
        }

        /// <summary>
        /// Purpose: Advances target refresh by one update step.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void TickTargetRefresh()
        {
            targetRefreshTimer -= Time.deltaTime;
            if (targetRefreshTimer > 0f && IsValidTarget(primaryTarget))
            {
                return;
            }

            targetRefreshTimer = targetRefreshInterval;
            FindSceneTargets();
        }

        /// <summary>
        /// Purpose: Finds scene targets from scene objects or cached data.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void FindSceneTargets()
        {
            RefreshMapManagerReference();
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                if (gameManager.Player1 != null)
                {
                    primaryTarget = gameManager.Player1.transform;
                }

                secondaryTarget = ResolveSecondaryTargetFromGameManager(gameManager);
                if (IsValidTarget(primaryTarget))
                {
                    return;
                }
            }

            GameObject player1 = GameObject.Find("Player1");
            GameObject player2 = GameObject.Find("Player2");

            if (player1 != null)
            {
                primaryTarget = player1.transform;
            }

            if (player2 != null)
            {
                secondaryTarget = player2.transform;
            }
        }

        /// <summary>
        /// Purpose: Resolves secondary target from game manager from the current runtime state.
        /// Inputs: `gameManager`; may also read serialized fields and current runtime state.
        /// Output: a `Transform` value.
        /// </summary>
        /// <param name="gameManager">Input value used by this method.</param>
        /// <returns>a `Transform` value.</returns>
        private Transform ResolveSecondaryTargetFromGameManager(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return secondaryTarget;
            }

            if (gameManager.CurrentGameMode == GameMode.LocalVS && gameManager.Player2 != null)
            {
                return gameManager.Player2.transform;
            }

            if (frameAIInAIBattle && gameManager.CurrentGameMode == GameMode.AIBattle && gameManager.AIPlayer != null)
            {
                return gameManager.AIPlayer.transform;
            }

            return null;
        }

        /// <summary>
        /// Purpose: Returns whether this object should use shared view.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool ShouldUseSharedView()
        {
            if (!shareCameraInLocalVS && !frameAIInAIBattle)
            {
                return false;
            }

            if (!IsValidTarget(primaryTarget) || !IsValidTarget(secondaryTarget))
            {
                return false;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return shareCameraInLocalVS;
            }

            return (shareCameraInLocalVS && gameManager.CurrentGameMode == GameMode.LocalVS) ||
                   (frameAIInAIBattle && gameManager.CurrentGameMode == GameMode.AIBattle);
        }

        /// <summary>
        /// Purpose: Calculates the world point the camera should look at before applying follow smoothing.
        /// Inputs: useSharedView chooses either Player1-only focus or the midpoint between two active targets.
        /// Output: returns the biased and map-clamped focus point used by camera position and rotation.
        /// </summary>
        /// <param name="useSharedView">True when the camera should frame two targets together.</param>
        /// <returns>World-space focus point on or near the battle map.</returns>
        private Vector3 ResolveFocusPoint(bool useSharedView)
        {
            Vector3 focusPoint;
            if (!useSharedView)
            {
                focusPoint = primaryTarget.position;
            }
            else
            {
                focusPoint = (primaryTarget.position + secondaryTarget.position) * 0.5f;
            }

            focusPoint = BlendFocusTowardMapCenter(focusPoint, useSharedView);
            focusPoint += ResolveFocusBias(useSharedView);
            return ClampFocusPointToMap(focusPoint, useSharedView);
        }

        /// <summary>
        /// Purpose: Resolves smoothed focus point from the current runtime state.
        /// Inputs: `targetFocusPoint`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="targetFocusPoint">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
        private Vector3 ResolveSmoothedFocusPoint(Vector3 targetFocusPoint)
        {
            if (!hasFocusPoint)
            {
                smoothedFocusPoint = targetFocusPoint;
                hasFocusPoint = true;
            }

            smoothedFocusPoint = Vector3.SmoothDamp(
                smoothedFocusPoint,
                targetFocusPoint,
                ref focusVelocity,
                focusSmoothTime);

            return smoothedFocusPoint;
        }

        /// <summary>
        /// Purpose: Smoothly approaches the requested camera distance for the active battle framing style.
        /// Inputs: `useSharedView` indicates whether two characters are framed; `sharedDistance` is their XZ distance.
        /// Output: returns the smoothed extra distance applied behind the normal camera offset.
        /// </summary>
        /// <param name="useSharedView">True when the camera is framing two active battle characters.</param>
        /// <param name="sharedDistance">Distance between framed characters in world-space XZ units.</param>
        /// <returns>Smoothed zoom-out distance in world units.</returns>
        private float ResolveSmoothedZoomOut(bool useSharedView, float sharedDistance)
        {
            float targetZoomOut = ResolveTargetZoomOut(useSharedView, sharedDistance);
            currentZoomOut = Mathf.SmoothDamp(currentZoomOut, targetZoomOut, ref zoomOutVelocity, zoomSmoothTime);
            return currentZoomOut;
        }

        /// <summary>
        /// Purpose: Chooses the desired camera distance before smoothing is applied.
        /// Inputs: `useSharedView` identifies a two-character view; `sharedDistance` measures target separation.
        /// Output: returns the requested extra camera distance in world units.
        /// </summary>
        /// <param name="useSharedView">True for AI battle or local versus shared-camera framing.</param>
        /// <param name="sharedDistance">Distance between the two framed targets on the XZ plane.</param>
        /// <returns>Target zoom-out distance in world units.</returns>
        private float ResolveTargetZoomOut(bool useSharedView, float sharedDistance)
        {
            if (keepConsistentMapScaleAcrossModes)
            {
                // One gentle overview offset makes solo slightly smaller and stops versus modes from
                // shrinking the arena when opponents move apart.
                return overviewZoomOut;
            }

            if (!useSharedView)
            {
                return 0f;
            }

            float zoomDistance = Mathf.Max(0f, sharedDistance - minSharedDistanceForZoom);
            return Mathf.Min(zoomDistance * sharedZoomOutPerCell, maxSharedZoomOut);
        }

        /// <summary>
        /// Purpose: Resolves camera position from the current runtime state.
        /// Inputs: `focusPoint`, `zoomOut`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="focusPoint">Input value used by this method.</param>
        /// <param name="zoomOut">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
        private Vector3 ResolveCameraPosition(Vector3 focusPoint, float zoomOut)
        {
            Vector3 offsetDirection = followOffset.sqrMagnitude > Mathf.Epsilon ? followOffset.normalized : Vector3.back;
            return focusPoint + followOffset + offsetDirection * zoomOut;
        }

        /// <summary>
        /// Purpose: Refreshes map manager reference from the latest runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void RefreshMapManagerReference()
        {
            if (cachedMapManager != null)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.ActiveMapManager != null)
            {
                cachedMapManager = gameManager.ActiveMapManager;
                return;
            }

            cachedMapManager = FindObjectOfType<MapManager>();
        }

        /// <summary>
        /// Purpose: Keeps the camera focus inside the useful map area so the arena stays visible near edges.
        /// Inputs: focusPoint is the requested focus and useSharedView selects solo or shared padding.
        /// Output: returns the clamped focus point.
        /// </summary>
        /// <param name="focusPoint">Requested world-space focus position.</param>
        /// <param name="useSharedView">True when framing two targets, false for solo-style framing.</param>
        /// <returns>Focus point clamped to the safe map rectangle.</returns>
        private Vector3 ClampFocusPointToMap(Vector3 focusPoint, bool useSharedView)
        {
            if (!clampFocusInsideMap)
            {
                return focusPoint;
            }

            RefreshMapManagerReference();
            if (cachedMapManager == null)
            {
                return focusPoint;
            }

            float cellSize = Mathf.Max(0.1f, cachedMapManager.CellSize);
            float mapMinX = 0f;
            float mapMinZ = 0f;
            float mapMaxX = Mathf.Max(0f, (cachedMapManager.MapWidth - 1) * cellSize);
            float mapMaxZ = Mathf.Max(0f, (cachedMapManager.MapHeight - 1) * cellSize);

            Vector2 paddingCells = useSharedView ? sharedFocusPaddingCells : soloFocusPaddingCells;
            float paddingX = Mathf.Max(0f, paddingCells.x) * cellSize;
            float paddingZ = Mathf.Max(0f, paddingCells.y) * cellSize;

            float minFocusX = mapMinX + paddingX;
            float maxFocusX = mapMaxX - paddingX;
            float minFocusZ = mapMinZ + paddingZ;
            float maxFocusZ = mapMaxZ - paddingZ;

            if (minFocusX > maxFocusX)
            {
                // Tiny maps can be smaller than the padding, so collapse the valid range to center.
                float mapCenterX = (mapMinX + mapMaxX) * 0.5f;
                minFocusX = mapCenterX;
                maxFocusX = mapCenterX;
            }

            if (minFocusZ > maxFocusZ)
            {
                // Same fallback for the Z axis when padding would invert the clamp range.
                float mapCenterZ = (mapMinZ + mapMaxZ) * 0.5f;
                minFocusZ = mapCenterZ;
                maxFocusZ = mapCenterZ;
            }

            focusPoint.x = Mathf.Clamp(focusPoint.x, minFocusX, maxFocusX);
            focusPoint.z = Mathf.Clamp(focusPoint.z, minFocusZ, maxFocusZ);
            return focusPoint;
        }

        /// <summary>
        /// Purpose: Returns blend focus toward map center for the current state.
        /// Inputs: `focusPoint`, `useSharedView`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="focusPoint">Input value used by this method.</param>
        /// <param name="useSharedView">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
        private Vector3 BlendFocusTowardMapCenter(Vector3 focusPoint, bool useSharedView)
        {
            RefreshMapManagerReference();
            if (cachedMapManager == null)
            {
                return focusPoint;
            }

            float cellSize = Mathf.Max(0.1f, cachedMapManager.CellSize);
            Vector3 mapCenter = new Vector3(
                (cachedMapManager.MapWidth - 1) * cellSize * 0.5f,
                focusPoint.y,
                (cachedMapManager.MapHeight - 1) * cellSize * 0.5f);

            float targetInfluence = Mathf.Clamp01(useSharedView ? sharedTargetInfluence : soloTargetInfluence);
            return Vector3.Lerp(mapCenter, focusPoint, targetInfluence);
        }

        /// <summary>
        /// Purpose: Resolves focus bias from the current runtime state.
        /// Inputs: `useSharedView`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="useSharedView">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
        private Vector3 ResolveFocusBias(bool useSharedView)
        {
            return useSharedView ? sharedFocusBias : soloFocusBias;
        }

        /// <summary>
        /// Purpose: Performs rotate toward for this component.
        /// Inputs: `lookPoint`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="lookPoint">Input value used by this method.</param>
        private void RotateToward(Vector3 lookPoint)
        {
            Vector3 lookDirection = lookPoint - transform.position;
            if (lookDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationLerpSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Purpose: Applies the current battle mode's desired perspective field of view.
        /// Inputs: `useSharedView` indicates two-character framing; `sharedDistance` is their XZ distance.
        /// Output: no return value; updates the controlled camera lens.
        /// </summary>
        /// <param name="useSharedView">True when a versus-mode shared view is active.</param>
        /// <param name="sharedDistance">Distance between shared-view characters in world units.</param>
        private void UpdateLens(bool useSharedView, float sharedDistance)
        {
            ApplyLens(ResolveTargetFieldOfView(useSharedView, sharedDistance), false);
        }

        /// <summary>
        /// Purpose: Resolves lens zoom while allowing all modes to use one readable arena scale by default.
        /// Inputs: `useSharedView` indicates two-character framing; `sharedDistance` is their XZ distance.
        /// Output: returns the requested perspective field of view in degrees.
        /// </summary>
        /// <param name="useSharedView">True when the view contains two active battle characters.</param>
        /// <param name="sharedDistance">Distance between the characters on the XZ plane.</param>
        /// <returns>Target perspective field of view in degrees.</returns>
        private float ResolveTargetFieldOfView(bool useSharedView, float sharedDistance)
        {
            float targetFieldOfView = Mathf.Clamp(defaultFieldOfView, minFieldOfView, maxFieldOfView);
            if (!keepConsistentMapScaleAcrossModes && useSharedView)
            {
                float fovDistance = Mathf.Max(0f, sharedDistance - minSharedDistanceForZoom);
                targetFieldOfView = Mathf.Clamp(
                    defaultFieldOfView + fovDistance * sharedFieldOfViewBoost,
                    minFieldOfView,
                    maxFieldOfView);
            }

            return targetFieldOfView;
        }

        /// <summary>
        /// Purpose: Applies lens to the current object or scene.
        /// Inputs: `fieldOfView`, `snap`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="fieldOfView">Input value used by this method.</param>
        /// <param name="snap">Input value used by this method.</param>
        private void ApplyLens(float fieldOfView, bool snap)
        {
            if (controlledCamera == null || controlledCamera.orthographic)
            {
                return;
            }

            float clampedFieldOfView = Mathf.Clamp(fieldOfView, minFieldOfView, maxFieldOfView);
            if (snap)
            {
                currentFieldOfView = clampedFieldOfView;
            }
            else
            {
                currentFieldOfView = Mathf.SmoothDamp(
                    currentFieldOfView,
                    clampedFieldOfView,
                    ref fieldOfViewVelocity,
                    lensSmoothTime);
            }

            controlledCamera.fieldOfView = currentFieldOfView;
        }

        /// <summary>
        /// Purpose: Applies camera viewport to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyCameraViewport()
        {
            if (controlledCamera == null)
            {
                return;
            }

            controlledCamera.rect = useHudSafeViewport ? ResolveGameplayViewport() : new Rect(0f, 0f, 1f, 1f);
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        /// <summary>
        /// Purpose: Refreshes camera viewport if needed from the latest runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void RefreshCameraViewportIfNeeded()
        {
            if (controlledCamera == null || (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height))
            {
                return;
            }

            ApplyCameraViewport();
        }

        /// <summary>
        /// Purpose: Resolves gameplay viewport from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Rect` value.
        /// </summary>
        /// <returns>a `Rect` value.</returns>
        private Rect ResolveGameplayViewport()
        {
            Rect configuredViewport = ClampViewport(gameplayViewport);
            if (!autoFitViewportToHud)
            {
                return configuredViewport;
            }

            // Keep the gameplay camera inside the authored battle layout slot instead of stretching to every
            // pixel right of the HUD. This keeps the map, HUD, and background feeling like one composed screen.
            float left = Mathf.Max(Mathf.Clamp01(leftHudSafeWidthNormalized), configuredViewport.x);
            float right = Mathf.Min(1f, configuredViewport.xMax);
            float width = Mathf.Max(0.05f, right - left);
            return ClampViewport(new Rect(left, configuredViewport.y, width, configuredViewport.height));
        }

        /// <summary>
        /// Purpose: Clamps viewport into a valid range.
        /// Inputs: `viewport`; may also read serialized fields and current runtime state.
        /// Output: a `Rect` value.
        /// </summary>
        /// <param name="viewport">Input value used by this method.</param>
        /// <returns>a `Rect` value.</returns>
        private Rect ClampViewport(Rect viewport)
        {
            float x = Mathf.Clamp01(viewport.x);
            float y = Mathf.Clamp01(viewport.y);
            float width = Mathf.Clamp(viewport.width, 0.05f, 1f - x);
            float height = Mathf.Clamp(viewport.height, 0.05f, 1f - y);
            return new Rect(x, y, width, height);
        }

        /// <summary>
        /// Purpose: Resolves shake offset from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <returns>a `Vector3` value.</returns>
        private Vector3 ResolveShakeOffset()
        {
            if (!enableShake || shakeTimer <= 0f)
            {
                shakeTimer = 0f;
                shakeMagnitude = 0f;
                return Vector3.zero;
            }

            shakeTimer = Mathf.Max(0f, shakeTimer - Time.deltaTime);
            float normalizedTime = shakeDuration <= 0f ? 0f : shakeTimer / shakeDuration;
            float dampedMagnitude = shakeMagnitude * normalizedTime * Mathf.Exp(-shakeDamping * (1f - normalizedTime));
            float time = Time.time * shakeFrequency;
            float x = Mathf.PerlinNoise(shakeSeedX, time) * 2f - 1f;
            float y = Mathf.PerlinNoise(shakeSeedY, time) * 2f - 1f;

            if (shakeTimer <= 0f)
            {
                shakeMagnitude = 0f;
            }

            return transform.right * (x * dampedMagnitude) + transform.up * (y * dampedMagnitude);
        }

        /// <summary>
        /// Purpose: Calculates target distance xz.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `float` value.
        /// </summary>
        /// <returns>a `float` value.</returns>
        private float CalculateTargetDistanceXZ()
        {
            if (!IsValidTarget(primaryTarget) || !IsValidTarget(secondaryTarget))
            {
                return 0f;
            }

            Vector3 primaryPosition = primaryTarget.position;
            Vector3 secondaryPosition = secondaryTarget.position;
            primaryPosition.y = 0f;
            secondaryPosition.y = 0f;
            return Vector3.Distance(primaryPosition, secondaryPosition);
        }

        /// <summary>
        /// Purpose: Returns whether this object is valid target.
        /// Inputs: `target`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="target">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsValidTarget(Transform target)
        {
            return target != null && target.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Purpose: Performs snap to target for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void SnapToTarget()
        {
            if (!IsValidTarget(primaryTarget))
            {
                return;
            }

            bool useSharedView = ShouldUseSharedView();
            float sharedDistance = useSharedView ? CalculateTargetDistanceXZ() : 0f;
            float zoomOut = ResolveTargetZoomOut(useSharedView, sharedDistance);
            currentZoomOut = zoomOut;
            Vector3 focusPoint = ResolveFocusPoint(useSharedView);
            smoothedFocusPoint = focusPoint;
            baseCameraPosition = ResolveCameraPosition(focusPoint, zoomOut);
            transform.position = baseCameraPosition;
            transform.LookAt(focusPoint + lookAtOffset);
            ApplyLens(ResolveTargetFieldOfView(useSharedView, sharedDistance), true);

            focusVelocity = Vector3.zero;
            followVelocity = Vector3.zero;
            zoomOutVelocity = 0f;
            fieldOfViewVelocity = 0f;
            lastShakeOffset = Vector3.zero;
            hasFocusPoint = true;
            hasBaseCameraPosition = true;
        }
    }
}
