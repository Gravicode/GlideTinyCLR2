namespace System.Reflection {
    using System;
    ////////////////////////////////////////////////////////////////////////////////
    //   Method is the class which represents a Method. These are accessed from
    //   Class through getMethods() or getMethod(). This class contains information
    //   about each method and also allows the method to be dynamically invoked
    //   on an instance.
    ////////////////////////////////////////////////////////////////////////////////
    [Serializable()]
    abstract public class MethodInfo : MethodBase {
        public override MemberTypes MemberType => System.Reflection.MemberTypes.Method;
        public abstract Type ReturnType {
            get;
        }
    }
}


