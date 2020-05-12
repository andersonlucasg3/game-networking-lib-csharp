using System;
using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters {
    [StructLayout(LayoutKind.Explicit)]
    struct ShortConverter {
        [FieldOffset(0)] public short value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct UShortConverter {
        [FieldOffset(0)] public ushort value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
    }

    public struct ShortByteArrayConverter {
        private byte[] _array;
        private ShortConverter _converter;

        public byte[] array {
            get {
                if (BitConverter.IsLittleEndian) {
                    this._array[0] = this._converter.byte1;
                    this._array[1] = this._converter.byte0;
                } else {
                    this._array[0] = this._converter.byte0;
                    this._array[1] = this._converter.byte1;
                }
                return this._array;
            }

            set {
                this._array = value;
                if (BitConverter.IsLittleEndian) {
                    this._converter.byte0 = this._array[1];
                    this._converter.byte1 = this._array[0];
                } else {
                    this._converter.byte0 = this._array[0];
                    this._converter.byte1 = this._array[1];
                }
            }
        }

        public short value { get => this._converter.value; set => this._converter.value = value; }

        public ShortByteArrayConverter(short value) {
            this._array = new byte[sizeof(short)];
            this._converter = new ShortConverter { value = value };
        }
    }

    public struct UShortByteArrayConverter {
        private byte[] _array;
        private UShortConverter _converter;

        public byte[] array {
            get {
                if (BitConverter.IsLittleEndian) {
                    this._array[0] = this._converter.byte1;
                    this._array[1] = this._converter.byte0;
                } else {
                    this._array[0] = this._converter.byte0;
                    this._array[1] = this._converter.byte1;
                }
                return this._array;
            }

            set {
                this._array = value;
                if (BitConverter.IsLittleEndian) {
                    this._converter.byte0 = this._array[1];
                    this._converter.byte1 = this._array[0];
                } else {
                    this._converter.byte0 = this._array[0];
                    this._converter.byte1 = this._array[1];
                }
            }
        }

        public ushort value { get => this._converter.value; set => this._converter.value = value; }

        public UShortByteArrayConverter(ushort value) {
            this._array = new byte[sizeof(ushort)];
            this._converter = new UShortConverter { value = value };
        }
    }
}