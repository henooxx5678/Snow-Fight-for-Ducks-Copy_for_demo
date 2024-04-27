using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class StatueGetHitDetector : GetHitDetector {

        public Statue statue;

        void Awake () {
            _type = Snowball.TargetType.Statue;
        }

        // == takeovered by snowball ==
        // void OnTriggerEnter2D (Collider2D other) {
        //     if (other.tag == "Snowball") {
        //         statue.GetHit();
        //     }
        // }
    }

}
