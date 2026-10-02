using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleCarArena.UI
{
    /// <summary>Plays the Start Menu option hover cue when the pointer enters a button.</summary>
    public sealed class StartMenuHoverFeedback : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private StartMenuView view;

        public void Configure(StartMenuView menuView)
        {
            view = menuView;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            view?.PlayOptionHoverSound();
        }
    }
}
