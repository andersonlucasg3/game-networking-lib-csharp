using System.Text;
using System.Collections.Generic;

namespace MatchMaking.Protobuf.Coders {
    internal class CoderHelper {
        internal static byte[] delimiter = Encoding.UTF8.GetBytes("\r\r\r");
        internal static string typePrefix = "com.medievalgame";

        internal static void InsertDelimiter(ref List<byte> buffer) {
            buffer.AddRange(delimiter);
        }
    }
}