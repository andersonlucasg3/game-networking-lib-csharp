using System;
using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters {
    [StructLayout(LayoutKind.Explicit)]
    struct LongConverter {
        [FieldOffset(0)] public long value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
        [FieldOffset(4)] public byte byte4;
        [FieldOffset(5)] public byte byte5;
        [FieldOffset(6)] public byte byte6;
        [FieldOffset(7)] public byte byte7;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct ULongConverter {
        [FieldOffset(0)] public ulong value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
        [FieldOffset(4)] public byte byte4;
        [FieldOffset(5)] public byte byte5;
        [FieldOffset(6)] public byte byte6;
        [FieldOffset(7)] public byte byte7;
    }

    public struct LongByteArrayConverter {
        private byte[] _array;
        private LongConverter _converter;

        public byte[] array {
            get {
                this._array[0] = this._converter.byte0;
                this._array[1] = this._converter.byte1;
                this._array[2] = this._converter.byte2;
                this._array[3] = this._converter.byte3;
                this._array[4] = this._converter.byte4;
                this._array[5] = this._converter.byte5;
                this._array[6] = this._converter.byte6;
                this._array[7] = this._converter.byte7;
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
                return this._array;
            }

            set {
                this._array = value;
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
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

        public long value { get => this._converter.value; set => this._converter.value = value; }

        public LongByteArrayConverter(long value) {
            this._array = new byte[sizeof(long)];
            this._converter = new LongConverter();
            this._converter.value = value;
        }
    }

    public struct ULongByteArrayConverter {
        private byte[] _array;
        private ULongConverter _converter;

        public byte[] array {
            get {
                this._array[0] = this._converter.byte0;
                this._array[1] = this._converter.byte1;
                this._array[2] = this._converter.byte2;
                this._array[3] = this._converter.byte3;
                this._array[4] = this._converter.byte4;
                this._array[5] = this._converter.byte5;
                this._array[6] = this._converter.byte6;
                this._array[7] = this._converter.byte7;
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
                return this._array;
            }

            set {
                this._array = value;
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
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

        public ulong value { get => this._converter.value; set => this._converter.value = value; }

        public ULongByteArrayConverter(ulong value) {
            this._array = new byte[sizeof(ulong)];
            this._converter = new ULongConverter();
            this._converter.value = value;
        }
    }
}