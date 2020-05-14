using System;
using System.Net;
using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters {
    [StructLayout(LayoutKind.Explicit)]
    struct IntConverter {
        [FieldOffset(0)] public int value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct UIntConverter {
        [FieldOffset(0)] public uint value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
    }

    public struct IntByteArrayConverter {
        private byte[] _array;
        private IntConverter _converter;

        public byte[] array {
            get {
                this._array[0] = this._converter.byte0;
                this._array[1] = this._converter.byte1;
                this._array[2] = this._converter.byte2;
                this._array[3] = this._converter.byte3;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
                return this._array;
            }

            set {
                this._array = value;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
                this._converter.byte0 = this._array[0];
                this._converter.byte1 = this._array[1];
                this._converter.byte2 = this._array[2];
                this._converter.byte3 = this._array[3];
            }
        }

        public int value { get => this._converter.value; set => this._converter.value = value; }

        public IntByteArrayConverter(int value) {
            this._array = new byte[sizeof(int)];
            this._converter = new IntConverter {
                value = value
            };
        }
    }

    public struct UIntByteArrayConverter {
        private byte[] _array;
        private UIntConverter _converter;

        public byte[] array {
            get {
                this._array[0] = this._converter.byte0;
                this._array[1] = this._converter.byte1;
                this._array[2] = this._converter.byte2;
                this._array[3] = this._converter.byte3;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
                return this._array;
            }

            set {
                this._array = value;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(this._array);
                }
                this._converter.byte0 = this._array[0];
                this._converter.byte1 = this._array[1];
                this._converter.byte2 = this._array[2];
                this._converter.byte3 = this._array[3];
            }
        }

        public uint value { get => this._converter.value; set => this._converter.value = value; }

        public UIntByteArrayConverter(uint value) {
            this._array = new byte[sizeof(int)];
            this._converter = new UIntConverter {
                value = value
            };
        }
    }
}