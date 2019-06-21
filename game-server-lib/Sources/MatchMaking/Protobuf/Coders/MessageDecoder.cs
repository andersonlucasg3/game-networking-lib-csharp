using System.Collections.Generic;
using System.Collections;

namespace MatchMaking.Protobuf.Coders {
    using MatchMaking.Coders;
    using Models;

    public sealed class MessageDecoder: IMessageDecoder {
        private byte[] delimiter = CoderHelper.delimiter;

        private List<byte> buffer;

        public MessageDecoder() {
            this.buffer = new List<byte>();
        }

        public void Add(byte[] bytes) {
            this.buffer.AddRange(bytes);
        }

        public MessageContainer Decode() {
            int delimiterIndex = this.CheckForDelimiter(this.buffer.ToArray());
            if (delimiterIndex != -1) {
                byte[] bytes = this.PackageBytes(delimiterIndex);
                var package = MessagePackage.Parser.ParseFrom(bytes);
                this.SliceBuffer(delimiterIndex);
                return new MessageContainer(package);
            }
            return null;
        }

        private int CheckForDelimiter(byte[] buffer) {
            return ArraySearch.IndexOf(buffer, this.delimiter);
        }

        private byte[] PackageBytes(int size) {
            return this.buffer.GetRange(0, size).ToArray();
        }

        private void SliceBuffer(int delimiterIndex) {
            this.buffer.RemoveRange(0, delimiterIndex + 3);
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
