﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace BinaryFormat
{
    public partial class Generator : ISourceGenerator
    {
        private void GenerateReader(
            GeneratorExecutionContext context,
            TypeDeclarationSyntax typeDecl,
            SemanticModel semanticModel)
        {
            if (!typeDecl.Modifiers.Any(tok => tok.IsKind(SyntaxKind.PartialKeyword)))
            {
                // Type must be partial
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        "TypeMustBePartial",
                        category: "BinaryFormat",
                        $"Type {typeDecl.Identifier.ValueText} must be partial",
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0,
                        location: typeDecl.Identifier.GetLocation()));
                return;
            }

            var containerSymbol = semanticModel.GetDeclaredSymbol(typeDecl)!;
            ITypeSymbol receiverType = containerSymbol;

            bool hasBigEndianAttribute = containerSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "BigEndianAttribute");
            bool hasLittleEndianAttribute = containerSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "LittleEndianAttribute");

            var fieldsAndProps = receiverType.GetMembers()
                .Where(m => m is {
                    DeclaredAccessibility: Accessibility.Public,
                    Kind: SymbolKind.Field or SymbolKind.Property,
                })
                .Select(m => new DataMemberSymbol(m)).ToList();

            var stringBuilder = new StringBuilder();
            string classOrStruct = typeDecl is ClassDeclarationSyntax ? "class" : "struct";

            // FIXME: modifiers, class/struct/record
            stringBuilder.AppendLine($"using System.Buffers.Binary;");
            stringBuilder.AppendLine($"");
            stringBuilder.AppendLine($"namespace {containerSymbol.ContainingNamespace}");
            stringBuilder.AppendLine($"{{");
            stringBuilder.AppendLine($"    public partial {classOrStruct} {typeDecl.Identifier}");
            stringBuilder.AppendLine($"    {{");

            if (hasLittleEndianAttribute && !hasBigEndianAttribute)
            {
                GenerateReadMethod(context, typeDecl, semanticModel, stringBuilder, "", "LittleEndian", fieldsAndProps);
            }
            else if (hasBigEndianAttribute && !hasLittleEndianAttribute)
            {
                GenerateReadMethod(context, typeDecl, semanticModel, stringBuilder, "", "BigEndian", fieldsAndProps);
            }
            else
            {
                GenerateReadMethod(context, typeDecl, semanticModel, stringBuilder, "LittleEndian", "LittleEndian", fieldsAndProps);
                stringBuilder.AppendLine();
                GenerateReadMethod(context, typeDecl, semanticModel, stringBuilder, "BigEndian", "BigEndian", fieldsAndProps);
            }

            stringBuilder.AppendLine($"    }}");
            stringBuilder.AppendLine($"}}");

            context.AddSource($"{containerSymbol.Name}.Reader.Generated.cs", stringBuilder.ToString());
        }

        private void GenerateReadMethod(
            GeneratorExecutionContext context,
            TypeDeclarationSyntax typeDecl,
            SemanticModel semanticModel,
            StringBuilder stringBuilder,
            string nameSuffix,
            string endianSuffix,
            List<DataMemberSymbol> fieldsAndProps)
        {
            int offset = 0;
            StringBuilder variableOffset = new StringBuilder();
            int variableOffsetIndex = 1;

            stringBuilder.AppendLine($"        public static {typeDecl.Identifier} Read{nameSuffix}(ReadOnlySpan<byte> buffer, out int bytesRead)");
            stringBuilder.AppendLine($"        {{");
            stringBuilder.AppendLine($"            var result = new {typeDecl.Identifier}");
            stringBuilder.AppendLine($"            {{");

            foreach (var m in fieldsAndProps)
            {
                var memberType = m.Type;
                string? readExpression;
                string castExpression = "";

                if (memberType.TypeKind == TypeKind.Enum &&
                    memberType is INamedTypeSymbol nts)
                {
                    // FIXME: Namespace
                    castExpression = $"({memberType.Name})";
                    memberType = nts.EnumUnderlyingType;
                }

                switch (memberType.SpecialType)
                {
                    // Endianness aware basic types
                    case SpecialType.System_UInt16:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_Int16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_Int64:
                        string? basicTypeName = memberType.SpecialType switch
                        {
                            SpecialType.System_UInt16 => "UInt16",
                            SpecialType.System_UInt32 => "UInt32",
                            SpecialType.System_UInt64 => "UInt64",
                            SpecialType.System_Int16 => "Int16",
                            SpecialType.System_Int32 => "Int32",
                            SpecialType.System_Int64 => "Int64",
                            _ => throw new InvalidOperationException()
                        };
                        int basicTypeSize = memberType.SpecialType switch
                        {
                            SpecialType.System_UInt16 => 2,
                            SpecialType.System_UInt32 => 4,
                            SpecialType.System_UInt64 => 8,
                            SpecialType.System_Int16 => 2,
                            SpecialType.System_Int32 => 4,
                            SpecialType.System_Int64 => 8,
                            _ => 0
                        };
                        readExpression = $"{castExpression}BinaryPrimitives.Read{basicTypeName}{endianSuffix}(buffer.Slice({offset}{variableOffset}, {basicTypeSize}))";
                        offset += basicTypeSize;
                        break;

                    case SpecialType.System_Byte:
                        readExpression = $"{castExpression}buffer[{offset}{variableOffset}]";
                        offset ++;
                        break;

                    default:
                        var methods = memberType.GetMembers().OfType<IMethodSymbol>();
                        if (methods.Any(m => m.Name == $"Read{nameSuffix}"))
                        {
                            // FIXME: Missing namespace
                            readExpression = $"{m.Type.Name}.Read{nameSuffix}(buffer.Slice({offset}{variableOffset}), out var _{variableOffsetIndex})";
                        }
                        else
                        {
                            // FIXME: Missing namespace
                            readExpression = $"{m.Type.Name}.Read(buffer.Slice({offset}{variableOffset}), out var _{variableOffsetIndex})";
                        }

                        variableOffset.Append($" + _{variableOffsetIndex}");
                        variableOffsetIndex++;

                        // FIXME: Handle other basic type
                        // FIXME: Handle nested struct/classes by calling their Read
                        //throw new NotSupportedException();
                        break;
                }

                stringBuilder.AppendLine($"                {m.Name} = {readExpression},");
            }

            stringBuilder.AppendLine($"            }};");
            stringBuilder.AppendLine($"            bytesRead = {offset}{variableOffset};");
            stringBuilder.AppendLine($"            return result;");
            stringBuilder.AppendLine($"        }}");
        }
    }
}
