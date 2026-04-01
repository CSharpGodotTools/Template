using PacketGen.Generators.PacketGeneration;
using System;
using System.Collections.Generic;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Shared helpers for emitting counted collection loops in read/write code generation.
/// </summary>
internal static class CollectionLoopEmitter
{
    /// <summary>
    /// Emits a counted write loop with optional root-level region markers.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">Collection expression being serialized.</param>
    /// <param name="countExpression">Expression yielding collection count.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="emitLoopBody">Callback that emits loop body lines.</param>
    public static void EmitWriteLoop(
        WriteContext ctx,
        string valueExpression,
        string countExpression,
        string indent,
        int depth,
        Action<string, string> emitLoopBody)
    {
        // Add region markers only for top-level write loops.
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {valueExpression}");

        string loopIndex = $"i{depth}";

        ctx.Shared.OutputLines.Add($"{indent}writer.Write({countExpression});");
        ctx.Shared.OutputLines.Add("");
        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countExpression}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        emitLoopBody(loopIndex, indent + "    ");

        ctx.Shared.OutputLines.Add($"{indent}}}");

        // Close region markers only for top-level write loops.
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }

    /// <summary>
    /// Builds consistent local variable names for collection read loops.
    /// </summary>
    /// <param name="nameSeed">Name seed derived from target expression.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <returns>Tuple containing count, index, and element variable names.</returns>
    public static (string CountVar, string LoopIndex, string ElementVar) BuildReadLoopNames(string nameSeed, int depth)
    {
        string countVar = TypeHandlerNameHelper.BuildName(nameSeed, "Count", depth);
        string loopIndex = TypeHandlerNameHelper.BuildName(nameSeed, "Index", depth);
        string elementVar = TypeHandlerNameHelper.BuildName(nameSeed, "Element", depth);

        return (countVar, loopIndex, elementVar);
    }

    /// <summary>
    /// Emits a counted read loop with optional root-level region markers.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="loopIndex">Loop-index variable name.</param>
    /// <param name="countVar">Count variable name.</param>
    /// <param name="emitLoopBody">Callback that emits loop body lines.</param>
    public static void EmitReadLoop(
        ReadContext ctx,
        string indent,
        int depth,
        string loopIndex,
        string countVar,
        Action<string, string> emitLoopBody)
    {
        // Add region markers only for top-level read loops.
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {ctx.TargetExpression}");

        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countVar}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        emitLoopBody(loopIndex, indent + "    ");

        ctx.Shared.OutputLines.Add($"{indent}}}");

        // Close region markers only for top-level read loops.
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }
}
