using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DoubleHeat.Animation;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum DuckSkin {
        Yellow,
        Black,
        White,
        Roast
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerManager))]
    public class PlayerAnimationManager : MonoBehaviour {

        class AnimStateSprites {
            public Dictionary<PlayerState, Sprite[]> statesAnimSprites;

            public AnimStateSprites () {
                statesAnimSprites = new Dictionary<PlayerState, Sprite[]>();
            }
        }

        const int TURNING_ANIM_FLIP_INDEX = 2;

        public readonly static Dictionary<DuckSkin, string> _DuckSkinResourceFileName = new Dictionary<DuckSkin, string>() {
            { DuckSkin.Yellow, "Yellow Duck Anims" },
            { DuckSkin.Black,  "Black Duck Anims" },
            { DuckSkin.White,  "White Duck Anims" },
            { DuckSkin.Roast,  "Roast Duck Anims" }
        };


        public SpriteRenderer duckSR;
        public SpriteRenderer carriedSR;
        public SpriteRenderer duckSnowCakeSR;
        public SpriteRenderer quackWordSR;

        [Header("Skin")]
        public DuckSkin currentDuckSkin;

        [Header("Animations")]
        public SeqImgAnim.FixedDurationAnimProperties idle;
        public SeqImgAnim.FixedDurationAnimProperties walking;
        public SeqImgAnim.FixedDurationAnimProperties turning;
        public SeqImgAnim.FixedDurationAnimProperties collecting;
        public SeqImgAnim.FixedDurationAnimProperties firing;
        public SeqImgAnim.FixedDurationAnimProperties building;
        public SeqImgAnim.FixedDurationAnimProperties repairing;
        public SeqImgAnim.FixedDurationAnimProperties knockedOut;
        public SeqImgAnim.FixedDurationAnimProperties recovering;
        public SeqImgAnim.FixedDurationAnimProperties quacking;
        public SeqImgAnim.FixedDurationAnimProperties swingingCake;
        public SeqImgAnim.FixedDurationAnimProperties movingWall;

        [System.NonSerialized]
        public PlayerManager  playerManager;


        static Dictionary<DuckSkin, Sprite[]>         _SkinShowingSprites    = new Dictionary<DuckSkin, Sprite[]>();
        static Dictionary<DuckSkin, AnimStateSprites> _DuckSkinsSprites      = new Dictionary<DuckSkin, AnimStateSprites>();
        static AnimStateSprites[]                     _CarriedAmountsSprites;
        static AnimStateSprites                       _DuckSnowCakeStatesSprites;
        static bool _IsSpritesLoaded = false;

        Dictionary<PlayerState, SeqImgAnim.FixedDurationAnimProperties> _animsProps;
        PlayerState _currentAnimState;

        IEnumerator _currentPlayingAnim;


        public static void LoadSpritesResources () {
            if (_IsSpritesLoaded)
                return;


            // Create tempAnimStateSpritesLists
            List<Sprite> tempShowingSpritesList = new List<Sprite>();
            Dictionary<PlayerState, List<Sprite>> tempAnimStateSpritesLists = new Dictionary<PlayerState, List<Sprite>>();
            foreach (PlayerState state in System.Enum.GetValues(typeof(PlayerState))) {
                tempAnimStateSpritesLists.Add(state, new List<Sprite>());
            }


            // == Load Ducks ==
            foreach (DuckSkin duckSkin in _DuckSkinResourceFileName.Keys) {

                // init
                _DuckSkinsSprites.Add(duckSkin, new AnimStateSprites());

                // clear temp
                tempShowingSpritesList.Clear();
                foreach (List<Sprite> spritesList in tempAnimStateSpritesLists.Values) {
                    spritesList.Clear();
                }

                // load sources
                Sprite[] duckSkinSprites = Resources.LoadAll<Sprite>("Sprites/Players/" + _DuckSkinResourceFileName[duckSkin]);

                foreach (Sprite sprite in duckSkinSprites) {

                    if (sprite.name.Contains("SkinShowing")) {
                        tempShowingSpritesList.Add(sprite);
                    }
                    else {
                        foreach(PlayerState state in tempAnimStateSpritesLists.Keys) {

                            if (sprite.name.Contains(state.ToString())) {
                                tempAnimStateSpritesLists[state].Add(sprite);
                                break;
                            }
                        }
                    }
                }

                // get results
                _SkinShowingSprites.Add(duckSkin, tempShowingSpritesList.ToArray());
                foreach (PlayerState state in tempAnimStateSpritesLists.Keys) {
                    _DuckSkinsSprites[duckSkin].statesAnimSprites.Add( state, tempAnimStateSpritesLists[state].ToArray() );
                }

            }


            // == Load Carried ==
            _CarriedAmountsSprites = new AnimStateSprites[Global.PLAYER_MAX_CARRIED_AMOUNT + 1];
            Sprite[] carriedSnowSprites = Resources.LoadAll<Sprite>("Sprites/Players/Carried Snow Anims");

            for (int i = 0 ; i < _CarriedAmountsSprites.Length ; i++) {

                // init
                _CarriedAmountsSprites[i] = new AnimStateSprites();

                // clear temp
                foreach (List<Sprite> spritesList in tempAnimStateSpritesLists.Values) {
                    spritesList.Clear();
                }

                foreach (Sprite sprite in carriedSnowSprites) {

                    foreach (PlayerState state in tempAnimStateSpritesLists.Keys) {

                        if (sprite.name.Contains(state.ToString() + "_" + i + "-snow")) {
                            tempAnimStateSpritesLists[state].Add(sprite);
                        }
                    }
                }

                // get results
                foreach (PlayerState state in tempAnimStateSpritesLists.Keys) {
                    _CarriedAmountsSprites[i].statesAnimSprites.Add( state, tempAnimStateSpritesLists[state].ToArray() );
                }

            }


            // == Load Duck Snow Cake ==
            Sprite[] duckSnowCakeSprites = Resources.LoadAll<Sprite>("Sprites/Players/Duck Snow Cake Anims");

            // clear temp
            foreach (List<Sprite> spritesList in tempAnimStateSpritesLists.Values) {
                spritesList.Clear();
            }

            foreach (Sprite sprite in duckSnowCakeSprites) {

                foreach (PlayerState state in tempAnimStateSpritesLists.Keys) {

                    if (sprite.name.Contains(state.ToString())) {
                        tempAnimStateSpritesLists[state].Add(sprite);
                    }
                }
            }

            // get results
            _DuckSnowCakeStatesSprites = new AnimStateSprites();
            foreach (PlayerState state in tempAnimStateSpritesLists.Keys) {
                _DuckSnowCakeStatesSprites.statesAnimSprites.Add( state, tempAnimStateSpritesLists[state].ToArray() );
            }


            _IsSpritesLoaded = true;
        }

        public static void ReleaseLoaded () {
            _SkinShowingSprites.Clear();
            _DuckSkinsSprites.Clear();
            _CarriedAmountsSprites = null;
            _DuckSnowCakeStatesSprites = null;

            _IsSpritesLoaded = false;
        }


        void Awake () {

            playerManager = gameObject.GetComponent<PlayerManager>();

            _animsProps = new Dictionary<PlayerState, SeqImgAnim.FixedDurationAnimProperties>() {
                { PlayerState.Idle, idle },
                { PlayerState.Walking, walking },
                { PlayerState.Turning, turning },
                { PlayerState.Collecting, collecting },
                { PlayerState.Firing, firing },
                { PlayerState.Building, building },
                { PlayerState.Repairing, repairing },
                { PlayerState.KnockedOut, knockedOut },
                { PlayerState.Downed, knockedOut },
                { PlayerState.Recovering, recovering },
                { PlayerState.Quacking, quacking },
                { PlayerState.SwingingCake, swingingCake },
                { PlayerState.MovingWall, movingWall }
            };

            LoadSpritesResources();
        }


        void Start () {
            PlayAnim(PlayerState.Idle);
            quackWordSR.enabled = false;
        }


        void Update () {

            PlayerState playerCurrentState = playerManager.CurrentState;

            if (playerCurrentState != _currentAnimState)
                PlayAnim(playerCurrentState);

            if (_currentAnimState != PlayerState.Quacking)
                quackWordSR.enabled = false;
        }

        void PlayAnim (PlayerState state) {

            float overrideDuration = 0f;

            if (!_animsProps[state].loop && Global.CurrentRoundInstance.GetPlayerActionTime(state) != 0)
                overrideDuration = Global.CurrentRoundInstance.GetPlayerActionTime(state);

            _currentAnimState = state;

            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            _currentPlayingAnim = Anim(state, overrideDuration);
            StartCoroutine(_currentPlayingAnim);
        }

        IEnumerator Anim (PlayerState state, float overrideDuration = 0f) {

            Sprite[] duckSprites = _DuckSkinsSprites[currentDuckSkin].statesAnimSprites[state];
            Sprite[] carriedSprites = _CarriedAmountsSprites[playerManager.CarriedAmount].statesAnimSprites[state];
            Sprite[] duckSnowCakeSprites = _DuckSnowCakeStatesSprites.statesAnimSprites[state];

            float animDuration = overrideDuration > 0 ? overrideDuration : _animsProps[state].duration;
            bool  loop = _animsProps[state].loop;
            float startTime = Time.time;
            int   currentFrameIndex = 0;
            int   prevFrameIndex = -1;

            while (currentFrameIndex < duckSprites.Length) {

                if (currentFrameIndex != prevFrameIndex) {

                    duckSR.sprite = duckSprites[currentFrameIndex];
                    carriedSR.sprite = GetSpriteByFrameIndex(carriedSprites, currentFrameIndex);
                    duckSnowCakeSR.sprite = GetSpriteByFrameIndex(duckSnowCakeSprites, currentFrameIndex);

                    if (state == PlayerState.Turning) {
                        if (currentFrameIndex == TURNING_ANIM_FLIP_INDEX)
                            playerManager.TurnByAnim();
                    }
                    else if (state == PlayerState.Quacking) {
                        if (currentFrameIndex > 1) // when frame index at {2, 3} show the word
                            quackWordSR.enabled = true;
                    }

                    prevFrameIndex = currentFrameIndex;
                }

                yield return null;

                currentFrameIndex = (int) ((Time.time - startTime) / animDuration * duckSprites.Length);

                if (loop)
                    currentFrameIndex = currentFrameIndex % duckSprites.Length;
            }

            _currentPlayingAnim = null;
        }

        Sprite GetSpriteByFrameIndex (Sprite[] sprites, int frameIndex) {
            if (frameIndex < sprites.Length)
                return sprites[frameIndex];
            return null;
        }

        public void PlayCurrentFromStart () {
            PlayAnim(_currentAnimState);
        }

    }
}
