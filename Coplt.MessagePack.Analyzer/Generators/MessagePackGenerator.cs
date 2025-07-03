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
    public static string? GetConverter(ITypeSymbol symbol, bool AsBytes = false)
    {
        switch (symbol.SpecialType)
        {
            case SpecialType.System_Object:
                if (symbol.NullableAnnotation is NullableAnnotation.Annotated)
                    return "global::Coplt.MessagePack.Converters.NullableClassConverter<object, global::Coplt.MessagePack.Converters.EmptyObjectConverter>";
                return "global::Coplt.MessagePack.Converters.EmptyObjectConverter";
            case SpecialType.System_Enum:
            {
                var t = (INamedTypeSymbol)symbol;
                var underlying = t.EnumUnderlyingType!;
                var underlying_converter = GetConverter(underlying)!;
                var underlying_name = underlying.ToDisplayString();
                var name = symbol.ToDisplayString();
                return $"global::Coplt.MessagePack.Converters.EnumConverter<{name}, {underlying_name}, {underlying_converter}>";
            }
            case SpecialType.System_Boolean:
                return "global::Coplt.MessagePack.Converters.BooleanConvert";
            case SpecialType.System_Char:
                return "global::Coplt.MessagePack.Converters.CharConvert";
            case SpecialType.System_SByte:
                return "global::Coplt.MessagePack.Converters.SByteConvert";
            case SpecialType.System_Byte:
                return "global::Coplt.MessagePack.Converters.ByteConvert";
            case SpecialType.System_Int16:
                return "global::Coplt.MessagePack.Converters.Int16Convert";
            case SpecialType.System_UInt16:
                return "global::Coplt.MessagePack.Converters.UInt16Convert";
            case SpecialType.System_Int32:
                return "global::Coplt.MessagePack.Converters.Int32Convert";
            case SpecialType.System_UInt32:
                return "global::Coplt.MessagePack.Converters.UInt32Convert";
            case SpecialType.System_Int64:
                return "global::Coplt.MessagePack.Converters.Int64Convert";
            case SpecialType.System_UInt64:
                return "global::Coplt.MessagePack.Converters.UInt64Convert";
            case SpecialType.System_Decimal:
                return "global::Coplt.MessagePack.Converters.DecimalConvert";
            case SpecialType.System_Single:
                return "global::Coplt.MessagePack.Converters.SingleConvert";
            case SpecialType.System_Double:
                return "global::Coplt.MessagePack.Converters.DoubleConvert";
            case SpecialType.System_String:
                return "global::Coplt.MessagePack.Converters.StringConvert";
            case SpecialType.System_IntPtr:
                return "global::Coplt.MessagePack.Converters.IntPtrConvert";
            case SpecialType.System_UIntPtr:
                return "global::Coplt.MessagePack.Converters.UIntPtrConvert";
            case SpecialType.System_Array:
                break;
            case SpecialType.System_Collections_IEnumerable:
                break;
            case SpecialType.System_Collections_Generic_IEnumerable_T:
                break;
            case SpecialType.System_Collections_Generic_IList_T:
                break;
            case SpecialType.System_Collections_Generic_ICollection_T:
                break;
            case SpecialType.System_Collections_IEnumerator:
                break;
            case SpecialType.System_Collections_Generic_IEnumerator_T:
                break;
            case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                break;
            case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                break;
            case SpecialType.System_Nullable_T:
                break;
            case SpecialType.System_DateTime:
                return "global::Coplt.MessagePack.Converters.DateTimeConvert";
            default:
                break;
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
