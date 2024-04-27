using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DoubleHeat.Animation;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum StatueSkin {
        Magical,
        Unicorn
    }

    public class StatueAnimationManager : MonoBehaviour {

        class HPStateSprites {
            public Sprite[][] hpStateAnimSprites;

            public HPStateSprites () {
                hpStateAnimSprites = new Sprite[Global.STATUE_MAX_HP][];
            }
        }


        static Dictionary<StatueSkin, Sprite[]>       _SkinsIdleSprites = new Dictionary<StatueSkin, Sprite[]>();
        static Dictionary<StatueSkin, HPStateSprites> _SkinsGetHitHPStateSprites = new Dictionary<StatueSkin, HPStateSprites>();
        static bool _IsSpritesLoaded = false;



        public Statue         statue;
        public SpriteRenderer statueSR;

        [Header("Skin")]
        public StatueSkin currentSkin;

        [Header("Animation")]
        public SeqImgAnim.FixedFrameRateAnimProperties getHitAnimProps;


        int         _currentAnimHP = -1;
        IEnumerator _currentPlayingAnim;


        public static void LoadSpritesResources () {
            if (_IsSpritesLoaded)
                return;

            Dictionary<StatueSkin, string> skinResourceFileName = new Dictionary<StatueSkin, string>() {
                { StatueSkin.Magical, "Magical Statue Anims" },
                { StatueSkin.Unicorn, "Unicorn Statue Anims" }
            };


            foreach (StatueSkin skin in skinResourceFileName.Keys) {

                // init
                _SkinsGetHitHPStateSprites.Add(skin, new HPStateSprites());

                // load resources
                Sprite[] skinAllSprites = Resources.LoadAll<Sprite>("Sprites/Objects/" + skinResourceFileName[skin]);

                List<Sprite> idleSpritesList = new List<Sprite>();
                List<Sprite>[] tempHPStateGetHitSpritesLists = new List<Sprite>[Global.STATUE_MAX_HP];
                for (int i = 0 ; i < tempHPStateGetHitSpritesLists.Length ; i++) {
                    tempHPStateGetHitSpritesLists[i] = new List<Sprite>();
                }

                foreach (Sprite sprite in skinAllSprites) {

                    if (sprite.name.Contains("Idle")) {
                        idleSpritesList.Add(sprite);
                    }
                    else {
                        for (int i = 0 ; i < tempHPStateGetHitSpritesLists.Length ; i++) {
                            if (sprite.name.Contains("GetHit" + (i + 1))) {
                                tempHPStateGetHitSpritesLists[i].Add(sprite);
                            }
                        }
                    }
                }

                for (int i = 0 ; i < tempHPStateGetHitSpritesLists.Length ; i++) {
                    tempHPStateGetHitSpritesLists[i].Add(idleSpritesList[i]);
                }

                // get results
                _SkinsIdleSprites.Add(skin, idleSpritesList.ToArray());
                for (int i = 0 ; i < tempHPStateGetHitSpritesLists.Length ; i++) {
                    _SkinsGetHitHPStateSprites[skin].hpStateAnimSprites[i] = tempHPStateGetHitSpritesLists[i].ToArray();
                }
            }

            _IsSpritesLoaded = true;
        }

        public static void ReleaseLoaded () {
            _SkinsIdleSprites.Clear();
            _SkinsGetHitHPStateSprites.Clear();

            _IsSpritesLoaded = false;
        }


        void Awake () {
            LoadSpritesResources();
        }

        void OnGetHitAnimEnd () {
            statue.OnGetHitAnimEnd();
        }


        public void PlayIdle (int currentHP) {
            if (_currentPlayingAnim != null) {
                StopCoroutine(_currentPlayingAnim);
                _currentPlayingAnim = null;
            }

            statueSR.sprite = _SkinsIdleSprites[currentSkin][currentHP];
            _currentAnimHP = currentHP;
        }

        public void PlayGetHitAnim (int afterHP) {
            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            Sprite[] animSprites = _SkinsGetHitHPStateSprites[currentSkin].hpStateAnimSprites[afterHP];
            _currentPlayingAnim = SeqImgAnim.Anim(statueSR, animSprites, getHitAnimProps.fps, getHitAnimProps.loop, getHitAnimProps.pingPong, new SeqImgAnim.AnimEndCallback(OnGetHitAnimEnd));
            StartCoroutine(_currentPlayingAnim);

            _currentAnimHP = afterHP;
        }


        public void CheckForCurrentHP (int hp) {
            if (_currentAnimHP != hp) {
                PlayGetHitAnim(hp);
            }
        }

    }
}
