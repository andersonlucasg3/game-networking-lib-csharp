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
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
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
            }
        }

        public short value { get => _converter.value; set => _converter.value = value; }

        public ShortByteArrayConverter(short value) {
            _array = new byte[sizeof(short)];
            _converter = new ShortConverter { value = value };
        }
    }

    public struct UShortByteArrayConverter {
        private byte[] _array;
        private UShortConverter _converter;

        public byte[] array {
            get {
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
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
            }
        }

        public ushort value { get => _converter.value; set => _converter.value = value; }

        public UShortByteArrayConverter(ushort value) {
            _array = new byte[sizeof(ushort)];
            _converter = new UShortConverter { value = value };
        }
    }
}