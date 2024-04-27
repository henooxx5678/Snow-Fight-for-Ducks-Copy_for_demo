using UnityEngine;

using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class EggsWallGetHitDetector : GetHitDetector {

        public Transform eggsWallTrans;

        public float shiftDistance;
        public float getHitAnimDuration;

        void Awake () {
            _type = Snowball.TargetType.Immortal;

        }


        void OnTriggerEnter2D (Collider2D other) {

            if (other.tag == "Snowball") {
                if (Vector2.Dot(other.gameObject.GetComponent<Snowball>().FlyingDirection, Vector2.right) > 0) {
                    // right
                    GetHitAnimTween(1);
                }
                else {
                    // left
                    GetHitAnimTween(-1);
                }

            }
        }


        Tween GetHitAnimTween (int dir) {
            eggsWallTrans.DOKill();
            return eggsWallTrans.DOMoveX(shiftDistance * dir, getHitAnimDuration / 2)
                .SetRelative()
                .SetLoops(2, LoopType.Yoyo);
        }

    }
}
