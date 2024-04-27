using Enum = System.Enum;
using Math = System.Math;
using TimeSpan = System.TimeSpan;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UniRx;
using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class VotingInstanceAnimationManager : MonoBehaviour {

        [System.Serializable]
        public class SlideAnimProps {
            public Ease  ease;
            public float duration;
            public float distance;
        }

        public Transform mainPanelTrans;
        public Transform ducksParent;

        [Header("Animation Properties")]
        public SlideAnimProps mainPanelShowUpAnimProps;
        public SlideAnimProps mainPanelGoOutAnimProps;
        public SlideAnimProps ducksShowUpAnimProps;
        public SlideAnimProps ducksGoOutAnimProps;
        public SlideAnimProps candidateAnimProps;

        public float ducksShowUpDelayedTime;
        public float candidatesShowUpTimeInterval;

        public float waitForElectedAnimDuration;


        public float GoOutDuration => Math.Max(mainPanelGoOutAnimProps.duration, ducksGoOutAnimProps.duration);



        public void PlayShowUp () {
            DOTween.Sequence()
                .Append( SlideInAnimTween(mainPanelTrans, mainPanelShowUpAnimProps) )
                .Insert( ducksShowUpDelayedTime, SlideInAnimTween(ducksParent, ducksShowUpAnimProps) );
        }

        public void PlayShowAllCandidates (RuleCard[] cards) {
            Sequence wholeAnimSeq = DOTween.Sequence();

            for (int i = 0 ; i < cards.Length ; i++) {
                Sequence animSeq = CandidateShowUpAnimSeq(cards[i]);

                if (i == 0)
                    wholeAnimSeq.Append( animSeq );
                else
                    wholeAnimSeq.Insert( i * candidatesShowUpTimeInterval, animSeq );
            }
        }

        public void PlayElectedAnim (RuleCard[] cards, int electedIndex) {

            DOTween.Sequence()
                .AppendInterval( waitForElectedAnimDuration )
                .AppendCallback( () => {
                    for (int i = 0 ; i < cards.Length ; i++) {
                        cards[i].ShowElectedResult( (i == electedIndex) );
                    }
                } );
        }

        public void PrepareToGoOut (float remainedTime, TweenCallback endCallback) {
            DOTween.Sequence()
                .AppendInterval( remainedTime - GoOutDuration )
                .Append( SlideOutAnimTween(mainPanelTrans, mainPanelGoOutAnimProps) )
                .Join( SlideOutAnimTween(ducksParent, ducksGoOutAnimProps) )
                .OnComplete(endCallback);
        }


        Sequence CandidateShowUpAnimSeq (RuleCard card) {
            return DOTween.Sequence()
                .Append( SlideInAnimTween(card.transform, candidateAnimProps) )
                .PrependCallback( () => card.gameObject.SetActive(true) );
        }

        Tween SlideInAnimTween (Transform targetTrans, SlideAnimProps props) {
            return targetTrans.DOMoveY(props.distance, props.duration)
                .From()
                .SetEase(props.ease);
        }

        Tween SlideOutAnimTween (Transform targetTrans, SlideAnimProps props) {
            return targetTrans.DOMoveY(props.distance, props.duration)
                .SetEase(props.ease);
        }

    }
}
