using UnityEngine;
using UnityEngine.UI;

namespace DoubleHeat.SnowFightForDucksGame {

    [RequireComponent(typeof(Text))]
    public class PlayerNameDisplayManager : MonoBehaviour {

        Text _uiText;
        Transform _followingTarget;


        void Awake () {
            _uiText = gameObject.GetComponent<Text>();
        }

        public void Init (string displayName, Transform following) {

            _uiText.text = displayName;
            _followingTarget = following;

        }

        void Update () {

            if (_followingTarget != null)
                transform.position = _followingTarget.position;
        }

    }
}
