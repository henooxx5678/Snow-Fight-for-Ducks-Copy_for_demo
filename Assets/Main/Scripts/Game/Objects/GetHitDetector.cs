using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class GetHitDetector : MonoBehaviour {

        protected Snowball.TargetType _type;

        public Snowball.TargetType Type {
            get => _type;
        }

    }

}
