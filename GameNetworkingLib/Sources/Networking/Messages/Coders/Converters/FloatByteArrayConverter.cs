using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters {
    [StructLayout(LayoutKind.Explicit)]
    struct FloatConverter {
        [FieldOffset(0)] public float value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
    }

    public struct FloatByteArrayConverter {
        private byte[] _array;
        private FloatConverter _converter;

        public byte[] array {
            get {
                this._array[0] = this._converter.byte0;
                this._array[1] = this._converter.byte1;
                this._array[2] = this._converter.byte2;
                this._array[3] = this._converter.byte3;
                return this._array;
            }

            set {
                this._array = value;
                this._converter.byte0 = this._array[0];
                this._converter.byte1 = this._array[1];
                this._converter.byte2 = this._array[2];
                this._converter.byte3 = this._array[3];
            }
        }

        public float value { get => this._converter.value; set => this._converter.value = value; }

        public FloatByteArrayConverter(float value) {
            this._array = new byte[sizeof(float)];
            this._converter = new FloatConverter();
            this._converter.value = value;
        }
    }
}