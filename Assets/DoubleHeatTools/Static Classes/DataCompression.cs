using System;

using UnityEngine;

namespace DoubleHeat {

    public static class DataCompression {

        public static float Direction2DToAngleDegree (Vector2 dir) {
            if (dir == Vector2.zero)
                return 0f;
            return Vector2.SignedAngle(Vector2.right, dir);
        }

        public static Vector2 AngleDegreeToDirection2D (float angle) {
            return (Vector2) (Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right);
        }

        public static byte[] ToByteArray (int[] a) {
            byte[] r = new byte[a.Length];
            for (int i = 0 ; i < r.Length ; i++) {
                r[i] = (byte) a[i];
            }
            return r;
        }

        public static int[] ByteArrayToIntArray (byte[] a) {
            int[] r = new int[a.Length];
            for (int i = 0 ; i < r.Length ; i++) {
                r[i] = (int) a[i];
            }
            return r;
        }
    }
}
