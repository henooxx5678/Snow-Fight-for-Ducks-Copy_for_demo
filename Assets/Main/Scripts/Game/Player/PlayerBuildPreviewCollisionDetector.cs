using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class PlayerBuildPreviewCollisionDetector : MonoBehaviour {

        public PlayerBuildPreviewManager buildPreviewManager;

        void OnTriggerStay2D (Collider2D other) {
            buildPreviewManager.IllegalToBuild();
        }


    }
}
