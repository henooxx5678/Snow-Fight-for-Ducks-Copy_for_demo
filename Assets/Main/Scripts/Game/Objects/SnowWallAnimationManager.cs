using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;

using DoubleHeat.Animation;

namespace DoubleHeat.SnowFightForDucksGame {

    public class SnowWallAnimationManager : MonoBehaviour {

        public enum DirectionType {
            RightUp,
            Right,
            RightDown
        }

        class HPStateSprites {
            public Sprite[][] hpStateAnimSprites;

            public HPStateSprites () {
                hpStateAnimSprites = new Sprite[Global.SNOW_WALL_MAX_HP][];
            }
        }


        static Dictionary<DirectionType, Sprite[]>       _DirTypesIdleSprites          = new Dictionary<DirectionType, Sprite[]>();
        static Dictionary<DirectionType, HPStateSprites> _DirTypesGetHitHPStateSprites = new Dictionary<DirectionType, HPStateSprites>();
        static bool _IsSpriteLoaded = false;


        public DirectionType dirType;

        public SnowWall       snowWall;
        public SpriteRenderer snowWallSR;
        public SpriteRenderer shadowSR;

        [Header("Animation")]
        public SeqImgAnim.FixedFrameRateAnimProperties getHitAnimProps;


        int         _currentAnimHP = 1;
        IEnumerator _currentPlayingAnim;



        public static void LoadSpritesResources () {
            if (_IsSpriteLoaded)
                return;

            Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites/Objects/Snow Wall Anims");

            foreach (DirectionType dir in System.Enum.GetValues(typeof(DirectionType))) {

                Sprite[]       tempIdleSprites            = new Sprite[Global.SNOW_WALL_MAX_HP + 1];
                List<Sprite>[] tempGetHitAnimSpritesLists = new List<Sprite>[Global.SNOW_WALL_MAX_HP];

                // init
                tempIdleSprites[0] = null;
                for (int i = 0 ; i < tempGetHitAnimSpritesLists.Length ; i++) {
                    tempGetHitAnimSpritesLists[i] = new List<Sprite>();
                }


                foreach (Sprite sprite in allSprites) {

                    if (sprite.name.Contains(dir.ToString())) {

                        for (int i = 0 ; i < tempIdleSprites.Length ; i++) {
                            if (sprite.name.Contains(dir.ToString() + "Idle" + i)) {
                                tempIdleSprites[i] = sprite;
                            }
                        }

                        for (int i = 0 ; i < tempGetHitAnimSpritesLists.Length ; i++) {
                            if (sprite.name.Contains(dir.ToString() + (i + 1) + "To")) {
                                tempGetHitAnimSpritesLists[i].Add(sprite);
                            }
                        }
                    }
                }

                for (int i = 1 ; i < tempGetHitAnimSpritesLists.Length ; i++) {
                    tempGetHitAnimSpritesLists[i].Add(tempIdleSprites[i]);
                }

                _DirTypesIdleSprites.Add(dir, tempIdleSprites);

                HPStateSprites hpStateSprites = new HPStateSprites();
                for (int i = 0 ; i < Global.SNOW_WALL_MAX_HP ; i++) {
                    hpStateSprites.hpStateAnimSprites[i] = tempGetHitAnimSpritesLists[i].ToArray();
                }

                _DirTypesGetHitHPStateSprites.Add(dir, hpStateSprites);

                _IsSpriteLoaded = true;
            }
        }

        public static void ReleaseLoaded () {
            _DirTypesIdleSprites.Clear();
            _DirTypesGetHitHPStateSprites.Clear();

            _IsSpriteLoaded = false;
        }


        void Awake () {
            LoadSpritesResources();
        }

        void OnGetHitAnimEnd () {
            snowWall.OnGetHitAnimEnd();
        }

        public void PlayIdle (int currentHP) {
            if (_currentPlayingAnim != null) {
                StopCoroutine(_currentPlayingAnim);
                _currentPlayingAnim = null;
            }

            snowWallSR.sprite = _DirTypesIdleSprites[dirType][currentHP];
            _currentAnimHP = currentHP;
        }

        public void PlayGetHitAnim (int afterHP) {
            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            Sprite[] animSprtes = _DirTypesGetHitHPStateSprites[dirType].hpStateAnimSprites[afterHP];
            _currentPlayingAnim = SeqImgAnim.Anim(snowWallSR, animSprtes, getHitAnimProps.fps, getHitAnimProps.loop, getHitAnimProps.pingPong, new SeqImgAnim.AnimEndCallback(OnGetHitAnimEnd));
            StartCoroutine(_currentPlayingAnim);

            shadowSR.DOKill(false);
            if (afterHP == 0)
                shadowSR.DOFade(0f, animSprtes.Length / getHitAnimProps.fps);

            _currentAnimHP = afterHP;
        }


        public void CheckForCurrentHP (int hp) {
            if (_currentAnimHP != hp) {
                PlayGetHitAnim(hp);
            }
        }

    }
}
