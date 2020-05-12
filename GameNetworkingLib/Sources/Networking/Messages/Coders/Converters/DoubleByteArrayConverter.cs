using System;
using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters {
    [StructLayout(LayoutKind.Explicit)]
    struct DoubleConverter {
        [FieldOffset(0)] public double value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
        [FieldOffset(4)] public byte byte4;
        [FieldOffset(5)] public byte byte5;
        [FieldOffset(6)] public byte byte6;
        [FieldOffset(7)] public byte byte7;
    }

    public struct DoubleByteArrayConverter {
        private byte[] _array;
        private DoubleConverter _converter;

        public byte[] array {
            get {
                if (BitConverter.IsLittleEndian) {
                    this._array[0] = this._converter.byte7;
                    this._array[1] = this._converter.byte6;
                    this._array[2] = this._converter.byte5;
                    this._array[3] = this._converter.byte4;
                    this._array[4] = this._converter.byte3;
                    this._array[5] = this._converter.byte2;
                    this._array[6] = this._converter.byte1;
                    this._array[7] = this._converter.byte0;
                } else {
                    this._array[0] = this._converter.byte0;
                    this._array[1] = this._converter.byte1;
                    this._array[2] = this._converter.byte2;
                    this._array[3] = this._converter.byte3;
                    this._array[4] = this._converter.byte4;
                    this._array[5] = this._converter.byte5;
                    this._array[6] = this._converter.byte6;
                    this._array[7] = this._converter.byte7;
                }
                return this._array;
            }

            set {
                this._array = value;
                if (BitConverter.IsLittleEndian) {
                    this._converter.byte0 = this._array[7];
                    this._converter.byte1 = this._array[6];
                    this._converter.byte2 = this._array[5];
                    this._converter.byte3 = this._array[4];
                    this._converter.byte4 = this._array[3];
                    this._converter.byte5 = this._array[2];
                    this._converter.byte6 = this._array[1];
                    this._converter.byte7 = this._array[0];
                } else {
                    this._converter.byte0 = this._array[0];
                    this._converter.byte1 = this._array[1];
                    this._converter.byte2 = this._array[2];
                    this._converter.byte3 = this._array[3];
                    this._converter.byte4 = this._array[4];
                    this._converter.byte5 = this._array[5];
                    this._converter.byte6 = this._array[6];
                    this._converter.byte7 = this._array[7];
                }
            }
        }

        public double value { get => this._converter.value; set => this._converter.value = value; }

        public DoubleByteArrayConverter(double value) {
            this._array = new byte[sizeof(double)];
            this._converter = new DoubleConverter();
            this._converter.value = value;
        }
    }
}