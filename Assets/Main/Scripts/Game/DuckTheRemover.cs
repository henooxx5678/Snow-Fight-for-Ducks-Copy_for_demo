using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class DuckTheRemover : MonoBehaviour {


        void Awake () {
            gameObject.SetActive(false);
        }

        void OnTriggerEnter2D (Collider2D other) {

            if (other.tag == "RuleCard") {
                other.transform.SetParent(transform);
            }

        }

    }
}
