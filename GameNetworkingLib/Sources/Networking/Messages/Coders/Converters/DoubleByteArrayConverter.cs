using System;
using System.Runtime.InteropServices;

namespace GameNetworking.Messages.Coders.Converters
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct DoubleConverter
    {
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

    public struct DoubleByteArrayConverter
    {
        private byte[] _array;
        private DoubleConverter _converter;

        public byte[] array
        {
            get
            {
                _array[0] = _converter.byte0;
                _array[1] = _converter.byte1;
                _array[2] = _converter.byte2;
                _array[3] = _converter.byte3;
                _array[4] = _converter.byte4;
                _array[5] = _converter.byte5;
                _array[6] = _converter.byte6;
                _array[7] = _converter.byte7;
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
                _converter.byte4 = _array[4];
                _converter.byte5 = _array[5];
                _converter.byte6 = _array[6];
                _converter.byte7 = _array[7];
            }
        }

        public double value
        {
            get => _converter.value;
            set => _converter.value = value;
        }

        public DoubleByteArrayConverter(double value)
        {
            _array = new byte[sizeof(double)];
            _converter = new DoubleConverter();
            _converter.value = value;
        }
    }
}