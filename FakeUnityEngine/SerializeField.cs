using System;

namespace UnityEngine {
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class SerializeFieldAttribute : System.Attribute {
        public SerializeFieldAttribute() { }
    }
}