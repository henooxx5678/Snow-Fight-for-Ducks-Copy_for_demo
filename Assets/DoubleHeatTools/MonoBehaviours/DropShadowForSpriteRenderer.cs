using UnityEngine;

namespace DoubleHeat {

    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteInEditMode]
    public class DropShadowForSpriteRenderer : MonoBehaviour {

        public SpriteRenderer sourceSpriteRenderer;
        [Range(-180f, 180f)]
        public float shadowDirectionAngle;
        public float shadowDistance;
        [Range(0f, 1f)]
        public float shadowOpacity = 0.5f;

        SpriteRenderer _sr;

        void Awake () {
            _sr = gameObject.GetComponent<SpriteRenderer>();
        }

        void Update () {
            _sr.sprite = sourceSpriteRenderer.sprite;

            Vector3 dir = DataCompression.AngleDegreeToDirection2D(shadowDirectionAngle);

            transform.position = sourceSpriteRenderer.transform.position + new Vector3(dir.x * transform.lossyScale.x, dir.y * transform.lossyScale.y, 0f) * shadowDistance;
            _sr.color = new Color(0f, 0f, 0f, shadowOpacity);
        }

    }
}
