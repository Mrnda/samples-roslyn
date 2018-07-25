using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoslynSamples.API.Operations
{
    [TestClass]
    public class OperationAnalysisSamples
    {
        [TestMethod]
        public void GettingExpressionResultType()
        {
            var source = @"
public class C 
{
    public static void Main(string[] args)
    { 
        int a = 1 + 45645;
        double b = a + 1.0;
    }
}";
            var tree = CSharpSyntaxTree.ParseText(source);
            var compilation = GetCompilation(tree);

            var methodSyntax = tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First();
            var methodBodySyntax = methodSyntax.Body;

            var semanticModel = compilation.GetSemanticModel(tree);
            var operation = semanticModel.GetOperation(methodBodySyntax);
            foreach (var variableDeclarationGroupOperation in operation.Children.OfType<IVariableDeclarationGroupOperation>())
            {
                foreach (var variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
                {
                    foreach (var variableDeclaratorOperation in variableDeclarationOperation.Declarators)
                    {
                        var initializerOperation = variableDeclaratorOperation.Initializer.Value;
                        Trace.WriteLine("");
                        Trace.WriteLine($"Operation result type: {initializerOperation.Type}");
                    }
                }
            }
        }

        public Compilation GetCompilation(SyntaxTree tree)
        {
            //Get .NET Core mscorlib reference
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            //Create project compilation so we can access operation information
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });
            return compilation;
        }
    }
}
