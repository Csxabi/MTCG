using System;

namespace MtcgServerTests
{
    internal static class Constants
    {
        // Demo User 1 is fixed (not changing)
        // Demo User 2 is variable (may be changed during testing)

        internal const string DemoUser1Name = "tester1";

        internal const string DemoUser1Pass = "tester1pw";

        internal static readonly Guid DemoUser1Id = Guid.Parse("00000000-0000-0000-0001-000000000000");

        internal static readonly Guid DemoCard1Id = Guid.Parse("00000000-0000-0000-0001-000000000001");

        internal const string DemoUser2Name = "tester2";

        internal const string DemoUser2Pass = "tester2pw";

        internal static readonly Guid DemoUser2Id = Guid.Parse("00000000-0000-0000-0002-000000000000");

        internal static readonly Guid DemoCard2Id = Guid.Parse("00000000-0000-0000-0002-000000000001");
    }
}
