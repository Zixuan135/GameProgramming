using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using BubbleTown.Map;
using UnityEngine;

namespace BubbleTown.CameraSystem
{
    /// <summary>
    /// Battle camera that keeps a readable angled 3D view over the grid.
    /// It follows Player1 in solo-style modes and frames both players with a shared camera in LocalVS.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform primaryTarget;
        [SerializeField] private Transform secondaryTarget;
        [SerializeField] private bool autoFindTargets = true;
        [SerializeField] private bool shareCameraInLocalVS = true;
        [SerializeField] private bool frameAIInAIBattle = false;
        [SerializeField, Min(0.05f)] private float targetRefreshInterval = 0.25f;

        [Header("Safe Viewport")]
        [SerializeField] private bool useHudSafeViewport = true;
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
        private MapManager cachedMapManager;

        public static CameraController ActiveCamera => activeCamera;
        public Transform PrimaryTarget => primaryTarget;
        public Transform SecondaryTarget => secondaryTarget;
        public float CurrentFieldOfView => controlledCamera != null ? controlledCamera.fieldOfView : currentFieldOfView;
        public float CurrentZoomOut => currentZoomOut;
        public bool IsShaking => shakeTimer > 0f;

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

        private void OnEnable()
        {
            activeCamera = this;
        }

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

        private void LateUpdate()
        {
            if (autoFindTargets)
            {
                TickTargetRefresh();
            }

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

        private void OnDisable()
        {
            if (activeCamera == this)
            {
                activeCamera = null;
            }
        }

        public static void ShakeActiveCamera(float duration, float magnitude)
        {
            activeCamera?.Shake(duration, magnitude);
        }

        public void ShakeDefault()
        {
            Shake(defaultShakeDuration, defaultShakeMagnitude);
        }

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

        public void SetTarget(Transform newTarget)
        {
            primaryTarget = newTarget;
            secondaryTarget = null;
            SnapToTarget();
        }

        public void SetSharedTargets(Transform newPrimaryTarget, Transform newSecondaryTarget)
        {
            primaryTarget = newPrimaryTarget;
            secondaryTarget = newSecondaryTarget;
            SnapToTarget();
        }

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

        private float ResolveSmoothedZoomOut(bool useSharedView, float sharedDistance)
        {
            float targetZoomOut = 0f;
            if (useSharedView)
            {
                float zoomDistance = Mathf.Max(0f, sharedDistance - minSharedDistanceForZoom);
                targetZoomOut = Mathf.Min(zoomDistance * sharedZoomOutPerCell, maxSharedZoomOut);
            }

            currentZoomOut = Mathf.SmoothDamp(currentZoomOut, targetZoomOut, ref zoomOutVelocity, zoomSmoothTime);
            return currentZoomOut;
        }

        private Vector3 ResolveCameraPosition(Vector3 focusPoint, float zoomOut)
        {
            Vector3 offsetDirection = followOffset.sqrMagnitude > Mathf.Epsilon ? followOffset.normalized : Vector3.back;
            return focusPoint + followOffset + offsetDirection * zoomOut;
        }

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
                float mapCenterX = (mapMinX + mapMaxX) * 0.5f;
                minFocusX = mapCenterX;
                maxFocusX = mapCenterX;
            }

            if (minFocusZ > maxFocusZ)
            {
                float mapCenterZ = (mapMinZ + mapMaxZ) * 0.5f;
                minFocusZ = mapCenterZ;
                maxFocusZ = mapCenterZ;
            }

            focusPoint.x = Mathf.Clamp(focusPoint.x, minFocusX, maxFocusX);
            focusPoint.z = Mathf.Clamp(focusPoint.z, minFocusZ, maxFocusZ);
            return focusPoint;
        }

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

        private Vector3 ResolveFocusBias(bool useSharedView)
        {
            return useSharedView ? sharedFocusBias : soloFocusBias;
        }

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

        private void UpdateLens(bool useSharedView, float sharedDistance)
        {
            float targetFieldOfView = Mathf.Clamp(defaultFieldOfView, minFieldOfView, maxFieldOfView);
            if (useSharedView)
            {
                float fovDistance = Mathf.Max(0f, sharedDistance - minSharedDistanceForZoom);
                targetFieldOfView = Mathf.Clamp(
                    defaultFieldOfView + fovDistance * sharedFieldOfViewBoost,
                    minFieldOfView,
                    maxFieldOfView);
            }

            ApplyLens(targetFieldOfView, false);
        }

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

        private void ApplyCameraViewport()
        {
            if (controlledCamera == null)
            {
                return;
            }

            controlledCamera.rect = useHudSafeViewport ? ClampViewport(gameplayViewport) : new Rect(0f, 0f, 1f, 1f);
        }

        private Rect ClampViewport(Rect viewport)
        {
            float x = Mathf.Clamp01(viewport.x);
            float y = Mathf.Clamp01(viewport.y);
            float width = Mathf.Clamp(viewport.width, 0.05f, 1f - x);
            float height = Mathf.Clamp(viewport.height, 0.05f, 1f - y);
            return new Rect(x, y, width, height);
        }

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

        private bool IsValidTarget(Transform target)
        {
            return target != null && target.gameObject.activeInHierarchy;
        }

        private void SnapToTarget()
        {
            if (!IsValidTarget(primaryTarget))
            {
                return;
            }

            bool useSharedView = ShouldUseSharedView();
            float sharedDistance = useSharedView ? CalculateTargetDistanceXZ() : 0f;
            float zoomOut = ResolveSmoothedZoomOut(useSharedView, sharedDistance);
            Vector3 focusPoint = ResolveFocusPoint(useSharedView);
            smoothedFocusPoint = focusPoint;
            baseCameraPosition = ResolveCameraPosition(focusPoint, zoomOut);
            transform.position = baseCameraPosition;
            transform.LookAt(focusPoint + lookAtOffset);
            ApplyLens(useSharedView ? defaultFieldOfView + sharedDistance * sharedFieldOfViewBoost : defaultFieldOfView, true);

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
