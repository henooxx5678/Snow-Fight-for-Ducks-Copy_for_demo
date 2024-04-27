using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class DuckSnowCakeGetHitDetector : GetHitDetector {

        public PlayerManager playerManager;

        void Awake () {
            _type = Snowball.TargetType.DuckSnowCake;
        }

        void OnTriggerEnter2D (Collider2D other) {
            if (other.tag == "Snowball") {
                playerManager.DuckSnowCakeHit();
            }
        }
    }

}
