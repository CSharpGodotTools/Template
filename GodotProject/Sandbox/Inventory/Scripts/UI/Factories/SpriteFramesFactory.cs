﻿using Godot;
using System;

namespace Template.Inventory;

public class SpriteFramesFactory : IResourceFactory
{
    public InventoryItemSprite CreateSprite(Resource resource, InventoryItemContainer itemContainer)
    {
        if (resource is SpriteFrames spriteFrames)
        {
            return new InventoryItemSprite(spriteFrames, itemContainer);
        }

        throw new ArgumentException("Resource is not a SpriteFrames instance.");
    }
}
