using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class RuleCardCanvasEventHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

        public RuleCard ruleCard;

        RectTransform _rectTrans;
        WaitForSeconds _checkIntervalWaitForTime = new WaitForSeconds(0.5f);
        Coroutine _currentCheckForOutOfView;


        void Awake () {
            _rectTrans = gameObject.GetComponent<RectTransform>();
        }

        void OnDestroy () {
            if (_currentCheckForOutOfView != null)
                StopCoroutine(_currentCheckForOutOfView);
        }


        IEnumerator CheckIfOutOfViewport () {
            while (true) {

                if (_rectTrans == null || !_rectTrans.IsInViewport(Global.gameSceneManager.mainCam))
                    ruleCard.OnOutOfViewport();

                yield return _checkIntervalWaitForTime;
            }
        }


        public void OnPointerClick (PointerEventData eventData) {
            ruleCard.Click();
        }

        public void OnPointerEnter (PointerEventData eventData) {
            ruleCard.PointerEnter();
        }

        public void OnPointerExit (PointerEventData eventData) {
            ruleCard.PointerExit();
        }


        // not used for now
        public void StartToCheckIfOutOfViewport () {
            _currentCheckForOutOfView = StartCoroutine(CheckIfOutOfViewport());
        }

    }
}
