using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class EggsWallTargetedByPlayerDetector : TargetedByPlayerDetector {

        public EggsWall eggsWall;

        void Awake () {
            _type = TargetableByPlayer.EggsWall;
        }

    }
}
