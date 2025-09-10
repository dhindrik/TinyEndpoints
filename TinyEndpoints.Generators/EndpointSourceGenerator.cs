using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace TinyEndpoints.Generators;

[Generator]
public class EndpointSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

        var compilation = context.Compilation;

        // Types we rely on
        var endpointAttributeBase = compilation.GetTypeByMetadataName("TinyEndpoints.EndpointAttribute");
        if (endpointAttributeBase is null) return; // library not referenced
        var generateEndpointsAttr = compilation.GetTypeByMetadataName("TinyEndpoints.GenerateEndpointsAttribute");

        // Collect any classes annotated with [GenerateEndpoints]
        var annotatedClasses = new List<INamedTypeSymbol>();
        if (generateEndpointsAttr != null)
        {
            foreach (var classDecl in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
                if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol clsSymbol)
                {
                    if (clsSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, generateEndpointsAttr)))
                    {
                        annotatedClasses.Add(clsSymbol);
                    }
                }
            }
        }

        var mappingBuilder = new StringBuilder();
        var instanceFields = new Dictionary<string, string>(); // containingTypeDisplay -> fieldName
        int instanceCounter = 0;

        foreach (var methodSyntax in receiver.CandidateMethods)
        {
            var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            if (model.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol symbol) continue;

            // Find first attribute that derives from EndpointAttribute
            var httpAttr = symbol.GetAttributes().FirstOrDefault(a => InheritsFrom(a.AttributeClass, endpointAttributeBase));
            if (httpAttr == null) continue;

            var route = httpAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
            if (route is null) continue;

            var methodKind = httpAttr.AttributeClass?.Name switch
            {
                string n when n.StartsWith("GetAttribute") => "MapGet",
                string n when n.StartsWith("PostAttribute") => "MapPost",
                string n when n.StartsWith("PutAttribute") => "MapPut",
                string n when n.StartsWith("DeleteAttribute") => "MapDelete",
                _ => null
            };
            if (methodKind is null) continue;

            var containingType = symbol.ContainingType.ToDisplayString();
            var methodName = symbol.Name;

            // Determine configurator type
            string? configuratorType = null;
            var configuratorProp = httpAttr.NamedArguments.FirstOrDefault(kv => kv.Key == "ConfiguratorType").Value;
            if (configuratorProp.Value is ITypeSymbol typeSymbol)
            {
                configuratorType = typeSymbol.ToDisplayString();
            }
            else if (httpAttr.AttributeClass is INamedTypeSymbol attrClass && attrClass.TypeArguments.Length == 1)
            {
                configuratorType = attrClass.TypeArguments[0].ToDisplayString();
            }

            string handlerReference;
            if (symbol.IsStatic)
            {
                handlerReference = $"{containingType}.{methodName}"; // method group
            }
            else
            {
                if (!instanceFields.TryGetValue(containingType, out var fieldName))
                {
                    fieldName = $"__instance_{instanceCounter++}";
                    instanceFields[containingType] = fieldName;
                }
                handlerReference = $"{fieldName}.{methodName}"; // instance method group
            }

            if (configuratorType is not null)
            {
                mappingBuilder.AppendLine($"{{ var b = app.{methodKind}(\"{route}\", {handlerReference}); var cfg = new {configuratorType}(); cfg.Configure(b); }}");
            }
            else
            {
                mappingBuilder.AppendLine($"app.{methodKind}(\"{route}\", {handlerReference});");
            }
        }

        // Nothing to emit if no endpoints discovered
        if (mappingBuilder.Length == 0) return;


        // For each annotated class emit a partial containing registration method.
        foreach (var cls in annotatedClasses)
        {
            var ns = cls.ContainingNamespace.IsGlobalNamespace ? null : cls.ContainingNamespace.ToDisplayString();
            var accessibility = cls.DeclaredAccessibility switch
            {
                Accessibility.Public => "public ",
                Accessibility.Internal => "internal ",
                _ => string.Empty
            };

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using Microsoft.AspNetCore.Builder;");
            if (ns is not null)
            {
                sb.Append("namespace ").Append(ns).AppendLine(";");
            }
            sb.AppendLine($"{accessibility}partial class {cls.Name}");
            sb.AppendLine("{");

            // Emit instance fields
            foreach (var kvp in instanceFields)
            {
                sb.AppendLine($"    private static readonly {kvp.Key} {kvp.Value} = new {kvp.Key}();");
            }
            sb.AppendLine("    public static void RegisterGeneratedEndpoints(WebApplication app)");
            sb.AppendLine("    {");
            sb.Append(mappingBuilder.ToString());
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"{cls.Name}.GeneratedEndpoints.g.cs", sb.ToString());
        }

    }

    private static bool InheritsFrom(INamedTypeSymbol? type, INamedTypeSymbol targetBase)
    {
        while (type != null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, targetBase)) return true;
            type = type.BaseType;
        }
        return false;
    }

    private sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> CandidateMethods { get; } = new();
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax m && m.AttributeLists.Count > 0)
            {
                CandidateMethods.Add(m);
            }
            else if (syntaxNode is ClassDeclarationSyntax c && c.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(c);
            }
        }
    }
}
