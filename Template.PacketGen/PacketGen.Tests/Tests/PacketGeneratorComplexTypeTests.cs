using Godot;
using System.Collections;

namespace PacketGen.Tests;

[TestFixture]
internal sealed class PacketGeneratorComplexTypeTests
{
    [Test]
    public void WriteRead_ClassStructAndNullableStruct_RoundTrip()
    {
        GeneratedAssemblyHarness harness = GeneratedAssemblyHarness.Build<PacketGenerator>(BuildComplexTypeSource(), "CPacketClassStruct.g.cs");
        Type packetType = harness.GetTypeOrFail("TestPackets.CPacketClassStruct");
        Type profileType = harness.GetTypeOrFail("TestPackets.PlayerProfile");
        Type entryType = harness.GetTypeOrFail("TestPackets.InventoryEntry");
        Type statsType = harness.GetTypeOrFail("TestPackets.PlayerStats");

        object entryA = CreateEntry(entryType, 11, 4);
        object entryB = CreateEntry(entryType, 22, 7);
        object stats = CreateStats(statsType, 70, new Vector2(10f, 15f));
        object primary = CreateProfile(profileType, entryType, stats, "Alpha", [entryA, entryB]);
        object secondary = CreateProfile(profileType, entryType, stats, "Beta", [entryB]);

        object packet = PacketReflectionHelper.CreatePacketInstance(packetType);
        PacketReflectionHelper.SetProperty(packet, "Primary", primary);
        PacketReflectionHelper.SetProperty(packet, "Secondary", null);
        PacketReflectionHelper.SetProperty(packet, "OptionalStats", CreateNullable(statsType, stats));
        PacketReflectionHelper.SetProperty(packet, "Profiles", CreateList(profileType, [primary, secondary, null]));
        PacketReflectionHelper.SetProperty(packet, "ProfileById", CreateDictionary(typeof(int), profileType, [1, primary, 2, secondary]));

        object writer = harness.CreateWriter();
        PacketReflectionHelper.InvokeWrite(packet, writer);

        object reader = harness.CreateReader(GeneratedAssemblyHarness.GetWriterValues(writer));
        object roundTripPacket = PacketReflectionHelper.CreatePacketInstance(packetType);
        PacketReflectionHelper.InvokeRead(roundTripPacket, reader);

        using (Assert.EnterMultipleScope())
        {
            DeepAssert.AreEqual(primary, PacketReflectionHelper.GetProperty(roundTripPacket, "Primary"), "Primary");
            DeepAssert.AreEqual(null, PacketReflectionHelper.GetProperty(roundTripPacket, "Secondary"), "Secondary");
            DeepAssert.AreEqual(CreateNullable(statsType, stats), PacketReflectionHelper.GetProperty(roundTripPacket, "OptionalStats"), "OptionalStats");
            DeepAssert.AreEqual(CreateList(profileType, [primary, secondary, null]), PacketReflectionHelper.GetProperty(roundTripPacket, "Profiles"), "Profiles");
            DeepAssert.AreEqual(CreateDictionary(typeof(int), profileType, [1, primary, 2, secondary]), PacketReflectionHelper.GetProperty(roundTripPacket, "ProfileById"), "ProfileById");
        }
    }

