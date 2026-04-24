using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.CameraSystem
{
    /// <summary>
    /// Battle camera that keeps a readable angled 3D view over the grid.
    /// It follows Player1 by default and can frame Player2 with a shared camera in LocalVS.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform primaryTarget;
        [SerializeField] private Transform secondaryTarget;
        [SerializeField] private bool autoFindTargets = true;
        [SerializeField] private bool shareCameraInLocalVS = true;

        [Header("View")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 10.5f, -8f);
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.25f, 0.25f);
        [SerializeField] private float focusSmoothTime = 0.12f;
        [SerializeField] private float followSmoothTime = 0.18f;
        [SerializeField] private float rotationLerpSpeed = 8f;
        [SerializeField] private float sharedZoomOutPerCell = 0.22f;
        [SerializeField] private float maxSharedZoomOut = 4f;

        [Header("Lens")]
        [SerializeField] private Camera controlledCamera;
        [SerializeField] private float defaultFieldOfView = 48f;
        [SerializeField] private float sharedFieldOfViewBoost = 0.45f;
        [SerializeField] private float maxFieldOfView = 56f;

        private Vector3 smoothedFocusPoint;
        private Vector3 focusVelocity;
        private Vector3 followVelocity;
        private bool hasFocusPoint;

        public Transform PrimaryTarget => primaryTarget;
        public Transform SecondaryTarget => secondaryTarget;

        private void Awake()
        {
            if (controlledCamera == null)
            {
                controlledCamera = GetComponent<Camera>();
            }
        }

        private void Start()
        {
            if (autoFindTargets)
            {
                FindSceneTargets();
            }

            ApplyLens(defaultFieldOfView);
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (autoFindTargets && primaryTarget == null)
            {
                FindSceneTargets();
            }

            if (primaryTarget == null)
            {
                return;
            }

            bool useSharedView = ShouldUseSharedView();
            Vector3 targetFocusPoint = ResolveFocusPoint(useSharedView);
            Vector3 focusPoint = ResolveSmoothedFocusPoint(targetFocusPoint);
            Vector3 desiredPosition = ResolveCameraPosition(focusPoint, useSharedView);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref followVelocity,
                followSmoothTime);

            RotateToward(focusPoint + lookAtOffset);
            UpdateLens(useSharedView);
        }

        public void SetTarget(Transform newTarget)
        {
            primaryTarget = newTarget;
            SnapToTarget();
        }

        public void SetSharedTargets(Transform newPrimaryTarget, Transform newSecondaryTarget)
        {
            primaryTarget = newPrimaryTarget;
            secondaryTarget = newSecondaryTarget;
            SnapToTarget();
        }

        private void FindSceneTargets()
        {
            GameObject player1 = GameObject.Find("Player1");
            GameObject player2 = GameObject.Find("Player2");

            if (primaryTarget == null && player1 != null)
            {
                primaryTarget = player1.transform;
            }

            if (secondaryTarget == null && player2 != null)
            {
                secondaryTarget = player2.transform;
            }
        }

        private bool ShouldUseSharedView()
        {
            if (!shareCameraInLocalVS || secondaryTarget == null || !secondaryTarget.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (GameManager.Instance == null)
            {
                return true;
            }

            return GameManager.Instance.CurrentGameMode == GameMode.LocalVS;
        }

        private Vector3 ResolveFocusPoint(bool useSharedView)
        {
            if (!useSharedView)
            {
                return primaryTarget.position;
            }

            return (primaryTarget.position + secondaryTarget.position) * 0.5f;
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

        private Vector3 ResolveCameraPosition(Vector3 focusPoint, bool useSharedView)
        {
            Vector3 desiredOffset = followOffset;
            if (useSharedView)
            {
                float targetDistance = Vector3.Distance(primaryTarget.position, secondaryTarget.position);
                float zoomOut = Mathf.Min(targetDistance * sharedZoomOutPerCell, maxSharedZoomOut);
                desiredOffset += followOffset.normalized * zoomOut;
            }

            return focusPoint + desiredOffset;
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

        private void UpdateLens(bool useSharedView)
        {
            if (!useSharedView || secondaryTarget == null)
            {
                ApplyLens(defaultFieldOfView);
                return;
            }

            float targetDistance = Vector3.Distance(primaryTarget.position, secondaryTarget.position);
            float targetFieldOfView = Mathf.Min(
                defaultFieldOfView + targetDistance * sharedFieldOfViewBoost,
                maxFieldOfView);

            ApplyLens(targetFieldOfView);
        }

        private void ApplyLens(float fieldOfView)
        {
            if (controlledCamera == null || controlledCamera.orthographic)
            {
                return;
            }

            controlledCamera.fieldOfView = fieldOfView;
        }

        private void SnapToTarget()
        {
            if (primaryTarget == null)
            {
                return;
            }

            bool useSharedView = ShouldUseSharedView();
            Vector3 focusPoint = ResolveFocusPoint(useSharedView);
            smoothedFocusPoint = focusPoint;
            transform.position = ResolveCameraPosition(focusPoint, useSharedView);
            transform.LookAt(focusPoint + lookAtOffset);
            focusVelocity = Vector3.zero;
            followVelocity = Vector3.zero;
            hasFocusPoint = true;
        }
    }
}
