using System.Collections.Immutable;
using System.Text;
using Coplt.Analyzers.Utilities;
using Coplt.MessagePack.Analyzer.Generators.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coplt.MessagePack.Analyzer.Generators;

[Generator]
public class MessagePackGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );
    public static string? GetConverter(ITypeSymbol symbol, bool AsBytes = false, bool NullableClass = true)
    {
        if (symbol is INamedTypeSymbol and ({ IsUnboundGenericType: true } or { IsFileLocal: true })) return null;
        if (symbol is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated } && NullableClass)
        {
            var name = symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(TypeDisplayFormat);
            var converter = GetConverter(symbol, AsBytes, false);
            return $"global::Coplt.MessagePack.Converters.NullableClassConverter<{name}, {converter}>";
        }
        switch (symbol.SpecialType)
        {
            case SpecialType.System_Object:
            case SpecialType.System_Enum:
                if (symbol.NullableAnnotation is NullableAnnotation.Annotated)
                    return "global::Coplt.MessagePack.Converters.NullableClassConverter<object, global::Coplt.MessagePack.Converters.EmptyObjectConverter>";
                return "global::Coplt.MessagePack.Converters.EmptyObjectConverter";
            case SpecialType.System_Boolean:
                return "global::Coplt.MessagePack.Converters.BooleanConverter";
            case SpecialType.System_Char:
                return "global::Coplt.MessagePack.Converters.CharConverter";
            case SpecialType.System_SByte:
                return "global::Coplt.MessagePack.Converters.SByteConverter";
            case SpecialType.System_Byte:
                return "global::Coplt.MessagePack.Converters.ByteConverter";
            case SpecialType.System_Int16:
                return "global::Coplt.MessagePack.Converters.Int16Converter";
            case SpecialType.System_UInt16:
                return "global::Coplt.MessagePack.Converters.UInt16Converter";
            case SpecialType.System_Int32:
                return "global::Coplt.MessagePack.Converters.Int32Converter";
            case SpecialType.System_UInt32:
                return "global::Coplt.MessagePack.Converters.UInt32Converter";
            case SpecialType.System_Int64:
                return "global::Coplt.MessagePack.Converters.Int64Converter";
            case SpecialType.System_UInt64:
                return "global::Coplt.MessagePack.Converters.UInt64Converter";
            case SpecialType.System_Decimal:
                return "global::Coplt.MessagePack.Converters.DecimalConverter";
            case SpecialType.System_Single:
                return "global::Coplt.MessagePack.Converters.SingleConverter";
            case SpecialType.System_Double:
                return "global::Coplt.MessagePack.Converters.DoubleConverter";
            case SpecialType.System_String:
                return "global::Coplt.MessagePack.Converters.StringConverter";
            case SpecialType.System_IntPtr:
                return "global::Coplt.MessagePack.Converters.IntPtrConverter";
            case SpecialType.System_UIntPtr:
                return "global::Coplt.MessagePack.Converters.UIntPtrConverter";
            case SpecialType.System_Collections_Generic_IEnumerable_T:
            {
                var named = (INamedTypeSymbol)symbol;
                var underlying = named.TypeArguments[0];
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.IEnumerableConverter<{underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_Collections_Generic_IList_T:
            {
                var named = (INamedTypeSymbol)symbol;
                var underlying = named.TypeArguments[0];
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.IListConverter<{underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_Collections_Generic_ICollection_T:
            {
                var named = (INamedTypeSymbol)symbol;
                var underlying = named.TypeArguments[0];
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.ICollectionConverter<{underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_Collections_Generic_IReadOnlyList_T:
            {
                var named = (INamedTypeSymbol)symbol;
                var underlying = named.TypeArguments[0];
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.IReadOnlyListConverter<{underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
            {
                var named = (INamedTypeSymbol)symbol;
                var underlying = named.TypeArguments[0];
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.IReadOnlyCollectionConverter<{underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_Nullable_T:
            {
                var named = (INamedTypeSymbol)symbol;
                var underlying = named.TypeArguments[0];
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.NullableConverter<{underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_DateTime:
                return "global::Coplt.MessagePack.Converters.DateTimeConvert";
        }
        if (symbol is IArrayTypeSymbol array)
        {
            if (!array.IsSZArray) return null;
            var element = array.ElementType;
            if (AsBytes && element.SpecialType is SpecialType.System_Byte)
                return "global::Coplt.MessagePack.Converters.BytesArrayConverter";
            var element_converter = GetConverter(element);
            if (element_converter is null) return null;
            var element_name = element.ToDisplayString(TypeDisplayFormat);
            return $"global::Coplt.MessagePack.Converters.ArrayConverter<{element_name}, {element_converter}>";
        }
        else
        {
            if (symbol is not INamedTypeSymbol named) return null;
            if (named.EnumUnderlyingType is { } enum_underlying)
            {
                var underlying_converter = GetConverter(enum_underlying)!;
                var underlying_name = enum_underlying.ToDisplayString(TypeDisplayFormat);
                var name = symbol.ToDisplayString(TypeDisplayFormat);
                return $"global::Coplt.MessagePack.Converters.EnumConverter<{name}, {underlying_name}, {underlying_converter}>";
            }
            else if (
                named.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == "Coplt.MessagePack.MessagePackAttribute"
                    ) is { } mp_attr
            )
            {
                var format = mp_attr.NamedArguments
                    .FirstOrDefault(a => a.Key == "ConverterName") is { Value.Value: string n }
                    ? n
                    : "MessagePackConverter";
                var name = string.Format(format, symbol.Name);
                return $"{named.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(TypeDisplayFormat)}.{name}";
            }
            var type = named.IsGenericType
                ? named.ConstructUnboundGenericType().ToDisplayString(TypeDisplayFormat)
                : named.ToDisplayString(TypeDisplayFormat);
            if (type.StartsWith("global::System."))
            {
                var sub = type.AsSpan()["global::System.".Length..];
                if (sub.SequenceEqual("TimeSpan".AsSpan()))
                {
                    return $"global::Coplt.MessagePack.Converters.TimeSpanConverter";
                }
                else if (sub.SequenceEqual("DateTimeOffset".AsSpan()))
                {
                    return $"global::Coplt.MessagePack.Converters.DateTimeOffsetConverter";
                }
                else if (sub.StartsWith("Guid".AsSpan()))
                {
                    return $"global::Coplt.MessagePack.Converters.GuidConverter";
                }
                else if (sub.StartsWith("ValueTuple".AsSpan()))
                {
                    var last = sub["ValueTuple".Length..];
                    if (last.IsEmpty) return $"global::Coplt.MessagePack.Converters.ValueTupleConverter";
                    var converters = named.TypeArguments.Select(t => GetConverter(t))
                        .Where(a => a != null).ToList();
                    if (converters.Count != named.TypeArguments.Length) return null;
                    var types = string.Join(", ", named.TypeArguments.Select(t => t.ToDisplayString(TypeDisplayFormat)));
                    return $"global::Coplt.MessagePack.Converters.ValueTupleConverter<{types}, {string.Join(", ", converters)}>";
                }
                else if (sub.StartsWith("Tuple".AsSpan()))
                {
                    var last = sub["Tuple".Length..];
                    if (last.IsEmpty) return $"global::Coplt.MessagePack.Converters.TupleConverter";
                    var converters = named.TypeArguments.Select(t => GetConverter(t))
                        .Where(a => a != null).ToList();
                    if (converters.Count != named.TypeArguments.Length) return null;
                    var types = string.Join(", ", named.TypeArguments.Select(t => t.ToDisplayString(TypeDisplayFormat)));
                    return $"global::Coplt.MessagePack.Converters.TupleConverter<{types}, {string.Join(", ", converters)}>";
                }
                else if (sub.SequenceEqual("Collections.Generic.Dictionary<,>".AsSpan()))
                {
                    var key = named.TypeArguments[0];
                    var value = named.TypeArguments[0];
                    var key_converter = GetConverter(key);
                    if (key_converter is null) return null;
                    var value_converter = GetConverter(value);
                    if (value_converter is null) return null;
                    var key_name = key.ToDisplayString(TypeDisplayFormat);
                    var value_name = value.ToDisplayString(TypeDisplayFormat);
                    return $"global::Coplt.MessagePack.Converters.DictionaryConverter<{key_name}, {value_name}, {key_converter}, {value_converter}>";
                }
                else if (sub.SequenceEqual("Collections.Frozen.FrozenDictionary<,>".AsSpan()))
                {
                    var key = named.TypeArguments[0];
                    var value = named.TypeArguments[0];
                    var key_converter = GetConverter(key);
                    if (key_converter is null) return null;
                    var value_converter = GetConverter(value);
                    if (value_converter is null) return null;
                    var key_name = key.ToDisplayString(TypeDisplayFormat);
                    var value_name = value.ToDisplayString(TypeDisplayFormat);
                    return $"global::Coplt.MessagePack.Converters.FrozenDictionaryConverter<{key_name}, {value_name}, {key_converter}, {value_converter}>";
                }
            }
        }
        return null;
    }
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Coplt.MessagePack.MessagePackAttribute",
            static (syntax, _) => syntax is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
            static (ctx, _) =>
            {
                var diagnostics = new List<Diagnostic>();
                var compilation = ctx.SemanticModel.Compilation;
                var attr = ctx.Attributes.First();
                var syntax = (TypeDeclarationSyntax)ctx.TargetNode;
                var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                var GenBase = Utils.BuildGenBase(syntax, symbol, compilation);
                var Name = syntax.Identifier.ToString();
                var args = attr.NamedArguments.ToDictionary(a => a.Key, a => a.Value);
                var NameTemplate = "MessagePackConverter";
                if (args.TryGetValue("ConverterName", out var ConverterName))
                {
                    NameTemplate = $"{ConverterName.Value}";
                }
                var AsArray = !args.TryGetValue("AsArray", out var as_array) || as_array.Value is not false;
                var IncludeField = args.TryGetValue("IncludeField", out var include_field) && include_field.Value is true;
                var IncludePrivate = args.TryGetValue("IncludePrivate", out var include_private) && include_private.Value is true;
                var Fields = ImmutableArray.CreateBuilder<FieldInfo>();
                var field_index_inc = 0;
                foreach (var member in symbol.GetMembers())
                {
                    FieldInfo info = default;
                    ITypeSymbol? type;
                    AttributeData? attr_data;
                    if (member is IFieldSymbol field)
                    {
                        if (!IncludeField) continue;
                        if (!IncludePrivate && field.DeclaredAccessibility is not Accessibility.Public) continue;
                        info.Read = true;
                        info.Write = !field.IsReadOnly;
                        info.Name = field.Name;
                        type = field.Type;
                        attr_data = field.GetAttributes()
                            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.MessagePack.MessagePackAttribute");
                    }
                    else if (member is IPropertySymbol property)
                    {
                        info.Read = property.GetMethod is { } get && (get.DeclaredAccessibility is Accessibility.Public || IncludePrivate);
                        info.Write = property.SetMethod is { } set && (set.DeclaredAccessibility is Accessibility.Public || IncludePrivate);
                        if (info is { Read: false, Write: false }) continue;
                        info.Name = property.Name;
                        type = property.Type;
                        attr_data = property.GetAttributes()
                            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.MessagePack.MessagePackAttribute");
                    }
                    else continue;
                    var field_args = attr_data?.NamedArguments.ToDictionary(a => a.Key, a => a.Value) ?? [];
                    if (field_args.TryGetValue("Skip", out var skip) && skip.Value is true) continue;
                    var AsBytes = !field_args.TryGetValue("AsBytes", out var as_bytes) || as_bytes.Value is not false;
                    if (field_args.TryGetValue("Index", out var index) && index.Value is int index_value)
                    {
                        info.Index = index_value;
                        field_index_inc = index_value + 1;
                    }
                    else if (attr_data?.ConstructorArguments.FirstOrDefault() is { Value: int index_value_ })
                    {
                        info.Index = index_value_;
                        field_index_inc = index_value_ + 1;
                    }
                    else
                    {
                        info.Index = field_index_inc++;
                    }
                    var converter_type = field_args.TryGetValue("Converter", out var Converter) && Converter.Value is ITypeSymbol converter
                        ? converter.ToDisplayString(TypeDisplayFormat)
                        : GetConverter(type, AsBytes);
                    if (converter_type is null)
                    {
                        // error
                        continue;
                    }
                    info.ConverterType = converter_type;
                    info.Type = type.ToDisplayString(TypeDisplayFormat);
                    Fields.Add(info);
                }
                return (GenBase, Name, NameTemplate, AsArray, Fields.ToImmutable(), AlwaysEq.Create(diagnostics));
            });
        context.RegisterSourceOutput(sources, static (ctx, input) =>
        {
            var (GenBase, Name, NameTemplate, AsArray, Fields, Diagnostics) = input;
            if (Diagnostics.Value.Count > 0)
            {
                foreach (var diagnostic in Diagnostics.Value)
                {
                    ctx.ReportDiagnostic(diagnostic);
                }
            }
            var code = new TemplateMessagePackConverter(
                GenBase, Name, NameTemplate, AsArray, Fields
            ).Gen();
            var source_text = SourceText.From(code, Encoding.UTF8);
            var raw_source_file_name = GenBase.FileFullName;
            var sourceFileName = $"{raw_source_file_name}.MessagePackConverter.g.cs";
            ctx.AddSource(sourceFileName, source_text);
        });
    }
}
