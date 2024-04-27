using System;
using System.Collections.Generic;

using UnityEngine;

namespace DoubleHeat {

    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

        public enum KeepRule {
            KeepOld,
            KeepNew
        }

        public static T current = null;


        [Header("Singleton Options")]
        public KeepRule keepRule = KeepRule.KeepOld;


        protected virtual void Awake () {

            T thisInstance = GetComponent<T>();

            if (current == null) {
                current = thisInstance;
            }
            else if (current != thisInstance) {
                if (keepRule == KeepRule.KeepOld) {

                    if (this.gameObject != null)
                        Destroy(this.gameObject);

                }
                else if (keepRule == KeepRule.KeepNew) {

                    if (current.gameObject != null)
                        Destroy(current.gameObject);

                    current = thisInstance;
                }
            }

        }

        protected virtual void OnDestroy () {
            if (current == GetComponent<T>())
                current = null;
        }

    }
}
