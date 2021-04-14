using System;
using System.Text;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Coders
{
    public static class CoderHelper
    {
        private static readonly ObjectPool<IntByteArrayConverter> _intConverterPool
            = new ObjectPool<IntByteArrayConverter>(() => new IntByteArrayConverter(0));

        public static readonly byte[] delimiter = Encoding.ASCII.GetBytes("\r\t\r\t\r");

        public static int InsertDelimiter(byte[] buffer, int index)
        {
            Array.Copy(delimiter, 0, buffer, index, delimiter.Length);
            return delimiter.Length;
        }

        public static int CheckForDelimiter(byte[] buffer, int length)
        {
            return ArraySearch.IndexOf(delimiter, delimiter.Length, buffer, length);
        }

        public static void PackageBytes(int size, byte[] buffer, byte[] packetBytes)
        {
            Array.Copy(buffer, packetBytes, size);
        }

        public static int SliceBuffer(int delimiterIndex, byte[] buffer, int count)
        {
            var delimiterEndIndex = delimiterIndex + delimiter.Length;
            var newLength = count - delimiterEndIndex;
            if (newLength > 0) Array.Copy(buffer, delimiterEndIndex, buffer, 0, newLength);
            return newLength;
        }

        public static int WriteInt(int value, byte[] buffer, int index)
        {
            var headerSize = sizeof(int);
            var converter = _intConverterPool.Rent();
            converter.value = value;
            Array.Copy(converter.array, 0, buffer, index, headerSize);
            _intConverterPool.Pay(converter);
            return headerSize;
        }

        public static int WriteByte(byte value, byte[] buffer, int index)
        {
            buffer[index] = value;
            return 1;
        }

        public static int AddChecksum(byte[] data, int index, int length)
        {
            var hash = CalculateChecksum(data, index, length);
            data[length] = hash;
            return 1;
        }

        public static byte CalculateChecksum(byte[] data, int index, int length)
        {
            byte checksum = 0;
            unchecked
            {
                for (var idx = index; idx < length; idx++) checksum += data[idx];
            }

            return checksum;
        }
    }

    public static class ArraySearch
    {
        public static int IndexOf(byte[] pattern, int patternLength, byte[] inArray, int arrayLength)
        {
            if (patternLength == 0 || arrayLength == 0 || arrayLength < patternLength) return -1;

            var missmatchInPattern = false;
            for (var inArrayIndex = 0; inArrayIndex < arrayLength; inArrayIndex++)
            {
                int patternIndex;
                var currentArrayIndex = inArrayIndex;
                for (patternIndex = 0; patternIndex < patternLength && currentArrayIndex < arrayLength; patternIndex++, currentArrayIndex++)
                {
                    missmatchInPattern = inArray[currentArrayIndex] != pattern[patternIndex];
                    if (missmatchInPattern) break;
                }

                if (missmatchInPattern) continue;

                if (patternIndex == patternLength && !missmatchInPattern) return inArrayIndex;
            }

            return -1;
        }
    }
}