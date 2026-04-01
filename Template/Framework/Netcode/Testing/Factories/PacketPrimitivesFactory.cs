namespace Template.Setup.Testing;

/// <summary>
/// Produces primitive-value packet fixtures for packet round-trip tests.
/// </summary>
public static class PacketPrimitivesFactory
{
    /// <summary>
    /// Creates a baseline primitive packet with typical values.
    /// </summary>
    /// <returns>Packet containing a standard primitive value set.</returns>
    public static CPacketPrimitives CreateSample()
    {
        return new CPacketPrimitives
        {
            BoolValue = true,
            ByteValue = 1,
            SByteValue = -1,
            ShortValue = -10,
            UShortValue = 10,
            IntValue = -100,
            UIntValue = 100u,
            LongValue = -1000L,
            ULongValue = 1000UL,
            FloatValue = 1.25f,
            DoubleValue = 2.5,
            DecimalValue = 3.75m,
            CharValue = 'A',
            StringValue = "Alpha"
        };
    }

    /// <summary>
    /// Creates a second primitive packet with distinct values for deep comparisons.
    /// </summary>
    /// <returns>Packet containing an alternate primitive value set.</returns>
    public static CPacketPrimitives CreateDeepSample()
    {
        return new CPacketPrimitives
        {
            BoolValue = false,
            ByteValue = 200,
            SByteValue = 5,
            ShortValue = 1234,
            UShortValue = 5678,
            IntValue = 123456,
            UIntValue = 654321u,
            LongValue = 123456789L,
            ULongValue = 987654321UL,
            FloatValue = 9.5f,
            DoubleValue = 10.25,
            DecimalValue = 42.42m,
            CharValue = 'Z',
            StringValue = "Omega"
        };
    }
}
