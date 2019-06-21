namespace Tests {
    using Protobuf;

    class Program {
        static void Main(string[] args) {
            CodersTests tests = new CodersTests();
            tests.Setup();
            tests.TestDecoder();
            tests.TestEncoder();
            tests.TestEncoderDecoder();
        } 
    }
}