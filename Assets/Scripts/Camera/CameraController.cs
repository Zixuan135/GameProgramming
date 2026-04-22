using UnityEngine;

namespace BubbleTown.CameraSystem
{
    /// <summary>
    /// Basic follow camera for battle scene.
    /// Keeps a fixed offset and smooths movement.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
        [SerializeField] private float followLerpSpeed = 8f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followLerpSpeed * Time.deltaTime);
            transform.LookAt(target.position);
        }
    }
}
