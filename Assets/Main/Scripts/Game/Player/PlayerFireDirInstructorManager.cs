using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class PlayerFireDirInstructorManager : MonoBehaviour {

        public SpriteRenderer sr;


        bool _isShowing = false;


        void Update () {

            sr.enabled = _isShowing;

            _isShowing = false;
        }


        public void Show (Color color, Vector2 dir) {
            _isShowing = true;
            transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);

            if (sr != null)
                sr.color = color;
        }

    }
}
