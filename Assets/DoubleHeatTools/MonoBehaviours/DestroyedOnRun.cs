using UnityEngine;

namespace DoubleHeat {

    public class DestroyedOnRun : MonoBehaviour {

        public bool isEnabled = true;

        void Awake () {
            if (isEnabled)
                Destroy(gameObject);
        }
    }
}
