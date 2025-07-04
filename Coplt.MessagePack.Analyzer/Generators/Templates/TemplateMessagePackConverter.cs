using System.Collections.Immutable;
using Coplt.Analyzers.Generators.Templates;

namespace Coplt.MessagePack.Analyzer.Generators.Templates;

public record struct FieldInfo(string Name, int Index, string Type, string ConverterType, bool Read, bool Write);

public class TemplateMessagePackConverter(
    GenBase GenBase,
    string Name,
    string NameTemplate,
    bool AsArray,
    ImmutableArray<FieldInfo> Fields
) : ATemplate(GenBase)
{
    public int MaxIndex = Fields.IsEmpty ? 0 : Fields.Max(a => a.Index);

    public string DataClassName = $"_{Name}__MessagePack_Converter_Data_";

    protected override void DoGenAfterUsing()
    {
        sb.AppendLine($"#pragma warning disable CS1998");
        sb.AppendLine();
    }

    protected override void DoGen()
    {
        var converter_name = string.Format(NameTemplate, Name);
        var converter_type_name = converter_name;

        sb.AppendLine("[global::Coplt.MessagePack.MessagePackConverterSource]");
        sb.AppendLine(GenBase.Target.Code);
        sb.AppendLine($"    : global::Coplt.MessagePack.IMessagePackConverterSource<{FullName}, {FullName}.{converter_type_name}>");
        sb.AppendLine($"    , global::Coplt.MessagePack.IMessagePackSerializable<{FullName}>");
        sb.AppendLine($"    , global::Coplt.MessagePack.IMessagePackAsyncSerializable<{FullName}>");
        sb.AppendLine($"    , global::Coplt.MessagePack.IMessagePackDeserializable<{FullName}>");
        sb.AppendLine($"    , global::Coplt.MessagePack.IMessagePackAsyncDeserializable<{FullName}>");
        sb.AppendLine("{");
        sb.AppendLine();

        #region IMessagePackSerializable

        sb.AppendLine(
            $"    static void global::Coplt.MessagePack.IMessagePackSerializable<{FullName}>.Serialize<TTarget>(ref global::Coplt.MessagePack.MessagePackWriter<TTarget> writer, {FullName} value, global::Coplt.MessagePack.MessagePackSerializerOptions options)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        {converter_type_name}.Write(ref writer, value, options);");
        sb.AppendLine($"    }}");
        sb.AppendLine();

        #endregion

        #region IMessagePackAsyncSerializable

        sb.AppendLine(
            $"    static global::System.Threading.Tasks.ValueTask global::Coplt.MessagePack.IMessagePackAsyncSerializable<{FullName}>.SerializeAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, {FullName} value, global::Coplt.MessagePack.MessagePackSerializerOptions options)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        return {converter_type_name}.WriteAsync(writer, value, options);");
        sb.AppendLine($"    }}");
        sb.AppendLine();

        #endregion

        #region IMessagePackDeserializable

        sb.AppendLine(
            $"    static {FullName} global::Coplt.MessagePack.IMessagePackDeserializable<{FullName}>.Deserialize<TSource>(ref global::Coplt.MessagePack.MessagePackReader<TSource> reader, global::Coplt.MessagePack.MessagePackSerializerOptions options)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        return {converter_type_name}.Read(ref reader, options);");
        sb.AppendLine($"    }}");
        sb.AppendLine();

        #endregion

        #region IMessagePackAsyncDeserializable

        sb.AppendLine(
            $"    static global::System.Threading.Tasks.ValueTask<{FullName}> global::Coplt.MessagePack.IMessagePackAsyncDeserializable<{FullName}>.DeserializeAsync<TSource>(global::Coplt.MessagePack.AsyncMessagePackReader<TSource> reader, global::Coplt.MessagePack.MessagePackSerializerOptions options)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        return {converter_type_name}.ReadAsync(reader, options);");
        sb.AppendLine($"    }}");
        sb.AppendLine();

        #endregion

        sb.AppendLine($"public readonly partial record struct {converter_type_name}");
        sb.AppendLine($"    : global::Coplt.MessagePack.IMessagePackConverter<{FullName}>");
        sb.AppendLine("{");

        #region Sync Write

        sb.AppendLine($"    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, {FullName} value, MessagePackSerializerOptions options)");
        sb.AppendLine("        where TTarget : IWriteTarget, allows ref struct");
        sb.AppendLine("    {");
        if (AsArray)
        {
            sb.AppendLine($"        if (options.StructMode is MessagePackStructMode.AsMap)");
            sb.AppendLine("        {");
            GenSyncWriteMap("            ");
            sb.AppendLine("        }");
            GenSyncWriteArray("        ");
        }
        else
        {
            sb.AppendLine($"        if (options.StructMode is MessagePackStructMode.AsArray)");
            sb.AppendLine("        {");
            GenSyncWriteArray("            ");
            sb.AppendLine("        }");
            GenSyncWriteMap("        ");
        }
        sb.AppendLine("    }");

        #endregion

        #region Sync Read

        sb.AppendLine($"    public static {FullName} Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)");
        sb.AppendLine("        where TSource : IReadSource, allows ref struct");
        sb.AppendLine("    {");
        sb.AppendLine("        var t = reader.PeekType();");
        sb.AppendLine(
            "        if (t is not (global::Coplt.MessagePack.MessagePackType.Array or global::Coplt.MessagePack.MessagePackType.Map)) throw new global::Coplt.MessagePack.MessagePackException(\"Expected array or map but not\");");
        if (AsArray)
        {
            sb.AppendLine("        if (t is global::Coplt.MessagePack.MessagePackType.Map)");
            sb.AppendLine("        {");
            GenSyncReadMap("            ");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            GenSyncReadArray("            ");
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine("        if (t is global::Coplt.MessagePack.MessagePackType.Array)");
            sb.AppendLine("        {");
            GenSyncReadArray("            ");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            GenSyncReadMap("            ");
            sb.AppendLine("        }");
        }
        sb.AppendLine("    }");

        #endregion

        #region Async Write

        sb.AppendLine(
            $"    public static async global::System.Threading.Tasks.ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, {FullName} value, MessagePackSerializerOptions options)");
        sb.AppendLine("        where TTarget : IAsyncWriteTarget");
        sb.AppendLine("    {");
        if (AsArray)
        {
            sb.AppendLine($"        if (options.StructMode is MessagePackStructMode.AsMap)");
            sb.AppendLine("        {");
            GenAsyncWriteMap("            ");
            sb.AppendLine("        }");
            GenAsyncWriteArray("        ");
        }
        else
        {
            sb.AppendLine($"        if (options.StructMode is MessagePackStructMode.AsArray)");
            sb.AppendLine("        {");
            GenAsyncWriteArray("            ");
            sb.AppendLine("        }");
            GenAsyncWriteMap("        ");
        }
        sb.AppendLine("    }");

        #endregion

        #region Async Read

        sb.AppendLine(
            $"    public static async global::System.Threading.Tasks.ValueTask<{FullName}> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)");
        sb.AppendLine("        where TSource : IAsyncReadSource");
        sb.AppendLine("    {");
        sb.AppendLine("        var t = await reader.PeekTypeAsync();");
        sb.AppendLine(
            "        if (t is not (global::Coplt.MessagePack.MessagePackType.Array or global::Coplt.MessagePack.MessagePackType.Map)) throw new global::Coplt.MessagePack.MessagePackException(\"Expected array or map but not\");");
        if (AsArray)
        {
            sb.AppendLine("        if (t is global::Coplt.MessagePack.MessagePackType.Map)");
            sb.AppendLine("        {");
            GenAsyncReadMap("            ");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            GenAsyncReadArray("            ");
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine("        if (t is global::Coplt.MessagePack.MessagePackType.Array)");
            sb.AppendLine("        {");
            GenAsyncReadArray("            ");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            GenAsyncReadMap("            ");
            sb.AppendLine("        }");
        }
        sb.AppendLine("    }");

        #endregion

        sb.AppendLine("}");

        sb.AppendLine();
        sb.AppendLine("}");
    }

    protected override void DoGenFileScope()
    {
        GenNameMapping();
    }

    private void GenNameMapping()
    {
        sb.AppendLine();
        sb.AppendLine($"file static class {DataClassName}");
        sb.AppendLine("{");
        sb.AppendLine(
            "    public static global::System.Collections.Frozen.FrozenDictionary<string, int> s_field_name_to_index = global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(new global::System.Collections.Generic.Dictionary<string, int>");
        sb.AppendLine("    {");
        foreach (var field in Fields)
        {
            sb.AppendLine($"        {{ \"{field.Name}\", {field.Index} }},");
        }
        sb.AppendLine("    });");
        sb.AppendLine("}");
    }

    #region SyncWrite

    private void GenSyncWriteArray(string tab)
    {
        sb.AppendLine($"{tab}writer.WriteArrayHead({Fields.Length});");
        sb.AppendLine($"{tab}for (var i = 0; i <= {MaxIndex}; i++)");
        sb.AppendLine($"{tab}{{");
        sb.AppendLine($"{tab}    switch (i)");
        sb.AppendLine($"{tab}    {{");
        foreach (var field in Fields)
        {
            if (!field.Read) continue;
            sb.AppendLine($"{tab}        case {field.Index}: {field.ConverterType}.Write(ref writer, value.{field.Name}, options); break;");
        }
        sb.AppendLine($"{tab}        default: writer.WriteNull(); break;");
        sb.AppendLine($"{tab}    }}");
        sb.AppendLine($"{tab}}}");
        sb.AppendLine($"{tab}return;");
    }

    private void GenSyncWriteMap(string tab)
    {
        sb.AppendLine($"{tab}writer.WriteMapHead({Fields.Length});");
        foreach (var field in Fields)
        {
            if (!field.Read) continue;
            sb.AppendLine($"{tab}writer.WriteString(\"{field.Name}\");");
            sb.AppendLine($"{tab}{field.ConverterType}.Write(ref writer, value.{field.Name}, options);");
        }
        sb.AppendLine($"{tab}return;");
    }

    #endregion

    #region AsyncWrite

    private void GenAsyncWriteArray(string tab)
    {
        sb.AppendLine($"{tab}await writer.WriteArrayHeadAsync({Fields.Length});");
        sb.AppendLine($"{tab}for (var i = 0; i <= {MaxIndex}; i++)");
        sb.AppendLine($"{tab}{{");
        sb.AppendLine($"{tab}    switch (i)");
        sb.AppendLine($"{tab}    {{");
        foreach (var field in Fields)
        {
            if (!field.Write) continue;
            sb.AppendLine($"{tab}        case {field.Index}: await {field.ConverterType}.WriteAsync(writer, value.{field.Name}, options); break;");
        }
        sb.AppendLine($"{tab}        default: await writer.WriteNullAsync(); break;");
        sb.AppendLine($"{tab}    }}");
        sb.AppendLine($"{tab}}}");
        sb.AppendLine($"{tab}return;");
    }

    private void GenAsyncWriteMap(string tab)
    {
        sb.AppendLine($"{tab}await writer.WriteMapHeadAsync({Fields.Length});");
        foreach (var field in Fields)
        {
            if (!field.Write) continue;
            sb.AppendLine($"{tab}await writer.WriteStringAsync(\"{field.Name}\");");
            sb.AppendLine($"{tab}await {field.ConverterType}.WriteAsync(writer, value.{field.Name}, options);");
        }
        sb.AppendLine($"{tab}return;");
    }

    #endregion

    #region SyncRead

    private void GenSyncReadArray(string tab)
    {
        sb.AppendLine($"{tab}var len = reader.ReadArrayHead() ?? throw new global::System.Diagnostics.UnreachableException();");
        sb.AppendLine($"{tab}var value = new {FullName}();");
        sb.AppendLine($"{tab}for (var i = 0; i < len; i++)");
        sb.AppendLine($"{tab}{{");
        sb.AppendLine($"{tab}    switch (i)");
        sb.AppendLine($"{tab}    {{");
        foreach (var field in Fields)
        {
            if (!field.Write) continue;
            sb.AppendLine($"{tab}        case {field.Index}: value.{field.Name} = {field.ConverterType}.Read(ref reader, options); break;");
        }
        sb.AppendLine($"{tab}        default: reader.SkipOnce(); break;");
        sb.AppendLine($"{tab}    }}");
        sb.AppendLine($"{tab}}}");
        sb.AppendLine($"{tab}return value;");
    }

    private void GenSyncReadMap(string tab)
    {
        sb.AppendLine($"{tab}var len = reader.ReadMapHead() ?? throw new global::System.Diagnostics.UnreachableException();");
        sb.AppendLine($"{tab}var value = new {FullName}();");
        sb.AppendLine($"{tab}for (var i = 0; i < len; i++)");
        sb.AppendLine($"{tab}{{");
        sb.AppendLine($"{tab}    var name = reader.ReadString() ?? throw new global::Coplt.MessagePack.MessagePackException(\"Excepted string but not\");");
        sb.AppendLine($"{tab}    if (!{DataClassName}.s_field_name_to_index.TryGetValue(name, out var index)) {{ reader.SkipOnce(); continue; }}");
        sb.AppendLine($"{tab}    switch (index)");
        sb.AppendLine($"{tab}    {{");
        foreach (var field in Fields)
        {
            if (!field.Write) continue;
            sb.AppendLine($"{tab}        case {field.Index}: value.{field.Name} = {field.ConverterType}.Read(ref reader, options); break;");
        }
        sb.AppendLine($"{tab}        default: reader.SkipOnce(); break;");
        sb.AppendLine($"{tab}    }}");
        sb.AppendLine($"{tab}}}");
        sb.AppendLine($"{tab}return value;");
    }

    #endregion

    #region AsyncRead

    private void GenAsyncReadArray(string tab)
    {
        sb.AppendLine($"{tab}var len = await reader.ReadArrayHeadAsync() ?? throw new global::System.Diagnostics.UnreachableException();");
        sb.AppendLine($"{tab}var value = new {FullName}();");
        sb.AppendLine($"{tab}for (var i = 0; i < len; i++)");
        sb.AppendLine($"{tab}{{");
        sb.AppendLine($"{tab}    switch (i)");
        sb.AppendLine($"{tab}    {{");
        foreach (var field in Fields)
        {
            if (!field.Write) continue;
            sb.AppendLine($"{tab}        case {field.Index}: value.{field.Name} = await {field.ConverterType}.ReadAsync(reader, options); break;");
        }
        sb.AppendLine($"{tab}        default: await reader.SkipOnceAsync(); break;");
        sb.AppendLine($"{tab}    }}");
        sb.AppendLine($"{tab}}}");
        sb.AppendLine($"{tab}return value;");
    }

    private void GenAsyncReadMap(string tab)
    {
        sb.AppendLine($"{tab}var len = await reader.ReadMapHeadAsync() ?? throw new global::System.Diagnostics.UnreachableException();");
        sb.AppendLine($"{tab}var value = new {FullName}();");
        sb.AppendLine($"{tab}for (var i = 0; i < len; i++)");
        sb.AppendLine($"{tab}{{");
        sb.AppendLine(
            $"{tab}    var name = await reader.ReadStringAsync() ?? throw new global::Coplt.MessagePack.MessagePackException(\"Excepted string but not\");");
        sb.AppendLine($"{tab}    if (!{DataClassName}.s_field_name_to_index.TryGetValue(name, out var index)) {{ await reader.SkipOnceAsync(); continue; }}");
        sb.AppendLine($"{tab}    switch (index)");
        sb.AppendLine($"{tab}    {{");
        foreach (var field in Fields)
        {
            if (!field.Write) continue;
            sb.AppendLine($"{tab}        case {field.Index}: value.{field.Name} = await {field.ConverterType}.ReadAsync(reader, options); break;");
        }
        sb.AppendLine($"{tab}        default: await reader.SkipOnceAsync(); break;");
        sb.AppendLine($"{tab}    }}");
        sb.AppendLine($"{tab}}}");
        sb.AppendLine($"{tab}return value;");
    }

    #endregion
}
