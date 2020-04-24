namespace System.Runtime.CompilerServices {

    using System;

    /** This Enum matchs the miImpl flags defined in corhdr.h. It is used to specify
     *  certain method properties.
     */

    [Serializable]
    public enum MethodImplOptions {
        Unmanaged = System.Reflection.MethodImplAttributes.Unmanaged,
        ForwardRef = System.Reflection.MethodImplAttributes.ForwardRef,
        PreserveSig = System.Reflection.MethodImplAttributes.PreserveSig,
        InternalCall = System.Reflection.MethodImplAttributes.InternalCall,
        Synchronized = System.Reflection.MethodImplAttributes.Synchronized,
        NoInlining = System.Reflection.MethodImplAttributes.NoInlining,
    }

    [Serializable]
    public enum MethodCodeType {
        IL = System.Reflection.MethodImplAttributes.IL,
        Native = System.Reflection.MethodImplAttributes.Native,
        /// <internalonly/>
        OPTIL = System.Reflection.MethodImplAttributes.OPTIL,
        Runtime = System.Reflection.MethodImplAttributes.Runtime
    }

    /** Custom attribute to specify additional method properties.
     */
    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
    sealed public class MethodImplAttribute : Attribute {
        internal MethodImplOptions _val;
        public MethodCodeType MethodCodeType;

        public MethodImplAttribute(MethodImplOptions methodImplOptions) => this._val = methodImplOptions;

        public MethodImplAttribute(short value) => this._val = (MethodImplOptions)value;

        public MethodImplAttribute() {
        }

        public MethodImplOptions Value => this._val;
    }

}


