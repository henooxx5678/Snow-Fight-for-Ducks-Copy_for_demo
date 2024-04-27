using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class SnowWallGetHitDetector : GetHitDetector {

        public SnowWall snowWall;

        void Awake () {
            _type = Snowball.TargetType.SnowWall;
        }

        // == takeovered by snowball ==
        // void OnTriggerEnter2D (Collider2D other) {
        //     if (other.tag == "Snowball") {
        //         snowWall.GetHit();
        //     }
        // }
    }

}
