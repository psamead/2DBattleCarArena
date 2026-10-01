using UnityEngine;

namespace BattleCarArena.Battle
{
    /// <summary>Visual-only contact motion, wheel rotation, and tire dust for a battle car.</summary>
    [DisallowMultipleComponent]
    public sealed class BattleCarPresentation : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float swayDistance = 0.15f;
        [SerializeField, Min(0f)] private float swayDegrees = 2.2f;
        [SerializeField, Min(0.1f)] private float swayFrequency = 7f;
        [SerializeField, Min(0.05f)] private float wheelRadius = 0.42f;
        [SerializeField, Min(0f)] private float contactSpinDegreesPerSecond = 1260f;
        private Rigidbody2D body;
        private Transform visualRoot;
        private Transform[] wheels;
        private ParticleSystem[] dust;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private bool inContact;
        private float phase;
        private float crashPulseRemaining;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visualRoot = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (visualRoot == null) return;
            restPosition = visualRoot.localPosition;
            restRotation = visualRoot.localRotation;
            wheels = new[] { FindDescendant(visualRoot, "FrontWheel"), FindDescendant(visualRoot, "RearWheel") };
            float dustDirection = visualRoot.lossyScale.x < 0f ? 1f : -1f;
            dust = new[]
            {
                CreateDust(FindDescendant(visualRoot, "FrontWheelDustAnchor"), dustDirection),
                CreateDust(FindDescendant(visualRoot, "RearWheelDustAnchor"), dustDirection)
            };
        }

        public void SetInContact(bool value)
        {
            if (inContact == value) return;
            inContact = value;
            if (value) phase = Random.Range(0f, 6.28318f);
        }

        public void TriggerCrash()
        {
            crashPulseRemaining = Mathf.Max(crashPulseRemaining, 0.9f);
            phase = Random.Range(0f, 6.28318f);
        }

        private void LateUpdate()
        {
            if (visualRoot == null) return;
            float speed = body != null ? body.linearVelocity.x : 0f;
            float rollingSpeed = speed;
            if (inContact && Mathf.Abs(rollingSpeed) < 0.6f)
                rollingSpeed = (transform.position.x < 0f ? 1f : -1f) * 0.8f;
            bool dustBurst = inContact || crashPulseRemaining > 0f;
            crashPulseRemaining = Mathf.Max(0f, crashPulseRemaining - Time.deltaTime);
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null)
                {
                    float mirrorSign = visualRoot.lossyScale.x < 0f ? -1f : 1f;
                    float spinSpeed = Mathf.Abs(rollingSpeed) / Mathf.Max(0.05f, wheelRadius) * Mathf.Rad2Deg;
                    if (inContact) spinSpeed = Mathf.Max(spinSpeed, contactSpinDegreesPerSecond);
                    float rotationDirection = Mathf.Abs(rollingSpeed) > 0.01f ? Mathf.Sign(rollingSpeed) : (transform.position.x < 0f ? 1f : -1f);
                    wheels[i].Rotate(0f, 0f, -rotationDirection * spinSpeed * Time.deltaTime * mirrorSign, Space.Self);
                }
                ParticleSystem particles = dust[i];
                if (particles == null) continue;
                var emission = particles.emission;
                emission.rateOverTime = dustBurst ? 46f : Mathf.Clamp(Mathf.Abs(speed) * 8f, 0f, 24f);
                if ((dustBurst || Mathf.Abs(speed) > 0.15f) && !particles.isPlaying) particles.Play();
                else if (!dustBurst && Mathf.Abs(speed) <= 0.15f && particles.isPlaying) particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            if (dustBurst)
            {
                phase += Time.deltaTime * (inContact ? swayFrequency : swayFrequency * 1.35f);
                visualRoot.localPosition = restPosition + Vector3.right * (Mathf.Sin(phase) * swayDistance);
                visualRoot.localRotation = restRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(phase * 0.82f) * swayDegrees);
            }
            else
            {
                visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, restPosition, Time.deltaTime * 10f);
                visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, restRotation, Time.deltaTime * 10f);
            }
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child != root && child.name == targetName) return child;
            return null;
        }

        private static ParticleSystem CreateDust(Transform anchor, float travelDirection)
        {
            if (anchor == null) return null;
            ParticleSystem system = anchor.GetComponent<ParticleSystem>();
            if (system == null) system = anchor.gameObject.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.22f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.42f, 1.05f);
            main.startColor = new Color(1f, 0.96f, 0.88f, 0.62f);
            main.maxParticles = 240;
            main.gravityModifier = -0.015f;
            var emission = system.emission;
            emission.rateOverTime = 0f;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.14f;
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(travelDirection * 1.2f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.5f);
            var color = system.colorOverLifetime;
            color.enabled = true;
            Gradient fade = new();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.9f, 0.76f, 0.57f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.58f, 0.12f), new GradientAlphaKey(0.36f, 0.68f), new GradientAlphaKey(0f, 1f) });
            color.color = fade;
            var noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.55f;
            noise.frequency = 0.32f;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 12;
            Material dustMaterial = Resources.Load<Material>("BattleDustPuff");
            if (dustMaterial != null) renderer.sharedMaterial = dustMaterial;
            return system;
        }
    }
}
