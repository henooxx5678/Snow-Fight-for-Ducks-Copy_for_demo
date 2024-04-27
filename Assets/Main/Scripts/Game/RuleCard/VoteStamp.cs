using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class VoteStamp : MonoBehaviour {

        public SpriteRenderer duckSR;
        public Sprite yellowDuckSprite;
        public Sprite blackDuckSprite;
        public Sprite whiteDuckSprite;
        public Sprite roastDuckSprite;


        VoteStampAnimationManager _animManager;

        Dictionary<DuckSkin, Sprite> _spriteOfDuckSkins;


        void Awake () {

            _animManager = gameObject.GetComponent<VoteStampAnimationManager>();

            _spriteOfDuckSkins = new Dictionary<DuckSkin, Sprite>() {
                { DuckSkin.Yellow, yellowDuckSprite },
                { DuckSkin.Black, blackDuckSprite },
                { DuckSkin.White, whiteDuckSprite },
                { DuckSkin.Roast, roastDuckSprite }
            };

            transform.localScale = Vector3.zero;
        }

        public void ShowUp (DuckSkin skin, TweenCallback endCallback) {

            duckSR.sprite = _spriteOfDuckSkins[skin];

            if (_animManager != null) {
                _animManager.PlayShowUpAnim(endCallback);
            }
        }

    }
}
