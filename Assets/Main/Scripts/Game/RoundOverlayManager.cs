using Enum = System.Enum;
using Math = System.Math;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UniRx;
using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class RoundOverlayManager : MonoBehaviour {

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

        public GameObject startingOverlay;
        public GameObject overlayingUnderlay;
        public Canvas canvas;
        public Text   readyText;
        public Text   countdownText;
        public Text   endRoundText;
        public Text   nextIsVotingText;
        public Text   gameOverText;

        [Header("Animation Properties")]
        public float endRoundMessageToNextIsVotingMessageTimeInterval;
        public float votingInstanceComingTimeProgessRate;
        public MessageAnimProps countDownTextAnimProps;
        public MessageAnimProps endRoundTextAnimProps;
        public MessageAnimProps nextIsVotingTextAnimProps;


        public float SwitchToVotingMessagesAnimTotalDuration => endRoundTextAnimProps.TotalDuration + nextIsVotingTextAnimProps.TotalDuration;
        public float VotingInstanceComingTime => SwitchToVotingMessagesAnimTotalDuration * votingInstanceComingTimeProgessRate;


        void Awake () {
            Reset();
        }

        void Reset () {
            startingOverlay.SetActive(false);
            readyText.gameObject.SetActive(true);

            canvas.enabled = false;
            overlayingUnderlay.SetActive(false);
            countdownText.gameObject.SetActive(false);
            endRoundText.gameObject.SetActive(false);
            nextIsVotingText.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(false);
        }

        public void SetupReadyMessage () {
            startingOverlay.SetActive(true);
            readyText.gameObject.SetActive(true);
        }

        public void PlayCountdownAnim (float timeLeft) {

            canvas.enabled = true;

            readyText.gameObject.SetActive(false);
            StartCoroutine(CountdownAnim( timeLeft, () => Reset() ));
        }

        public void PlayEndRoundAnim (TweenCallback votingInstacneShowUpCallback, TweenCallback animOnCompleteCallback) {

            canvas.enabled = true;
            overlayingUnderlay.SetActive(true);

            DOTween.Sequence()
                .AppendInterval( VotingInstanceComingTime )
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

        public void PlayGameOverAnim (TweenCallback endCallback) {
             canvas.enabled = true;
             overlayingUnderlay.SetActive(true);

             MessageAnimSeq(gameOverText.transform, nextIsVotingTextAnimProps)
                .OnComplete(endCallback);
        }


        IEnumerator CountdownAnim (float timeLeft, TweenCallback endCallback) {

            float targetTime = Time.time + timeLeft;
            int prevShowedNumber = -1;

            while (timeLeft > 0) {

                int showedNumber = (int) timeLeft + 1;

                if (showedNumber != prevShowedNumber) {

                    PlayCountdownNumber(showedNumber);
                    prevShowedNumber = showedNumber;
                }

                yield return null;

                timeLeft = targetTime - Time.time;
            }

            endCallback();
        }

        void PlayCountdownNumber (int number) {
            countdownText.text = number.ToString();
            MessageAnimSeq(countdownText.transform, countDownTextAnimProps);
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
