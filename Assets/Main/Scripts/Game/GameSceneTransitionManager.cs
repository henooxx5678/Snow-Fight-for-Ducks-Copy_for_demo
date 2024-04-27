using Enum = System.Enum;
using Math = System.Math;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UniRx;
using DG.Tweening;
namespace DoubleHeat.SnowFightForDucksGame {

    public class GameSceneTransitionManager : MonoBehaviour {

        [System.Serializable]
        public class MessageAnimProps {

            public float TotalDuration => inDuration + stayDuration + outDuration;

            public Ease  inEase;
            public Ease  stayEase;
            public Ease  outEase;

            public float inDuration;
            public float stayDuration;
            public float outDuration;

            public float inAndOutDistance;
            public float stayShiftDistance;
        }

        public Canvas canvas;
        public Text   endRoundText;
        public Text   nextIsVotingText;
        public Text   fixedRulesAddedText;
        public Text   fixedRulesRemovedText;


        [Header("Animation Properties")]
        public float endRoundMessageToNextIsVotingMessageTimeInterval;
        public float waitForVotingInstanceGoInTime;
        public MessageAnimProps countDownTextAnimProps;
        public MessageAnimProps endRoundTextAnimProps;
        public MessageAnimProps nextIsVotingTextAnimProps;



        public float SwitchToVotingMessagesAnimTotalDuration => endRoundTextAnimProps.TotalDuration + nextIsVotingTextAnimProps.TotalDuration;


        void Awake () {
            Reset();
        }

        void Reset () {

            canvas.enabled = false;
            endRoundText.gameObject.SetActive(false);
            nextIsVotingText.gameObject.SetActive(false);
            fixedRulesAddedText.gameObject.SetActive(false);
            fixedRulesRemovedText.gameObject.SetActive(false);
        }


        public void PlayEndRoundAnim (TweenCallback votingInstacneShowUpCallback, TweenCallback animOnCompleteCallback) {

            canvas.enabled = true;

            DOTween.Sequence()
                .AppendInterval( waitForVotingInstanceGoInTime )
                .AppendCallback( votingInstacneShowUpCallback );

            DOTween.Sequence()
                .Append( MessageAnimSeq(endRoundText.transform, endRoundTextAnimProps) )
                .AppendInterval( endRoundMessageToNextIsVotingMessageTimeInterval )
                .Append( MessageAnimSeq(nextIsVotingText.transform, nextIsVotingTextAnimProps) )
                .OnComplete( () => {
                    Reset();
                    animOnCompleteCallback();
                } );

        }


        Sequence MessageAnimSeq (Transform target, MessageAnimProps props) {

            return DOTween.Sequence()
                .Append( target.DOLocalMoveY(-props.stayShiftDistance, props.inDuration)
                    .From(-props.inAndOutDistance)
                    .SetEase(props.inEase)
                )
                .Append( target.DOLocalMoveY(props.stayShiftDistance, props.stayDuration)
                    .From(-props.stayShiftDistance)
                    .SetEase(props.stayEase)
                )
                .Append( target.DOLocalMoveY(props.inAndOutDistance, props.outDuration)
                    .From(props.stayShiftDistance)
                    .SetEase(props.outEase)
                )
                .PrependCallback( () =>  target.gameObject.SetActive(true) )
                .OnComplete( () => target.gameObject.SetActive(false) );

        }



    }
}
