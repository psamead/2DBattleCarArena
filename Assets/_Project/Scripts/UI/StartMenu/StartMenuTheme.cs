using TMPro;
using UnityEngine;

namespace BattleCarArena.UI
{
    [CreateAssetMenu(fileName = "StartMenuTheme", menuName = "Battle Car Arena/UI/Start Menu Theme")]
    public sealed class StartMenuTheme : ScriptableObject
    {
        [Header("Replaceable Assets")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite logoSprite;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private AudioClip menuMusic;

        [Header("Palette")]
        [SerializeField] private Color backgroundColor = new(0.025f, 0.035f, 0.055f, 1f);
        [SerializeField] private Color panelColor = new(0.055f, 0.075f, 0.11f, 0.94f);
        [SerializeField] private Color accentColor = new(1f, 0.34f, 0.08f, 1f);
        [SerializeField] private Color primaryTextColor = new(0.96f, 0.97f, 1f, 1f);
        [SerializeField] private Color mutedTextColor = new(0.58f, 0.65f, 0.74f, 1f);
        [SerializeField] private Color buttonColor = new(0.12f, 0.16f, 0.23f, 1f);
        [SerializeField] private Color buttonHoverColor = new(0.29f, 0.49f, 0.82f, 1f);

        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite LogoSprite => logoSprite;
        public Sprite ButtonSprite => buttonSprite;
        public TMP_FontAsset FontAsset => fontAsset;
        public AudioClip MenuMusic => menuMusic;
        public Color BackgroundColor => backgroundColor;
        public Color PanelColor => panelColor;
        public Color AccentColor => accentColor;
        public Color PrimaryTextColor => primaryTextColor;
        public Color MutedTextColor => mutedTextColor;
        public Color ButtonColor => buttonColor;
        public Color ButtonHoverColor => buttonHoverColor;
    }
}
