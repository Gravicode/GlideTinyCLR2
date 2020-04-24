namespace System.Runtime.CompilerServices {
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class InternalsVisibleToAttribute : Attribute {
        private string _assemblyName;

        public InternalsVisibleToAttribute(string assemblyName) => this._assemblyName = assemblyName;

        public string AssemblyName => this._assemblyName;

    }
}


