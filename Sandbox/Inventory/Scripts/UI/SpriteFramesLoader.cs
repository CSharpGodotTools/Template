using Godot;
using System.Collections.Generic;

namespace Template.Inventory;

public class SpriteFramesLoader
{
    private static Dictionary<string, SpriteFrames> _spriteFramesCache = [];

    public static SpriteFrames Load(string resourcePath)
    {
        if (!_spriteFramesCache.TryGetValue(resourcePath, out SpriteFrames spriteFrames))
        {
            spriteFrames = new SpriteFrames();

            if (resourcePath.EndsWith(".tres"))
            {
                spriteFrames = GD.Load<SpriteFrames>(resourcePath);
            }
            else
            {
                spriteFrames.AddFrame("default", GD.Load<CompressedTexture2D>(resourcePath));
            }

            _spriteFramesCache[resourcePath] = spriteFrames;
        }

        return spriteFrames;
    }
}
