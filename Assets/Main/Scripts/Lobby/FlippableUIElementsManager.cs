using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class FlippableUIElementsManager : MonoBehaviour {

        public Transform[] transformsWhoKeepRotation;

        public void CorrectRotation () {

            foreach (Transform trans in transformsWhoKeepRotation) {

                trans.localRotation = Quaternion.identity;
                Quaternion appliedRotation = trans.rotation;
                trans.rotation = Quaternion.identity;

                for (int i = 0 ; i < trans.childCount ; i++) {
                    trans.GetChild(i).rotation = appliedRotation;
                }
            }
        }

    }
}
