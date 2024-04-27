using System.Collections;

using UnityEngine;

using DoubleHeat.Animation;

namespace DoubleHeat.SnowFightForDucksGame {

    public class RuleCardShowingAnimationManager : MonoBehaviour {

        [System.Serializable]
        public class ShowingAnimProps {
            public SeqImgAnim.FixedFrameRateAnimProperties basicProps;
            public float startAgainWaitingTime;
        }


        public SpriteRenderer showingSR;

        public ShowingAnimProps animStatuesRepairedOnceAtRoundEndProps;
        public ShowingAnimProps animRepairProhibitedProps;
        public ShowingAnimProps animBuildProhibitedProps;
        public ShowingAnimProps animStatuesRottingProps;

        public ShowingAnimProps animKnockOutFartherProps;
        public ShowingAnimProps animKnockDownProhibitedProps;
        public ShowingAnimProps animHighGravityProps;
        public ShowingAnimProps animTurnProhibitedProps;

        public ShowingAnimProps animSlowerAndCollidableSnowballProps;
        public ShowingAnimProps animKnuckleBallProps;
        public ShowingAnimProps animCurveBallProps;
        public ShowingAnimProps animFire2GoesVProps;

        public ShowingAnimProps animSnowWallBuildFastAndRottingProps;
        public ShowingAnimProps animSnowWallCostDownProps;
        public ShowingAnimProps animEggsWallMovableProps;
        public ShowingAnimProps animTravelingStatueProps;

        public ShowingAnimProps animDuckSnowCakeProps;


        public bool AlwaysPlayAnim {
            get => _alwaysPlayAnim;
            set {
                bool prev = _alwaysPlayAnim;
                _alwaysPlayAnim = value;

                if (_alwaysPlayAnim && _currentPlaying == null)
                    PlayAnimFromStart();
                else if (!_alwaysPlayAnim && prev)
                    PlayTitleImage();
            }
        }


        Sprite   _titleImage;
        Sprite[] _animSprites;
        ShowingAnimProps _animProps;
        bool _alwaysPlayAnim = false;

        Coroutine _currentPlaying;
        Coroutine _currentWaitingForNextStart;


        public void Init (string ruleName, bool alwaysPlayAnim = false) {
            LoadShowingAnimSprites(ruleName);
            SetAnimPropsByRuleName(ruleName);

            if (alwaysPlayAnim)
                PlayAnimFromStart();
            else
                PlayTitleImage();

            _alwaysPlayAnim = alwaysPlayAnim;
        }


        public void PlayTitleImage () {
            if (_alwaysPlayAnim)
                return;

            if (_currentPlaying != null) {
                StopCoroutine(_currentPlaying);
                _currentPlaying = null;
            }
            if (_currentWaitingForNextStart != null) {
                StopCoroutine(_currentWaitingForNextStart);
                _currentWaitingForNextStart = null;
            }

            if (_titleImage != null)
                showingSR.sprite = _titleImage;
        }


        public void PlayAnim () {
            if (_currentPlaying == null) {
                PlayAnimFromStart();
            }
        }

        public void PlayAnimFromStart () {
            if (_currentPlaying != null)
                StopCoroutine(_currentPlaying);

            if (_animSprites != null)
                _currentPlaying = StartCoroutine(SeqImgAnim.Anim(showingSR, _animSprites, _animProps.basicProps.fps, _animProps.basicProps.loop, _animProps.basicProps.pingPong, AnimEnded));
        }

        void AnimEnded () {
            _currentWaitingForNextStart = StartCoroutine(WaitForNextStart());
        }

        IEnumerator WaitForNextStart () {
            yield return new WaitForSeconds(_animProps.startAgainWaitingTime);
            PlayAnimFromStart();
        }




        void LoadShowingAnimSprites (string ruleName) {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Voting/Rule Card Showing Anims/" + ruleName.ToString());

            if (sprites != null) {

                _titleImage = sprites[0];

                Sprite[] animSprites = new Sprite[sprites.Length - 1];
                for (int i = 0 ; i < animSprites.Length ; i++) {
                    animSprites[i] = sprites[i + 1];
                }
                _animSprites = animSprites;
            }
        }

        void SetAnimPropsByRuleName (string name) {

            if      (name == FixedRule.StatuesRepairedOnceAtRoundEnd.ToString())
                _animProps = animStatuesRepairedOnceAtRoundEndProps;
            else if (name == FixedRule.RepairProhibited.ToString())
                _animProps = animRepairProhibitedProps;
            else if (name == FixedRule.BuildProhibited.ToString())
                _animProps = animBuildProhibitedProps;
            else if (name == FixedRule.StatuesRotting.ToString())
                _animProps = animStatuesRottingProps;

            // -- Player --
            else if (name == OptionalRule.KnockOutFarther.ToString())
                _animProps = animKnockOutFartherProps;
            else if (name == OptionalRule.KnockDownProhibited.ToString())
                _animProps = animKnockDownProhibitedProps;
            else if (name == OptionalRule.HighGravity.ToString())
                _animProps = animHighGravityProps;
            else if (name == OptionalRule.TurnProhibited.ToString())
                _animProps = animTurnProhibitedProps;

            // -- Snowball --
            else if (name == OptionalRule.SlowerAndCollidableSnowball.ToString())
                _animProps = animSlowerAndCollidableSnowballProps;
            else if (name == OptionalRule.KnuckleBall.ToString())
                _animProps = animKnuckleBallProps;
            else if (name == OptionalRule.CurveBall.ToString())
                _animProps = animCurveBallProps;
            else if (name == OptionalRule.Fire2GoesV.ToString())
                _animProps = animFire2GoesVProps;

            // -- Building --
            else if (name == OptionalRule.SnowWallBuildFastAndRotting.ToString())
                _animProps = animSnowWallBuildFastAndRottingProps;
            else if (name == OptionalRule.SnowWallCostDown.ToString())
                _animProps = animSnowWallCostDownProps;
            else if (name == OptionalRule.EggsWallMovable.ToString())
                _animProps = animEggsWallMovableProps;
            else if (name == OptionalRule.TravelingStatue.ToString())
                _animProps = animTravelingStatueProps;

            // -- Special --
            else if (name == OptionalRule.DuckSnowCake.ToString())
                _animProps = animDuckSnowCakeProps;

        }


    }
}
