using TMPro;
using UnityEngine;

namespace BattleCarArena.UI
{
    [CreateAssetMenu(fileName = "GarageHubTheme", menuName = "Battle Car Arena/UI/Garage Hub Theme")]
    public sealed class GarageHubTheme : ScriptableObject
    {
        [Header("Replaceable Assets")]
        [SerializeField] private Sprite neutralBackground;
        [SerializeField] private Sprite alternateBackground;
        [SerializeField] private Sprite titleLogo;
        [SerializeField] private Sprite carPreview;
        [SerializeField] private Sprite alternateCarPreview;
        [SerializeField, ColorUsage(false)] private Color carPreviewTint = new(1f, 0.93f, 0.84f, 1f);
        [SerializeField, ColorUsage(false)] private Color alternateCarPreviewTint = new(0.96f, 0.44f, 0.15f, 0.6f);
        [SerializeField] private Sprite engineUpgradeIcon;
        [SerializeField] private Sprite weaponUpgradeIcon;
        [SerializeField] private Sprite armorUpgradeIcon;
        [Tooltip("Legacy shared icon used when a dedicated upgrade icon is not assigned.")]
        [SerializeField] private Sprite upgradeIcon;
        [SerializeField] private Sprite panel;
        [SerializeField] private Sprite button;
        [SerializeField] private Sprite slotFrame;
        [SerializeField] private Sprite actionButtonFrame;
        [SerializeField] private TMP_FontAsset titleFont;
        [SerializeField] private TMP_FontAsset actionButtonFont;
        [SerializeField] private AudioClip backgroundMusic;

        [Header("Background Motion")]
        [SerializeField, Min(1f)] private float breathingCycleSeconds = 18f;
        [SerializeField, Min(0f)] private float flickerIntervalMinimum = 8f;
        [SerializeField, Min(0f)] private float flickerIntervalMaximum = 18f;
        [SerializeField, Min(0.02f)] private float flickerDuration = 0.45f;
        [SerializeField, Min(0.1f)] private float flickerFrequencyHz = 4f;
        [SerializeField, Range(0f, 0.25f)] private float flickerStrength = 0.08f;
        [SerializeField, ColorUsage(false)] private Color carFlickerTint = new(1f, 0.75f, 0.55f, 1f);
        [SerializeField, Range(0f, 1f)] private float carFlickerResponse = 0.42f;
        [SerializeField, Tooltip("Zero chooses a different random sequence each play session; a nonzero seed makes timing repeatable.")]
        private int randomSeed;

        [Header("Music")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;

        public Sprite NeutralBackground => neutralBackground;
        public Sprite AlternateBackground => alternateBackground;
        public Sprite TitleLogo => titleLogo;
        public Sprite CarPreview => carPreview;
        public Sprite AlternateCarPreview => alternateCarPreview;
        public Color CarPreviewTint => carPreviewTint;
        public Color AlternateCarPreviewTint => alternateCarPreviewTint;
        public Sprite EngineUpgradeIcon => engineUpgradeIcon != null ? engineUpgradeIcon : upgradeIcon;
        public Sprite WeaponUpgradeIcon => weaponUpgradeIcon != null ? weaponUpgradeIcon : upgradeIcon;
        public Sprite ArmorUpgradeIcon => armorUpgradeIcon != null ? armorUpgradeIcon : upgradeIcon;
        public Sprite Panel => panel;
        public Sprite Button => button;
        public Sprite SlotFrame => slotFrame;
        public Sprite ActionButtonFrame => actionButtonFrame;
        public TMP_FontAsset TitleFont => titleFont;
        public TMP_FontAsset ActionButtonFont => actionButtonFont;
        public AudioClip BackgroundMusic => backgroundMusic;
        public float BreathingCycleSeconds => breathingCycleSeconds;
        public float FlickerIntervalMinimum => flickerIntervalMinimum;
        public float FlickerIntervalMaximum => flickerIntervalMaximum;
        public float FlickerDuration => flickerDuration;
        public float FlickerFrequencyHz => flickerFrequencyHz;
        public float FlickerStrength => flickerStrength;
        public Color CarFlickerTint => carFlickerTint;
        public float CarFlickerResponse => carFlickerResponse;
        public int RandomSeed => randomSeed;
        public float MusicVolume => musicVolume;
    }
}
