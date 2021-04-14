using System;
using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct FloatConverter
    {
        [FieldOffset(0)] public float value;
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
    }

    public struct FloatByteArrayConverter
    {
        private byte[] _array;
        private FloatConverter _converter;

        public byte[] array
        {
            get
            {
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
                _array[2] = _converter.byte2;
                _array[3] = _converter.byte3;
                if (!BitConverter.IsLittleEndian) Array.Reverse(_array);
                return _array;
            }

            set
            {
                _array = value;
                if (!BitConverter.IsLittleEndian) Array.Reverse(_array);
                _converter.byte0 = _array[0];
                _converter.byte1 = _array[1];
                _converter.byte2 = _array[2];
                _converter.byte3 = _array[3];
            }
        }

        public float value
        {
            get => _converter.value;
            set => _converter.value = value;
        }

        public FloatByteArrayConverter(float value)
        {
            _array = new byte[sizeof(float)];
            _converter = new FloatConverter
            {
                value = value
            };
        }
    }
}