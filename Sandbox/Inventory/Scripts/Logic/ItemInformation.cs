using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Template.Inventory;

public static partial class ItemInformation
{
    private static readonly Dictionary<Material, Item> _items = [];

    static ItemInformation()
    {
        LoadJsonData();
    }

    public static Item Get(Material material)
    {
        if (_items.TryGetValue(material, out Item item))
        {
            return item;
        }

        throw new NotImplementedException(nameof(material));
    }

    private static void LoadJsonData()
    {
        string path = DirectoryUtils.FindFile("res://", "items.json");
        string jsonText = File.ReadAllText(path);

        // Parse the JSON data
        JsonDocument jsonDocument;

        try
        {
            jsonDocument = JsonDocument.Parse(jsonText);
        }
        catch (JsonException ex)
        {
            JsonExceptionHandler.Handle(ex, jsonText, path);
            return;
        }

        JsonElement root = jsonDocument.RootElement;

        // Iterate through the JSON properties and populate the _items dictionary
        foreach (JsonProperty property in root.EnumerateObject())
        {
            string itemName = property.Name;
            JsonElement itemData = property.Value;

            string description = GetPropertyValue(itemData, "Description", "");
            string resourcePath = GetPropertyValue(itemData, "Resource", "res://Template/Sprites/UI/icon.svg");
            string colorName = GetPropertyValue(itemData, "Color", "White");

            // Convert color name to a Color object
            Color color = new(colorName);

            // Create item and add it to the dictionary
            Item item = new Item(itemName)
                .SetDescription(description)
                .SetResource(resourcePath)
                .SetColor(color);

            // Convert item name to the material
            Material material = (Material)Enum.Parse(typeof(Material), itemName);

            _items[material] = item;
        }
    }

    private static string GetPropertyValue(JsonElement itemData, string propertyName, string defaultValue)
    {
        if (itemData.TryGetProperty(propertyName, out JsonElement propertyElement))
        {
            return propertyElement.GetString();
        }

        return defaultValue;
    }
}
