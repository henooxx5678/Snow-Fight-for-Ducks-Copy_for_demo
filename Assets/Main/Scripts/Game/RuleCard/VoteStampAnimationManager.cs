using UnityEngine;

using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class VoteStampAnimationManager : MonoBehaviour {

        [System.Serializable]
        public class VoteStampAnimProps {
            public float showUpDuration;
            public float showUpShakeStrength;
            public int   showUpVibrato;
            public float snowUpRandomness;
        }


        public VoteStampAnimProps animProps;

        public void PlayShowUpAnim (TweenCallback endCallback) {

            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one, animProps.showUpDuration))
                .Join(transform.DOShakeRotation(animProps.showUpDuration, animProps.showUpShakeStrength, animProps.showUpVibrato, animProps.snowUpRandomness))
                .OnComplete(endCallback);
        }

    }
}
