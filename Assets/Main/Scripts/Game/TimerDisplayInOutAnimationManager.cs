using UnityEngine;

using DG.Tweening;


namespace DoubleHeat.SnowFightForDucksGame {

    public class TimerDisplayInOutAnimationManager : MonoBehaviour {

        public RectTransform positionHandlerRectTrans;

        public Ease  outEase;
        public float duration;
        public float shiftDistance;


        void Awake () {
            positionHandlerRectTrans.DOAnchorPosY(shiftDistance, duration)
                .SetRelative()
                .SetEase(outEase)
                .SetAutoKill(false);

            positionHandlerRectTrans.DOPause();
        }


        public void GoOut () {
            positionHandlerRectTrans.DORestart();
        }

        public void GoIn () {
            positionHandlerRectTrans.DOComplete();
            positionHandlerRectTrans.DOPlayBackwards();
        }

    }
}
