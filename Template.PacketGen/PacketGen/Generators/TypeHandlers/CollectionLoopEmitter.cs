using PacketGen.Generators.PacketGeneration;
using System;
using System.Collections.Generic;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Shared helpers for emitting counted collection loops in read/write code generation.
/// </summary>
internal static class CollectionLoopEmitter
{
    public static void EmitWriteLoop(
        WriteContext ctx,
        string valueExpression,
        string countExpression,
        string indent,
        int depth,
        Action<string, string> emitLoopBody)
    {
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {valueExpression}");

        string loopIndex = $"i{depth}";

        ctx.Shared.OutputLines.Add($"{indent}writer.Write({countExpression});");
        ctx.Shared.OutputLines.Add("");
        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countExpression}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        emitLoopBody(loopIndex, indent + "    ");

        ctx.Shared.OutputLines.Add($"{indent}}}");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }

    public static (string CountVar, string LoopIndex, string ElementVar) BuildReadLoopNames(string nameSeed, int depth)
    {
        string countVar = TypeHandlerNameHelper.BuildName(nameSeed, "Count", depth);
        string loopIndex = TypeHandlerNameHelper.BuildName(nameSeed, "Index", depth);
        string elementVar = TypeHandlerNameHelper.BuildName(nameSeed, "Element", depth);

        return (countVar, loopIndex, elementVar);
    }

    public static void EmitReadLoop(
        ReadContext ctx,
        string indent,
        int depth,
        string loopIndex,
        string countVar,
        Action<string, string> emitLoopBody)
    {
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {ctx.TargetExpression}");

        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countVar}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        emitLoopBody(loopIndex, indent + "    ");

        ctx.Shared.OutputLines.Add($"{indent}}}");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }
}
