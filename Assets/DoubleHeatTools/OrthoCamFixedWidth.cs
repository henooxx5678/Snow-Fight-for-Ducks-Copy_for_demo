using UnityEngine;

namespace DoubleHeat {

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class OrthoCamFixedWidth : MonoBehaviour {

        public float width;


        Camera _cam;
        public Camera Cam {
            get {
                if (_cam == null)
                    _cam = GetComponent<Camera>();

                return _cam;
            }
        }

        float _prevAspect = -1f;

        void OnValidate () {
            if (Cam.orthographic) {
                UpdateOrthoSize();
            }
        }

        void Awake () {
            if (Cam.orthographic)
                _prevAspect = Cam.aspect;
        }

        void Update () {

            if (Cam.orthographic) {
                if (_prevAspect != Cam.aspect) {
                    UpdateOrthoSize();
                }
            }

            _prevAspect = Cam.aspect;

            #if UNITY_EDITOR

            if (!Application.isPlaying) {
                if (Cam.orthographic) {
                    width = Cam.orthographicSize * Cam.aspect;
                }
            }

            #endif

        }

        void UpdateOrthoSize () {
            Cam.orthographicSize = width / Cam.aspect;
        }

    }

}
