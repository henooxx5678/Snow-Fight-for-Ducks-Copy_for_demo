using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UniRx;
using DG.Tweening;

using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class WelcomePanelManager : MonoBehaviour {

        public Image           coverImg;
        public Image           titleWordsImg;
        public TextMeshProUGUI anyKeyMessage;

        public float coverImageAnimShiftDistance;
        public float coverImageAnimDuration;

        public float titleWordsAnimStartTime;
        public float titleWordsShowingAnimDuration;
        public float titleWordsShowingAnimShiftDistance;

        public float durationBetweenTitleWordsAnimAndAnyKeyMessageAnim;

        public float anyKeyMessageAnimIntervalTime;
        public float anyKeyMessageFadeDuration;

        Sequence _introAnimSeq;
        Tween _anyKeyMessageAnim;


        void OnEnable () {
            PlayIntro();
        }

        void PlayIntro () {

            RectTransform titleWordsTrans = titleWordsImg.transform as RectTransform;
            float targetY = titleWordsTrans.anchoredPosition.y;
            titleWordsTrans.anchoredPosition = titleWordsTrans.anchoredPosition.GetAfterSetY(targetY - titleWordsShowingAnimShiftDistance);

            _introAnimSeq = DOTween.Sequence().Append( coverImg.GetComponent<RectTransform>().DOAnchorPosY(-coverImageAnimShiftDistance, coverImageAnimDuration )
                .From(true)
                .SetEase(Ease.OutSine)
            )
            .Join( coverImg.DOFade(0.01f, coverImageAnimDuration)
                .From()
                .SetEase(Ease.OutQuint)
            )
            .Insert( titleWordsAnimStartTime, titleWordsImg.DOFade(1f, titleWordsShowingAnimDuration )
                .From(0f, true)
                .SetEase(Ease.OutQuad)
            )
            .Join( titleWordsTrans.DOAnchorPosY(targetY, titleWordsShowingAnimDuration, false)
                .SetEase(Ease.OutSine)
            )
            .AppendInterval(durationBetweenTitleWordsAnimAndAnyKeyMessageAnim)
            .Append( anyKeyMessage.DOFade(0f, anyKeyMessageFadeDuration)
                .From()
                .SetEase(Ease.OutCubic)
            )
            .OnComplete(StartAnyKeyMessageAnim);
        }

        void StartAnyKeyMessageAnim () {

            if (_anyKeyMessageAnim != null)
                _anyKeyMessageAnim.Kill(false);

            _anyKeyMessageAnim = anyKeyMessage.DOFade(0.03f, anyKeyMessageFadeDuration)
                .SetEase(Ease.InCubic)
                .SetLoops(2, LoopType.Yoyo)
                .SetAutoKill(false)
                .OnComplete(() => _anyKeyMessageAnim.Restart(true, anyKeyMessageAnimIntervalTime));
        }

        void OnDisable () {
            _introAnimSeq.Kill(true);
            _anyKeyMessageAnim.Kill(true);
        }

        void Update () {
            if (!string.IsNullOrEmpty(Input.inputString) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {

                if (_introAnimSeq != null && _introAnimSeq.IsPlaying()) {
                    _introAnimSeq.Complete(true);
                }
                else {
                    Global.startSceneManager.PassWelcomeStage();
                }
            }
        }

    }
}
