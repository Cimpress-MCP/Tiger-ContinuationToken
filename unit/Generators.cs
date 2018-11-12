using System;
using FsCheck;

namespace Test
{
    static class Generators
    {
        public static Arbitrary<byte[]> Salt { get; } =
            Gen.Sized(s => Gen.ArrayOf(Math.Max(s, 8), Arb.Generate<byte>())).ToArbitrary();
    }
}
