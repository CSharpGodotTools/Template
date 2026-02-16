using Godot;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

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
        object primary = CreateProfile(profileType, entryType, stats, "Alpha", new object[] { entryA, entryB });
        object secondary = CreateProfile(profileType, entryType, stats, "Beta", new object[] { entryB });

        object packet = PacketReflectionHelper.CreatePacketInstance(packetType);
        PacketReflectionHelper.SetProperty(packet, "Primary", primary);
        PacketReflectionHelper.SetProperty(packet, "Secondary", null);
        PacketReflectionHelper.SetProperty(packet, "OptionalStats", CreateNullable(statsType, stats));
        PacketReflectionHelper.SetProperty(packet, "Profiles", CreateList(profileType, new object?[] { primary, secondary, null }));
        PacketReflectionHelper.SetProperty(packet, "ProfileById", CreateDictionary(typeof(int), profileType, new object?[] { 1, primary, 2, secondary }));

        object writer = harness.CreateWriter();
        PacketReflectionHelper.InvokeWrite(packet, writer);

        object reader = harness.CreateReader(harness.GetWriterValues(writer));
        object roundTripPacket = PacketReflectionHelper.CreatePacketInstance(packetType);
        PacketReflectionHelper.InvokeRead(roundTripPacket, reader);

        using (Assert.EnterMultipleScope())
        {
            DeepAssert.AreEqual(primary, PacketReflectionHelper.GetProperty(roundTripPacket, "Primary"), "Primary");
            DeepAssert.AreEqual(null, PacketReflectionHelper.GetProperty(roundTripPacket, "Secondary"), "Secondary");
            DeepAssert.AreEqual(CreateNullable(statsType, stats), PacketReflectionHelper.GetProperty(roundTripPacket, "OptionalStats"), "OptionalStats");
            DeepAssert.AreEqual(CreateList(profileType, new object?[] { primary, secondary, null }), PacketReflectionHelper.GetProperty(roundTripPacket, "Profiles"), "Profiles");
            DeepAssert.AreEqual(CreateDictionary(typeof(int), profileType, new object?[] { 1, primary, 2, secondary }), PacketReflectionHelper.GetProperty(roundTripPacket, "ProfileById"), "ProfileById");
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

    private static object CreateEntry(Type entryType, int itemId, int quantity)
    {
        object entry = Activator.CreateInstance(entryType)!;
        PacketReflectionHelper.SetProperty(entry, "ItemId", itemId);
        PacketReflectionHelper.SetProperty(entry, "Quantity", quantity);
        return entry;
    }

    private static object CreateStats(Type statsType, int level, Vector2 spawn)
    {
        object stats = Activator.CreateInstance(statsType)!;
        PacketReflectionHelper.SetProperty(stats, "Level", level);
        PacketReflectionHelper.SetProperty(stats, "Spawn", spawn);
        return stats;
    }

    private static object CreateProfile(Type profileType, Type entryType, object stats, string name, object[] entries)
    {
        object profile = Activator.CreateInstance(profileType)!;
        PacketReflectionHelper.SetProperty(profile, "Name", name);
        PacketReflectionHelper.SetProperty(profile, "Stats", stats);
        PacketReflectionHelper.SetProperty(profile, "Inventory", CreateArray(entryType, entries));
        PacketReflectionHelper.SetProperty(profile, "Backpack", CreateList(entryType, entries));
        PacketReflectionHelper.SetProperty(profile, "SlotMap", CreateDictionary(typeof(int), entryType, new object?[] { 1, entries[0], 2, entries[^1] }));
        return profile;
    }

    private static object CreateArray(Type elementType, object[] values)
    {
        Array array = Array.CreateInstance(elementType, values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            array.SetValue(values[i], i);
        }

        return array;
    }

    private static object CreateList(Type elementType, object?[] values)
    {
        Type listType = typeof(List<>).MakeGenericType(elementType);
        IList list = (IList)Activator.CreateInstance(listType)!;

        foreach (object? value in values)
        {
            list.Add(value);
        }

        return list;
    }

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

    private static object CreateNullable(Type valueType, object value)
    {
        Type nullableType = typeof(Nullable<>).MakeGenericType(valueType);
        return Activator.CreateInstance(nullableType, value)!;
    }

    private static string BuildComplexTypeSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Godot;
        using Framework.Netcode;

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