    [Test]
    public void GeneratedSource_ContainsComplexTypeSerializationMarkers()
    {
        GeneratedAssemblyHarness harness = GeneratedAssemblyHarness.Build<PacketGenerator>(BuildComplexTypeSource(), "CPacketClassStruct.g.cs");
        string source = harness.Result.GeneratedSource;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source, Does.Contain("writer.Write(Primary is not null);"));
            Assert.That(source, Does.Contain("writer.Write(OptionalStats.HasValue);"));
            Assert.That(source, Does.Contain("Primary = primaryValue0;"));
            Assert.That(source, Does.Contain("OptionalStats = optionalStatsValue0;"));
            Assert.That(source, Does.Contain("Profiles.Add(profilesElement0);"));
        }
    }

    /// <summary>
    /// Creates and initializes an inventory entry instance for complex-type packet tests.
    /// </summary>
    /// <param name="entryType">Generated entry CLR type.</param>
    /// <param name="itemId">Item identifier value.</param>
    /// <param name="quantity">Item quantity value.</param>
    /// <returns>Initialized entry instance.</returns>
    private static object CreateEntry(Type entryType, int itemId, int quantity)
    {
        object entry = Activator.CreateInstance(entryType)!;
        PacketReflectionHelper.SetProperty(entry, "ItemId", itemId);
        PacketReflectionHelper.SetProperty(entry, "Quantity", quantity);
        return entry;
    }

    /// <summary>
    /// Creates and initializes a player stats struct instance.
    /// </summary>
    /// <param name="statsType">Generated stats CLR type.</param>
    /// <param name="level">Level value.</param>
    /// <param name="spawn">Spawn position value.</param>
    /// <returns>Initialized stats instance.</returns>
    private static object CreateStats(Type statsType, int level, Vector2 spawn)
    {
        object stats = Activator.CreateInstance(statsType)!;
        PacketReflectionHelper.SetProperty(stats, "Level", level);
        PacketReflectionHelper.SetProperty(stats, "Spawn", spawn);
        return stats;
    }

    /// <summary>
    /// Creates and initializes a player profile object with nested collections.
    /// </summary>
    /// <param name="profileType">Generated profile CLR type.</param>
    /// <param name="entryType">Generated inventory entry CLR type.</param>
    /// <param name="stats">Stats object assigned to the profile.</param>
    /// <param name="name">Profile name.</param>
    /// <param name="entries">Inventory entry payload.</param>
    /// <returns>Initialized profile instance.</returns>
    private static object CreateProfile(Type profileType, Type entryType, object stats, string name, object[] entries)
    {
        object profile = Activator.CreateInstance(profileType)!;
        PacketReflectionHelper.SetProperty(profile, "Name", name);
        PacketReflectionHelper.SetProperty(profile, "Stats", stats);
        PacketReflectionHelper.SetProperty(profile, "Inventory", CreateArray(entryType, entries));
        PacketReflectionHelper.SetProperty(profile, "Backpack", CreateList(entryType, entries));
        PacketReflectionHelper.SetProperty(profile, "SlotMap", CreateDictionary(typeof(int), entryType, [1, entries[0], 2, entries[^1]]));
        return profile;
    }

    /// <summary>
    /// Creates an array of the specified element type and fills it with provided values.
    /// </summary>
    /// <param name="elementType">Array element CLR type.</param>
    /// <param name="values">Values to assign by index.</param>
    /// <returns>Populated array instance.</returns>
    private static Array CreateArray(Type elementType, object[] values)
    {
        Array array = Array.CreateInstance(elementType, values.Length);
        for (int i = 0; i < values.Length; i++)
            array.SetValue(values[i], i);

        return array;
    }

    /// <summary>
    /// Creates a strongly typed list instance and appends the provided values.
    /// </summary>
    /// <param name="elementType">List element CLR type.</param>
    /// <param name="values">Values to add to the list.</param>
    /// <returns>Populated list instance.</returns>
    private static object CreateList(Type elementType, object?[] values)
    {
        Type listType = typeof(List<>).MakeGenericType(elementType);
        IList list = (IList)Activator.CreateInstance(listType)!;

        foreach (object? value in values)
            list.Add(value);

        return list;
    }

    /// <summary>
    /// Creates a strongly typed dictionary and fills it from alternating key/value entries.
    /// </summary>
    /// <param name="keyType">Dictionary key CLR type.</param>
    /// <param name="valueType">Dictionary value CLR type.</param>
    /// <param name="entries">Alternating key/value sequence.</param>
    /// <returns>Populated dictionary instance.</returns>
    private static object CreateDictionary(Type keyType, Type valueType, object?[] entries)
    {
        Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        IDictionary dictionary = (IDictionary)Activator.CreateInstance(dictionaryType)!;

        for (int i = 0; i < entries.Length; i += 2)
        {
            object? key = entries[i];
            Assert.That(key, Is.Not.Null, "Dictionary keys must be non-null for generated packet tests.");
            dictionary.Add(key!, entries[i + 1]);
        }

        return dictionary;
    }

    /// <summary>
    /// Creates a boxed nullable value for the specified underlying struct type.
    /// </summary>
    /// <param name="valueType">Underlying value type of the nullable.</param>
    /// <param name="value">Value to wrap.</param>
    /// <returns>Boxed nullable instance.</returns>
    private static object CreateNullable(Type valueType, object value)
    {
        Type nullableType = typeof(Nullable<>).MakeGenericType(valueType);
        return Activator.CreateInstance(nullableType, value)!;
    }

    /// <summary>
    /// Builds packet source used for complex class, struct, nullable, and collection serialization tests.
    /// </summary>
    /// <returns>Packet source code string.</returns>
    private static string BuildComplexTypeSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Godot;
        using {{PacketGenTestConstants.PacketNamespace}};

        namespace TestPackets;

        public struct PlayerStats
        {
            public int Level { get; set; }
            public Vector2 Spawn { get; set; }
        }

        public class InventoryEntry
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; }
        }

        public class PlayerProfile
        {
            public string Name { get; set; }
            public PlayerStats Stats { get; set; }
            public InventoryEntry[] Inventory { get; set; }
            public List<InventoryEntry> Backpack { get; set; }
            public Dictionary<int, InventoryEntry> SlotMap { get; set; }
        }

        public partial class CPacketClassStruct : ClientPacket
        {
            public PlayerProfile Primary { get; set; }
            public PlayerProfile Secondary { get; set; }
            public PlayerStats? OptionalStats { get; set; }
            public List<PlayerProfile> Profiles { get; set; }
            public Dictionary<int, PlayerProfile> ProfileById { get; set; }
        }
        """;
    }
}
