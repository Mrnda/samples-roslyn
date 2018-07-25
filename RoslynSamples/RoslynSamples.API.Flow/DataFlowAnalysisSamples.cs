using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoslynSamples.API.Flow
{
    [TestClass]
    public class DataFlowAnalysisSamples
    {
        [TestMethod]
        public void AnalysisOnArrayAccess()
        {
            var source = @"
public class Sample
{
   public void Foo()
   {
        int[] outerArray = new int[10] { 0, 1, 2, 3, 4, 0, 1, 2, 3, 4};
        for (int index = 0; index < 10; index++)
        {
             int[] innerArray = new int[10] { 0, 1, 2, 3, 4, 0, 1, 2, 3, 4 };
             index = index + 2;
             outerArray[index - 1] = 5;
        }
   }
}";
            var tree = CSharpSyntaxTree.ParseText(source);
            SemanticModel model = GetSemanticModel(tree);

            var forStatement = tree.GetRoot().DescendantNodes().OfType<ForStatementSyntax>().Single();
            DataFlowAnalysis result = model.AnalyzeDataFlow(forStatement);

            Trace.WriteLine(source);
            DumpSymbolArray("Always Assigned", result.AlwaysAssigned);
            DumpSymbolArray("Written Outside", result.WrittenOutside);
            DumpSymbolArray("Written Inside", result.WrittenInside);
            DumpSymbolArray("Read Inside", result.ReadInside);
            DumpSymbolArray("Variables Declared", result.VariablesDeclared);

            void DumpSymbolArray(string prefix, ImmutableArray<ISymbol> symbols)
            {
                foreach (var symbol in symbols)
                {
                    Trace.WriteLine($"{prefix}: {symbol}");
                }
            }
        }

        private  SemanticModel GetSemanticModel(SyntaxTree tree)
        {
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);
            return model;
        }
    }
}