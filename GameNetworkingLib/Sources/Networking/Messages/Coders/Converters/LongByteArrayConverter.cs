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
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
                _array[2] = _converter.byte2;
                _array[3] = _converter.byte3;
                _array[4] = _converter.byte4;
                _array[5] = _converter.byte5;
                _array[6] = _converter.byte6;
                _array[7] = _converter.byte7;
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
                _converter.byte4 = _array[4];
                _converter.byte5 = _array[5];
                _converter.byte6 = _array[6];
                _converter.byte7 = _array[7];
            }
        }

        public long value { get => _converter.value; set => _converter.value = value; }

        public LongByteArrayConverter(long value) {
            _array = new byte[sizeof(long)];
            _converter = new LongConverter();
            _converter.value = value;
        }
    }

    public struct ULongByteArrayConverter {
        private byte[] _array;
        private ULongConverter _converter;

        public byte[] array {
            get {
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
                _array[2] = _converter.byte2;
                _array[3] = _converter.byte3;
                _array[4] = _converter.byte4;
                _array[5] = _converter.byte5;
                _array[6] = _converter.byte6;
                _array[7] = _converter.byte7;
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
                _converter.byte4 = _array[4];
                _converter.byte5 = _array[5];
                _converter.byte6 = _array[6];
                _converter.byte7 = _array[7];
            }
        }

        public ulong value { get => _converter.value; set => _converter.value = value; }

        public ULongByteArrayConverter(ulong value) {
            _array = new byte[sizeof(ulong)];
            _converter = new ULongConverter();
            _converter.value = value;
        }
    }
}