using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoslynSamples.API.Symbols
{
    [TestClass]
    public class SymbolAnalysisSamples
    {
        [TestMethod]
        public void AccessingClassSymbol()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
class MyClass
{
    class NestedClass
    {
    }
    struct NestedStruct
    {
    }
    void Method()
    {
    }
    int Property { get; set; }
    int field;
    MyClass() 
    {

    }
}");
            //Get .NET Core mscorlib reference
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            //Create project compilation so we can access symbol information
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });
            var semanticModel = compilation.GetSemanticModel(tree);
            
            //Find class symbol by its syntax
            var classSyntax = tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
            var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

            foreach (var member in classSymbol.GetMembers())
            {
                Trace.WriteLine($"Symbol name: {member.Name}");
                Trace.WriteLine($"Symbol kind: {member.Kind}");
                if (member is IPropertySymbol property)
                {
                    Trace.WriteLine($"Property symbol type: {property.Type.Name}");
                }
                Trace.WriteLine("");
            }
        }
    }
}
