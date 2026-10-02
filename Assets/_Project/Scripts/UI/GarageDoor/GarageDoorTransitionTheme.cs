using UnityEngine;

namespace BattleCarArena.UI
{
    [CreateAssetMenu(fileName = "GarageDoorTransitionTheme", menuName = "Battle Car Arena/UI/Garage Door Transition Theme")]
    public sealed class GarageDoorTransitionTheme : ScriptableObject
    {
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Sprite garageBackgroundSprite;
        [SerializeField] private Sprite garageCarSprite;

        public Sprite FrameSprite => frameSprite;
        public Sprite PanelSprite => panelSprite;
        public Sprite GarageBackgroundSprite => garageBackgroundSprite;
        public Sprite GarageCarSprite => garageCarSprite;

        public void Configure(Sprite frame, Sprite panel, Sprite garageBackground, Sprite garageCar)
        {
            frameSprite = frame;
            panelSprite = panel;
            garageBackgroundSprite = garageBackground;
            garageCarSprite = garageCar;
        }
    }
}
