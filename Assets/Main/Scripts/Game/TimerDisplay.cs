using UnityEngine;
using UnityEngine.UI;

using DoubleHeat;

namespace DoubleHeat.SnowFightForDucksGame {

    public class TimerDisplay : MonoBehaviour {

        static Sprite[] _NumbersSprite;
        static bool _IsSpriteLoaded = false;


        public Image[] minDisplay = new Image[2];
        public Image[] secDisplay = new Image[2];



        public static void LoadSpritesResources () {
            if (_IsSpriteLoaded)
                return;

            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/UIs/HUD");

            _NumbersSprite = new Sprite[10];

            foreach (Sprite sprite in sprites) {
                for (int i = 0 ; i < _NumbersSprite.Length ; i++) {
                    if (sprite.name.Contains("TimerNumber" + i))
                        _NumbersSprite[i] = sprite;
                }
            }

            _IsSpriteLoaded = true;
        }

        public static void ReleaseLoaded () {
            _NumbersSprite = null;
            _IsSpriteLoaded = false;
        }


        void Awake () {
            LoadSpritesResources();
        }


        public void UpdateDisplay (float time) {
            TimerTimeDisplay timeDisplay = TimerTimeDisplay.FromSeconds(time);

            minDisplay[0].sprite = _NumbersSprite[timeDisplay.min % 10];
            minDisplay[1].sprite = _NumbersSprite[timeDisplay.min / 10];
            secDisplay[0].sprite = _NumbersSprite[timeDisplay.sec % 10];
            secDisplay[1].sprite = _NumbersSprite[timeDisplay.sec / 10];
        }



    }

}
