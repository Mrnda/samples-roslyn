using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoslynSamples.API.Syntax
{
    [TestClass]
    public class SyntaxParsingSamples
    {
        [TestMethod]
        public void AnalysingBasicClass()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
    using System;

    public class MyClass
    {
        public void MyMethod()
        {
            throw new NotImplementedException();
        }
    }");

            var syntaxNode = tree.GetRoot();

            var @class = syntaxNode.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

            var method = @class.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            Trace.WriteLine($"Method name: {method.Identifier.ToFullString()}");
            Trace.WriteLine($"Method return type: {method.ReturnType.ToFullString()}");
            Trace.WriteLine($"Method body: {method.Body}");
        }

        [TestMethod]
        public void EmptyStatementRemoving()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
    public class Sample
    {
       public void Foo()
       {
          Console.WriteLine();
          ;
        }
    }");
            //Create rewriter class and use it on created tree
            var rewriter = new EmptyStatementDuplicatorRewriter();
            var result = rewriter.Visit(tree.GetRoot()).NormalizeWhitespace();

            Trace.WriteLine(result.ToFullString());
        }

        public class EmptyStatementDuplicatorRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax node)
            {
                return SyntaxFactory.Block(
                    new[] 
                    {
                        SyntaxFactory.EmptyStatement(),
                        SyntaxFactory.EmptyStatement()
                    }
                );
            }
        }
    }
}
