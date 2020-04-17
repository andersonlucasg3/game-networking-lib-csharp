using System;
using System.Collections.Generic;
using System.Text;

namespace Messages.Coders {
    internal static class CoderHelper {
        internal static byte[] delimiter = Encoding.UTF8.GetBytes("\r\r\r");
        internal static string typePrefix = "com.medievalgame";

        internal static void InsertDelimiter(ref List<byte> buffer) {
            buffer.AddRange(delimiter);
        }

        internal static int CheckForDelimiter(byte[] buffer) {
            return ArraySearch.IndexOf(buffer, delimiter);
        }

        internal static byte[] PackageBytes(int size, byte[] buffer) {
            byte[] bytes = new byte[size];
            Array.Copy(buffer, bytes, size);
            return bytes;
        }

        internal static void SliceBuffer(int delimiterIndex, ref List<byte> buffer) {
            buffer.RemoveRange(0, delimiterIndex + 3);
        }

        internal static void WriteHeader(int type, ref List<byte> buffer) {
            buffer.AddRange(BitConverter.GetBytes(type));
        }

        internal static int ReadHeader(byte[] buffer) {
            byte[] headerBytes = new byte[sizeof(int)];
            Array.Copy(buffer, headerBytes, sizeof(int));
            return BitConverter.ToInt32(headerBytes, 0);
        }

        internal static bool IsType(int type, byte[] buffer) {
            return type == ReadHeader(buffer);
        }
    }

    internal static class ArraySearch {
        internal static bool ContentEquals(byte[] one, byte[] other) {
            if (one == null || other == null) {
                return false;
            }
            if (one.Length != other.Length) {
                return false;
            }

            for (var i = 0; i < one.Length; i++) {
                if (one[i] != other[i]) {
                    return false;
                }
            }

            return true;
        }

        private class PartialMatch {
            public int Index { get; private set; }
            public int MatchLength { get; set; }

            public PartialMatch(int index) {
                Index = index;
                MatchLength = 1;
            }
        }

        internal static int IndexOf(byte[] arrayToSearch, byte[] patternToFind) {
            if (patternToFind.Length == 0
              || arrayToSearch.Length == 0
              || arrayToSearch.Length < patternToFind.Length)
                return -1;

            List<PartialMatch> partialMatches = new List<PartialMatch>();

            for (int i = 0; i < arrayToSearch.Length; i++) {
                for (int j = partialMatches.Count - 1; j >= 0; j--)
                    if (arrayToSearch[i] == patternToFind[partialMatches[j].MatchLength]) {
                        partialMatches[j].MatchLength++;

                        if (partialMatches[j].MatchLength == patternToFind.Length)
                            return partialMatches[j].Index;
                    } else
                        partialMatches.Remove(partialMatches[j]);

                if (arrayToSearch[i] == patternToFind[0]) {
                    if (patternToFind.Length == 1) {
                        return i;
                    }
                    partialMatches.Add(new PartialMatch(i));
                }
            }

            return -1;
        }
    }
}
