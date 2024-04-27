using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public struct Vector2Log {
        public Vector2 v2;
        public float time;

        public Vector2Log (Vector2 v, float t) {
            v2 = v;
            time = t;
        }

        public static Vector2Log zero = new Vector2Log(Vector2.zero, 0f);
    }
}
