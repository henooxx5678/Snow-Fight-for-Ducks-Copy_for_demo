
namespace DoubleHeat {

    [System.Serializable]
    public class FloatRange {

        public float min;
        public float max;

        public FloatRange (float minOrCenter, float maxOrExtents, bool usingCenterAndExtents = false) {
            if (!usingCenterAndExtents) {
                min = minOrCenter;
                max = maxOrExtents;
            }
            else {
                min = minOrCenter - maxOrExtents;
                max = minOrCenter + maxOrExtents;
            }
        }

        public bool IsInRange (float value) {
            return value >= min && value <= max;
        }
    }
}
