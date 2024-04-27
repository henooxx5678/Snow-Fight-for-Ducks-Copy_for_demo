using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum TargetableByPlayer {
        Statue,
        EggsWall,
        None
    }

    public class TargetedByPlayerDetector : MonoBehaviour {

        public TargetableByPlayer Type => _type;

        protected TargetableByPlayer _type = TargetableByPlayer.None;

    }
}
