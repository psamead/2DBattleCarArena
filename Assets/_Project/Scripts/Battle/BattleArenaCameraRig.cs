using UnityEngine;

namespace BattleCarArena.Battle
{
    [RequireComponent(typeof(Camera))]
    public sealed class BattleArenaCameraRig : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform challengerTarget;

        [Header("Framing")]
        [SerializeField, Min(0.1f)] private float wideViewSize = 5.4f;
        [SerializeField, Min(0.1f)] private float closeCombatViewSize = 3.4f;
        [SerializeField, Min(0.1f)] private float fightViewSize = 4f;
        [SerializeField, Min(0f)] private float wideViewDistance = 9.5f;
        [SerializeField, Min(0f)] private float closeViewDistance = 2.5f;
        [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.48f;
        [SerializeField, Min(0.01f)] private float followSmoothTime = 0.3f;
        [SerializeField] private Vector2 trackingWeight = new(0.8f, 0.35f);
        [SerializeField] private Vector2 maxTrackingOffset = new(1.4f, 0.6f);
        [SerializeField, Min(0f)] private float pushDirectionLead = 1.1f;
        [SerializeField, Min(0.01f)] private float pushDirectionSmoothTime = 0.22f;

        [Header("Impact Motion")]
        [SerializeField, Min(0f)] private float impactShakeAmplitude = 0.23f;
        [SerializeField, Min(0.01f)] private float impactShakeDuration = 0.34f;
        [SerializeField, Min(0f)] private float impactShakeFrequency = 34f;

        private Vector3 followPosition;
        private Vector3 followVelocity;
        private float sizeVelocity;
        private float shakeRemaining;
        private float shakeStrength;
        private float cameraDepth;
        private Vector3 previousMidpoint;
        private float pushDirection;
        private float pushDirectionVelocity;
        private bool battleStarted;
        private bool battleFinished;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            followPosition = transform.position;
            cameraDepth = transform.position.z;
            if (playerTarget != null && challengerTarget != null)
            {
                previousMidpoint = (playerTarget.position + challengerTarget.position) * 0.5f;
            }

            if (targetCamera != null)
            {
                targetCamera.orthographic = true;
                targetCamera.orthographicSize = wideViewSize;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || playerTarget == null || challengerTarget == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            Vector3 midpoint = (playerTarget.position + challengerTarget.position) * 0.5f;
            float midpointSpeed = (midpoint.x - previousMidpoint.x) / deltaTime;
            previousMidpoint = midpoint;
            float desiredPushDirection = battleFinished ? 0f : Mathf.Clamp(midpointSpeed / 1.25f, -1f, 1f);
            pushDirection = Mathf.SmoothDamp(pushDirection, desiredPushDirection, ref pushDirectionVelocity,
                pushDirectionSmoothTime, Mathf.Infinity, deltaTime);

            float separation = Vector2.Distance(playerTarget.position, challengerTarget.position);
            float distanceBlend = battleFinished ? 1f : Mathf.InverseLerp(closeViewDistance, wideViewDistance, separation);
            float desiredSize = Mathf.Lerp(closeCombatViewSize, wideViewSize, distanceBlend);
            if (battleStarted && !battleFinished)
            {
                desiredSize = Mathf.Min(desiredSize, fightViewSize);
            }
            targetCamera.orthographicSize = Mathf.SmoothDamp(
                targetCamera.orthographicSize, desiredSize, ref sizeVelocity, zoomSmoothTime, Mathf.Infinity, deltaTime);

            float desiredX = battleFinished ? 0f : Mathf.Clamp(midpoint.x * trackingWeight.x, -maxTrackingOffset.x, maxTrackingOffset.x)
                + pushDirection * pushDirectionLead;
            float desiredY = battleFinished ? 0f : Mathf.Clamp(midpoint.y * trackingWeight.y, -maxTrackingOffset.y, maxTrackingOffset.y);
            Vector3 desiredPosition = new(
                desiredX,
                desiredY,
                cameraDepth);
            followPosition = Vector3.SmoothDamp(followPosition, desiredPosition, ref followVelocity, followSmoothTime, Mathf.Infinity, deltaTime);

            Vector3 shakeOffset = CalculateShakeOffset();
            transform.position = followPosition + shakeOffset;

            shakeRemaining = Mathf.Max(0f, shakeRemaining - deltaTime);
            if (shakeRemaining <= 0f)
            {
                shakeStrength = 0f;
            }
        }

        public void TriggerImpact(float intensity = 1f)
        {
            shakeRemaining = Mathf.Max(shakeRemaining, impactShakeDuration);
            shakeStrength = Mathf.Max(shakeStrength, Mathf.Max(0f, intensity));
        }

        public void SetBattleFinished()
        {
            battleFinished = true;
        }

        public void SetBattleStarted()
        {
            battleStarted = true;
        }

        private Vector3 CalculateShakeOffset()
        {
            if (shakeRemaining <= 0f || shakeStrength <= 0f)
            {
                return Vector3.zero;
            }

            float fade = shakeRemaining / impactShakeDuration;
            float phase = Time.time * impactShakeFrequency;
            float x = Mathf.Sin(phase * 1.7f) * 0.6f + Mathf.Sin(phase * 2.9f) * 0.4f;
            float y = Mathf.Cos(phase * 1.3f) * 0.65f + Mathf.Sin(phase * 2.3f) * 0.35f;
            return new Vector3(x, y, 0f) * (impactShakeAmplitude * shakeStrength * fade);
        }
    }
}
