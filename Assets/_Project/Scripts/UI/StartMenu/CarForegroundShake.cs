using UnityEngine;

namespace BattleCarArena.UI
{
    /// <summary>
    /// Adds a small, continuous engine vibration to the start-menu car artwork.
    /// </summary>
    public sealed class CarForegroundShake : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField, Min(0f)] private float positionAmplitude = 2.5f;
        [SerializeField, Min(0f)] private float rotationAmplitude = 0.35f;
        [SerializeField, Min(0f)] private float frequency = 18f;

        private Vector2 baseAnchoredPosition;
        private float baseRotationZ;

        private void Awake()
        {
            CacheBaseTransform();
        }

        private void OnEnable()
        {
            CacheBaseTransform();
        }

        private void Update()
        {
            if (target == null || frequency <= 0f)
            {
                return;
            }

            float phase = Time.unscaledTime * frequency;
            float x = Mathf.Sin(phase * 1.13f) * positionAmplitude;
            float y = Mathf.Cos(phase * 0.97f) * positionAmplitude * 0.65f;
            float rotation = Mathf.Sin(phase * 1.07f) * rotationAmplitude;

            target.anchoredPosition = baseAnchoredPosition + new Vector2(x, y);
            target.localRotation = Quaternion.Euler(0f, 0f, baseRotationZ + rotation);
        }

        private void CacheBaseTransform()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            if (target == null)
            {
                return;
            }

            baseAnchoredPosition = target.anchoredPosition;
            baseRotationZ = target.localEulerAngles.z;
        }
    }
}
