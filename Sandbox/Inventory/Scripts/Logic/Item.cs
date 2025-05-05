using Godot;
using System;

namespace Template.Inventory;

public class Item
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Resource { get; private set; }
    public Color Color { get; private set; }

    public Item(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Item(Item other)
    {
        Name = other.Name;
    }

    public Item SetDescription(string description)
    {
        Description = description;
        return this;
    }

    public Item SetResource(string resource)
    {
        Resource = resource;
        return this;
    }

    public Item SetColor(Color color)
    {
        Color = color;
        return this;
    }

    public bool Equals(Item other)
    {
        if (other == null)
        {
            return false;
        }

        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return Equals((Item)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}
