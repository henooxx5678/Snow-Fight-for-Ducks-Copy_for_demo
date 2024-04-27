using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace DoubleHeat.Animation {

    public class SeqImgAnim {

        public delegate void AnimEndCallback ();


        public class AnimPropertiesOptions {
            public bool loop;
            public bool pingPong;
        }

        [System.Serializable]
        public class FixedDurationAnimProperties : AnimPropertiesOptions {
            public float duration;
        }

        [System.Serializable]
        public class FixedFrameRateAnimProperties : AnimPropertiesOptions {
            public float fps;
        }


        public static IEnumerator Anim (SpriteRenderer sr, Sprite[] sprites, float fps, bool loop, bool pingPong, AnimEndCallback endCallback, float startTimeCompensation = 0f) {

            float startTime = Time.time + startTimeCompensation;
            int   framesLength = sprites.Length;
            int   currentFrameIndex = 0;

            if (pingPong) {
                framesLength *= 2;
            }

            while (currentFrameIndex < framesLength) {

                int actualFrameIndex = currentFrameIndex;

                if (currentFrameIndex >= sprites.Length) {
                    if (pingPong) {
                        actualFrameIndex = sprites.Length * 2 - (currentFrameIndex + 1);
                    }
                }

                if (actualFrameIndex < sprites.Length)
                    sr.sprite = sprites[actualFrameIndex];
                else
                    sr.sprite = null;

                yield return null;

                currentFrameIndex = (int) ((Time.time - startTime) * fps);

                if (loop) {
                    currentFrameIndex = currentFrameIndex % framesLength;
                }
            }

            if (endCallback != null)
                endCallback();
        }

        public static IEnumerator ImgAnim (Image img, Sprite[] sprites, bool keepNativeSize, Vector2[] anchoredPositions, float fps, bool loop, bool pingPong, AnimEndCallback endCallback, float startTimeCompensation = 0f) {

            float startTime = Time.time + startTimeCompensation;
            int   framesLength = sprites.Length;
            int   currentFrameIndex = 0;

            if (pingPong) {
                framesLength *= 2;
            }

            while (currentFrameIndex < framesLength) {

                int actualFrameIndex = currentFrameIndex;

                if (currentFrameIndex >= sprites.Length) {
                    if (pingPong) {
                        actualFrameIndex = sprites.Length * 2 - (currentFrameIndex + 1);
                    }
                }

                if (actualFrameIndex < sprites.Length) {
                    img.sprite = sprites[actualFrameIndex];
                    if (keepNativeSize)
                        img.SetNativeSize();
                }
                else {
                    img.sprite = null;
                }

                if (anchoredPositions != null && actualFrameIndex < anchoredPositions.Length) {
                    ((RectTransform) img.transform).anchoredPosition = anchoredPositions[actualFrameIndex];
                }

                yield return null;

                currentFrameIndex = (int) ((Time.time - startTime) * fps);

                if (loop) {
                    currentFrameIndex = currentFrameIndex % framesLength;
                }
            }

            if (endCallback != null)
                endCallback();
        }



    }
}
