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
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
                _array[2] = _converter.byte2;
                _array[3] = _converter.byte3;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(_array);
                }
                return _array;
            }

            set {
                _array = value;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(_array);
                }
                _converter.byte0 = _array[0];
                _converter.byte1 = _array[1];
                _converter.byte2 = _array[2];
                _converter.byte3 = _array[3];
            }
        }

        public int value { get => _converter.value; set => _converter.value = value; }

        public IntByteArrayConverter(int value) {
            _array = new byte[sizeof(int)];
            _converter = new IntConverter {
                value = value
            };
        }
    }

    public struct UIntByteArrayConverter {
        private byte[] _array;
        private UIntConverter _converter;

        public byte[] array {
            get {
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
                _array[2] = _converter.byte2;
                _array[3] = _converter.byte3;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(_array);
                }
                return _array;
            }

            set {
                _array = value;
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(_array);
                }
                _converter.byte0 = _array[0];
                _converter.byte1 = _array[1];
                _converter.byte2 = _array[2];
                _converter.byte3 = _array[3];
            }
        }

        public uint value { get => _converter.value; set => _converter.value = value; }

        public UIntByteArrayConverter(uint value) {
            _array = new byte[sizeof(int)];
            _converter = new UIntConverter {
                value = value
            };
        }
    }
}