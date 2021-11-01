using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BinaryFormat
{
    [Generator]
    public partial class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif 

            context.RegisterForPostInitialization((pi) => pi.AddSource("BinaryFormat.Attribute.cs", AttributeSource));
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                ExecuteInternal(context);
            }
            catch (Exception e)
            {
                //This is temporary till https://github.com/dotnet/roslyn/issues/46084 is fixed
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SI0000",
                        "An exception was thrown by the StrongInject generator",
                        "An exception was thrown by the StrongInject generator: '{0}'",
                        "StrongInject",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    e.ToString().Replace("\n", "")));
            }
        }
        public void ExecuteInternal(GeneratorExecutionContext context)
        {
            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxContextReceiver;

            foreach (var userType in syntaxReceiver.TypesToAugment)
            {
                GenerateReader(context, userType, context.Compilation.GetSemanticModel(userType.SyntaxTree));
            }
        }

        class MySyntaxReceiver : ISyntaxContextReceiver
        {
            public List<TypeDeclarationSyntax> TypesToAugment { get; private set; } = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is TypeDeclarationSyntax tds &&
                    context.SemanticModel.GetDeclaredSymbol(tds) is INamedTypeSymbol symbol &&
                    symbol.GetAttributes().Any(a => a.AttributeClass.Name == "GenerateReaderWriterAttribute"))
                {
                    TypesToAugment.Add(tds);
                }
            }
        }
    }
}
