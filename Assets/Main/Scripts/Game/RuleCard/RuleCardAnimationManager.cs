using UnityEngine;

using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class RuleCardAnimationManager : MonoBehaviour {

        [System.Serializable]
        public class AnimProps {
            public float nonElectedGoOutSpeed;
            public float electedMoveToCenterDuration;
            public float electedMoveToCenterScaledRate;
            public float electedMoveOutDuration;
            public float electedMoveOutDistance;
        }

        public AnimProps animProps;

        public void PlayElectedAnim () {
            DOTween.Sequence()
                .Append( transform.DOMove(Vector3.zero, animProps.electedMoveToCenterDuration) )
                .Join( transform.DOScale(animProps.electedMoveToCenterScaledRate, animProps.electedMoveToCenterDuration) );
        }

        public void NonElectedGoOut () {
            transform.DOMoveY(-10000f, animProps.nonElectedGoOutSpeed)
                .SetRelative()
                .SetSpeedBased();
        }

        public void ElectedPrepareToGoOut (float remainedTime) {
            DOTween.Sequence()
                .AppendInterval( remainedTime - animProps.electedMoveOutDuration )
                .Append( transform.DOMoveY(animProps.electedMoveOutDistance, animProps.electedMoveOutDuration)
                    .SetRelative()
                );
        }

    }
}
