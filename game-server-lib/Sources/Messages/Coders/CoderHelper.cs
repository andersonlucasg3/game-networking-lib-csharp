using System;
using System.Text;
using System.Collections.Generic;

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

        internal static byte[] PackageBytes(int size, List<byte> buffer) {
            return buffer.GetRange(0, size).ToArray();
        }

        internal static void SliceBuffer(int delimiterIndex, ref List<byte> buffer) {
            buffer.RemoveRange(0, delimiterIndex + 3);
        }

        internal static void WriteHeader(Type type, ref List<byte> buffer) {
            buffer.AddRange(BitConverter.GetBytes(type.GetHashCode()));
        }

        internal static bool IsType(Type type, List<byte> buffer) {
            byte[] typeBytes = buffer.GetRange(0, sizeof(int)).ToArray();
            return typeBytes.Equals(BitConverter.GetBytes(type.GetHashCode()));
        }
    }

    internal static class ArraySearch {
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
                    if (patternToFind.Length == 1)
                        return i;
                    else
                        partialMatches.Add(new PartialMatch(i));
                }
            }

            return -1;
        }
    }
}