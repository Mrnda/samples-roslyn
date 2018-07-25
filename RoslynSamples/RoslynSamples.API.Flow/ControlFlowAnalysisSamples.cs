using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoslynSamples.API.Flow
{
    [TestClass]
    public class ControlFlowAnalysisSamples
    {
        [TestMethod]
        public void AnalysisOnForCycle()
        {
            var source = @"
    class C
    {
        void M()
        {
            for (int i = 0; i < 10; i++)
            {
                if (i == 3)
                    continue;
                return;
                if (i == 8)
                    break;
            }
        }
    }
";
            var tree = CSharpSyntaxTree.ParseText(source);
            var model = GetSemanticModel(tree);

            var firstFor = tree.GetRoot().DescendantNodes().OfType<ForStatementSyntax>().Single();
            ControlFlowAnalysis result = model.AnalyzeControlFlow(firstFor.Statement);

            Trace.WriteLine(source);
            Trace.WriteLine("");
            Trace.WriteLine($"Is start point reachable: {result.StartPointIsReachable}");
            Trace.WriteLine($"Is end point reachable: {result.EndPointIsReachable}");
            foreach (var exitPoint in result.ExitPoints)
            {
                Trace.WriteLine($"Exit points: {exitPoint}");
            }
            foreach (var returnStatements in result.ReturnStatements)
            {
                Trace.WriteLine($"Resturnt statements: {returnStatements}");
            }
        }

        private SemanticModel GetSemanticModel(SyntaxTree tree)
        {
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            return compilation.GetSemanticModel(tree);
        }
    }
}
