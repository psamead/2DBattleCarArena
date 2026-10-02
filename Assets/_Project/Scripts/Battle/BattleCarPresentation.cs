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
        private ParticleSystem[] defeatSmoke;
        private Sprite contactShadowSprite;
        private Texture2D contactShadowTexture;
        private SpriteRenderer[] contactShadows;
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
            contactShadows = CreateWheelShadows(visualRoot, wheels);
            defeatSmoke = CreateDefeatSmoke(visualRoot);
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

        public void StopBattleDust()
        {
            inContact = false;
            crashPulseRemaining = 0f;
            if (dust != null)
            {
                foreach (ParticleSystem particles in dust)
                {
                    if (particles == null) continue;
                    var emission = particles.emission;
                    emission.rateOverTime = 0f;
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        public void PlayDefeatEffects()
        {
            StopBattleDust();
            if (defeatSmoke == null) return;
            foreach (ParticleSystem plume in defeatSmoke)
                if (plume != null && !plume.isPlaying)
                    plume.Play();
        }

        private void OnDestroy()
        {
            if (contactShadowSprite != null) Destroy(contactShadowSprite);
            if (contactShadowTexture != null) Destroy(contactShadowTexture);
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
                emission.rateOverTime = dustBurst ? 40f : Mathf.Clamp(Mathf.Abs(speed) * 8f, 0f, 24f);
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
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.95f);
            main.startColor = new Color(1f, 0.96f, 0.88f, 0.42f);
            main.maxParticles = 200;
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
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.44f, 0.12f), new GradientAlphaKey(0.24f, 0.68f), new GradientAlphaKey(0f, 1f) });
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

        private static ParticleSystem[] CreateDefeatSmoke(Transform root)
        {
            return new[]
            {
                CreateSmokePlume(root, "EngineSmoke", new Vector3(0.9f, 0.42f, -0.02f), 16f, 0.54f, 1.0f),
                CreateSmokePlume(root, "HoodSmoke", new Vector3(0.25f, 0.22f, -0.02f), 9f, 0.42f, 0.82f),
                CreateSmokePlume(root, "CabinSmoke", new Vector3(-0.35f, 0.5f, -0.02f), 7f, 0.4f, 0.78f),
                CreateSmokePlume(root, "RearDamageSmoke", new Vector3(-1.12f, 0.02f, -0.02f), 5f, 0.4f, 0.78f)
            };
        }

        private static ParticleSystem CreateSmokePlume(Transform root, string name, Vector3 localPosition,
            float emissionRate, float minimumSize, float maximumSize)
        {
            GameObject smokeObject = new(name);
            smokeObject.transform.SetParent(root, false);
            smokeObject.transform.localPosition = localPosition;
            ParticleSystem system = smokeObject.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.1f, 4.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(minimumSize, maximumSize);
            main.startColor = new Color(0.1f, 0.095f, 0.09f, 0.92f);
            main.maxParticles = 112;
            var emission = system.emission;
            emission.rateOverTime = emissionRate;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            float driftDirection = root.lossyScale.x < 0f ? -1f : 1f;
            velocity.x = new ParticleSystem.MinMaxCurve(0.18f * driftDirection);
            velocity.y = new ParticleSystem.MinMaxCurve(0.92f);
            var color = system.colorOverLifetime;
            color.enabled = true;
            Gradient fade = new();
            fade.SetKeys(
                new[] { new GradientColorKey(new Color(0.08f, 0.075f, 0.07f), 0f), new GradientColorKey(new Color(0.2f, 0.19f, 0.18f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.82f, 0.12f), new GradientAlphaKey(0.58f, 0.72f), new GradientAlphaKey(0f, 1f) });
            color.color = fade;
            var noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.48f;
            noise.frequency = 0.25f;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 13;
            Material smokeMaterial = Resources.Load<Material>("BattleDustPuff");
            if (smokeMaterial != null) renderer.sharedMaterial = smokeMaterial;
            return system;
        }

        private SpriteRenderer[] CreateWheelShadows(Transform root, Transform[] wheelTransforms)
        {
            const int textureWidth = 64;
            const int textureHeight = 32;
            contactShadowTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false, true)
            {
                name = "RuntimeCarContactShadowTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            Color32[] pixels = new Color32[textureWidth * textureHeight];
            for (int y = 0; y < textureHeight; y++)
            {
                float ny = (y + 0.5f) / textureHeight * 2f - 1f;
                for (int x = 0; x < textureWidth; x++)
                {
                    float nx = (x + 0.5f) / textureWidth * 2f - 1f;
                    float falloff = Mathf.Pow(Mathf.Clamp01(1f - nx * nx - ny * ny), 2f);
                    byte alpha = (byte)Mathf.RoundToInt(falloff * 105f);
                    pixels[y * textureWidth + x] = new Color32(9, 7, 6, alpha);
                }
            }
            contactShadowTexture.SetPixels32(pixels);
            contactShadowTexture.Apply(false, true);
            contactShadowSprite = Sprite.Create(contactShadowTexture, new Rect(0, 0, textureWidth, textureHeight),
                new Vector2(0.5f, 0.5f), textureWidth);
            contactShadowSprite.name = "RuntimeCarContactShadow";
            contactShadowSprite.hideFlags = HideFlags.DontSave;

            SpriteRenderer[] renderers = new SpriteRenderer[wheelTransforms.Length];
            for (int i = 0; i < wheelTransforms.Length; i++)
            {
                Transform wheel = wheelTransforms[i];
                if (wheel == null) continue;
                string shadowName = i == 0 ? "FrontWheelShadow" : "RearWheelShadow";
                GameObject shadowObject = new(shadowName);
                shadowObject.transform.SetParent(root, false);
                Transform anchor = FindDescendant(root, i == 0 ? "FrontWheelDustAnchor" : "RearWheelDustAnchor");
                float shadowY = anchor != null ? anchor.localPosition.y + 0.06f : wheel.localPosition.y - wheelRadius;
                shadowObject.transform.localPosition = new Vector3(wheel.localPosition.x, shadowY, 0.02f);
                shadowObject.transform.localScale = new Vector3(1.45f, 0.72f, 1f);
                SpriteRenderer renderer = shadowObject.AddComponent<SpriteRenderer>();
                renderer.sprite = contactShadowSprite;
                renderer.sortingOrder = 9;
                renderers[i] = renderer;
            }
            return renderers;
        }
    }
}
