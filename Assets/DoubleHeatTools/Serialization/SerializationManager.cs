using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace DoubleHeat.Serialization {

    public static class SerializationManager {

        public static bool Save (string directory, string filename, object saveData) {

            BinaryFormatter formatter = GetBinaryFormatter();

            if (Directory.Exists(directory)) {
                FileStream stream = new FileStream(directory + filename, FileMode.Create);

                formatter.Serialize(stream, saveData);
                stream.Close();

                return true;
            }

            return false;
        }

        public static object Load (string path) {

            if (File.Exists(path)) {

                BinaryFormatter formatter = GetBinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                try {
                    object data = formatter.Deserialize(stream);
                    stream.Close();
                    return data;
                }
                catch {
                    Debug.LogError($"Failed to load file at {path}");
                    stream.Close();
                    return null;
                }
            }
            else {
                Debug.LogWarning($"Attempt to load file \"{path}\" which is not exist.");
                return null;
            }

        }


        static BinaryFormatter GetBinaryFormatter () {

            BinaryFormatter formatter = new BinaryFormatter();
            SurrogateSelector selector = new SurrogateSelector();

            selector.AddSurrogate(typeof(Vector2),    new StreamingContext(StreamingContextStates.All), new Vector2SerializationSurrogate());
            selector.AddSurrogate(typeof(Vector3),    new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogate());
            selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new QuaternionSerializationSurrogate());

            formatter.SurrogateSelector = selector;

            return formatter;
        }

    }
}
