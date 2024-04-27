using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;

using DoubleHeat.Animation;

namespace DoubleHeat.SnowFightForDucksGame {

    public class SnowballAnimationManager : MonoBehaviour {

        public SpriteRenderer snowballSR;
        public SpriteRenderer shadowSR;

        [Header("Animation")]
        public SeqImgAnim.FixedDurationAnimProperties smashingAnimProps;

        static Sprite   _FlyingSprite;
        static Sprite[] _SmashingAnimSprites;
        static bool _IsSpritesLoaded = false;

        IEnumerator _currentPlayingAnim;


        void Awake () {
            LoadSpritesResources();
            PlayFlying();
        }

        static void LoadSpritesResources () {
            if (_IsSpritesLoaded)
                return;

            Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites/Objects/Snowball Anims");

            List<Sprite> tempSmashingAnimSpritesList = new List<Sprite>();

            foreach (Sprite sprite in allSprites) {
                if (sprite.name.Contains("Flying")) {
                    _FlyingSprite = sprite;
                }
                else if (sprite.name.Contains("Smashing")) {
                    tempSmashingAnimSpritesList.Add(sprite);
                }
            }

            _SmashingAnimSprites = tempSmashingAnimSpritesList.ToArray();

            _IsSpritesLoaded = true;
        }


        public void PlayFlying () {
            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            snowballSR.sprite = _FlyingSprite;
        }

        public void PlaySmashing (float startTimeCompensation = 0f) {
            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            float fps = _SmashingAnimSprites.Length / smashingAnimProps.duration;
            _currentPlayingAnim = SeqImgAnim.Anim(snowballSR, _SmashingAnimSprites, fps, smashingAnimProps.loop, smashingAnimProps.pingPong, new SeqImgAnim.AnimEndCallback(OnSmashingAnimEnd), startTimeCompensation);
            StartCoroutine(_currentPlayingAnim);

            shadowSR.DOKill(false);
            shadowSR.DOFade(0f, smashingAnimProps.duration);
        }

        void OnSmashingAnimEnd () {
            Destroy(gameObject);
        }

    }

}
