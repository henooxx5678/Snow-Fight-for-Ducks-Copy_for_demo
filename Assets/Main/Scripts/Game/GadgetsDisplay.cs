using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class GadgetsDisplay : MonoBehaviour {

        [System.Serializable]
        public class SwitchableIcon {
            public Sprite enabledSprite;
            public Sprite disabledSprite;
        }

        public SwitchableIcon snowballIconSprites;
        public SwitchableIcon hammerIconSprites;
        public SwitchableIcon wrenchIconSprites;
        public SwitchableIcon moverIconSprites;

        public SpriteRenderer snowballSR;
        public SpriteRenderer hammerSR;
        public SpriteRenderer wrenchSR;
        public SpriteRenderer moverSR;

        public float enabledShiftUpDistance;
        public float shiftingAnimSpeed;
        public float iconsIntervalDistance;

        Dictionary<PlayerGadget, SwitchableIcon> gadgetsIconSprites;
        Dictionary<PlayerGadget, SpriteRenderer> iconsSR;


        void Awake () {
            gadgetsIconSprites = new Dictionary<PlayerGadget, SwitchableIcon>() {
                { PlayerGadget.Snowball, snowballIconSprites },
                { PlayerGadget.Hammer,   hammerIconSprites },
                { PlayerGadget.Wrench,   wrenchIconSprites },
                { PlayerGadget.Mover,    moverIconSprites }
            };

            iconsSR = new Dictionary<PlayerGadget, SpriteRenderer>() {
                { PlayerGadget.Snowball, snowballSR },
                { PlayerGadget.Hammer,   hammerSR },
                { PlayerGadget.Wrench,   wrenchSR },
                { PlayerGadget.Mover,    moverSR }
            };
        }


        public void UpdateDisplay (PlayerGadget activeGadget, List<PlayerGadget> displayedGadgets) {

            int index = 0;

            foreach (PlayerGadget gadget in iconsSR.Keys) {

                if (displayedGadgets.Contains(gadget)) {

                    iconsSR[gadget].enabled = true;

                    RectTransform rectTransform = iconsSR[gadget].gameObject.GetComponent<RectTransform>();
                    rectTransform.DOKill(false);

                    Vector2 pos = rectTransform.anchoredPosition;
                    pos.x = (index - (displayedGadgets.Count - 1) / 2f) * iconsIntervalDistance;
                    rectTransform.anchoredPosition = pos;

                    if (gadget == activeGadget) {
                        iconsSR[gadget].sprite = gadgetsIconSprites[gadget].enabledSprite;
                        iconsSR[gadget].color = Color.white;

                        rectTransform.DOAnchorPosY(enabledShiftUpDistance, shiftingAnimSpeed, false).SetSpeedBased();
                    }
                    else {
                        iconsSR[gadget].sprite = gadgetsIconSprites[gadget].disabledSprite;
                        iconsSR[gadget].color = new Color(0.8f, 0.8f, 0.8f, 1f);

                        rectTransform.DOAnchorPosY(0f, shiftingAnimSpeed, false).SetSpeedBased();
                    }

                    index++;
                }
                else {
                    iconsSR[gadget].enabled = false;
                }
            }
        }

    }
}
