using System.Runtime.Serialization;

using UnityEngine;

namespace DoubleHeat.Serialization {

    public class Vector2SerializationSurrogate : ISerializationSurrogate {

        public void GetObjectData (object obj, SerializationInfo info, StreamingContext context) {
            Vector2 v2 = (Vector2) obj;
            info.AddValue("x", v2.x);
            info.AddValue("y", v2.y);
        }

        public object SetObjectData (object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Vector2 v2 = (Vector2) obj;
            v2.x = (float) info.GetValue("x", typeof(float));
            v2.y = (float) info.GetValue("y", typeof(float));
            obj = v2;
            return obj;
        }
    }


    public class Vector3SerializationSurrogate : ISerializationSurrogate {

        public void GetObjectData (object obj, SerializationInfo info, StreamingContext context) {
            Vector3 v3 = (Vector3) obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        public object SetObjectData (object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Vector3 v3 = (Vector3) obj;
            v3.x = (float) info.GetValue("x", typeof(float));
            v3.y = (float) info.GetValue("y", typeof(float));
            v3.z = (float) info.GetValue("z", typeof(float));
            obj = v3;
            return obj;
        }
    }


    public class QuaternionSerializationSurrogate : ISerializationSurrogate {

        public void GetObjectData (object obj, SerializationInfo info, StreamingContext context) {
            Quaternion q = (Quaternion) obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
        }

        public object SetObjectData (object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Quaternion q = (Quaternion) obj;
            q.x = (float) info.GetValue("x", typeof(float));
            q.y = (float) info.GetValue("y", typeof(float));
            q.z = (float) info.GetValue("z", typeof(float));
            q.w = (float) info.GetValue("w", typeof(float));
            obj = q;
            return obj;
        }
    }
    
}
