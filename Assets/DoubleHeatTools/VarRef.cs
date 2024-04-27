using System;

namespace DoubleHeat {

    public sealed class VarRef<T> {
        public Func<T> Get {get; private set;}
        public Action<T> Set {get; private set;}

        public VarRef (Func<T> getter, Action<T> setter) {
            Get = getter;
            Set = setter;
        }
    }
}
