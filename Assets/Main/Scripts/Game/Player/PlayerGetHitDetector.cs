using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class PlayerGetHitDetector : GetHitDetector {

        public PlayerManager playerManager;

        void Awake () {
            _type = Snowball.TargetType.Player;
        }

        void OnTriggerEnter2D (Collider2D other) {
            if (other.tag == "Snowball") {

                Snowball snowball = other.gameObject.GetComponent<Snowball>();

                if (snowball.OwnerNumber != playerManager.photonView.Owner.ActorNumber) {

                    playerManager.GetHit(snowball.OwnerNumber, snowball.IdByOwner, snowball.FlyingDirection);
                }

            }
        }
    }

}
