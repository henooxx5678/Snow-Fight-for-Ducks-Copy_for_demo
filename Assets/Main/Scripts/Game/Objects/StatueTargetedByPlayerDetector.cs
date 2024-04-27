using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class StatueTargetedByPlayerDetector : TargetedByPlayerDetector {

        public Statue statue;

        void Awake () {
            _type = TargetableByPlayer.Statue;
        }

    }
}
