#if DEBUG
using Godot;
using System;
using System.IO;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static class VisualUiResources
{
    public static readonly Texture2D EyeOpen = LoadEmbeddedTexture("Visualize.EyeOpen.png");
    public static readonly Texture2D EyeClosed = LoadEmbeddedTexture("Visualize.EyeClosed.png");
    public static readonly Texture2D Wrench = LoadEmbeddedTexture("Visualize.Wrench.png");

    public static readonly Color MutableMembersColor = new(0.8f, 1, 0.8f);
    public static readonly Color ReadonlyMembersColor = new(1.0f, 0.75f, 0.8f);

    /// <summary>
    /// Loads a PNG image embedded in the assembly as a resource and converts it into an ImageTexture.
    /// The <paramref name="resourceName"/> must be the fully qualified path including the namespace
    /// and folders, for example "Visualize.Icons.EyeOpen.png".
    /// </summary>
    private static ImageTexture LoadEmbeddedTexture(string resourceName)
    {
        Assembly assembly = typeof(VisualUiResources).Assembly;

        using Stream stream = assembly.GetManifestResourceStream(resourceName) ??
            throw new InvalidOperationException($"Embedded resource not found: {resourceName}");

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        byte[] bytes = ms.ToArray();

        Image image = new();
        image.LoadPngFromBuffer(bytes);

        return ImageTexture.CreateFromImage(image);
    }
}
#endif
