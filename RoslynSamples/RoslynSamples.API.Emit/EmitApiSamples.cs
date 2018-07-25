using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoslynSamples.API.Emit
{
    [TestClass]
    public class EmitApiSamples
    {
        [TestMethod]
        public void EmittingConsoleApplication()
        {
            var text = "Hello Roslyn!";
            var tree = CSharpSyntaxTree.ParseText($@"
using System;
using System.Diagnostics;

public class MyClass
{{
    public static void Main()
    {{
        Console.WriteLine(""{text}"");
    }}   
}}");

            var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            options = options.WithOptimizationLevel(OptimizationLevel.Release);     //Set optimization level
            options = options.WithPlatform(Platform.X86);                           //Set platform
            

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib }, options: options);

            //Emit to stream
            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);
                
                //Load into currently running assembly. Normally we'd probably
                //want to do this in an AppDomain
                var ourAssembly = Assembly.Load(ms.ToArray());
                var type = ourAssembly.GetType("MyClass");
                
                //Invokes our main method and writes "Hello World" :)
                type.InvokeMember("Main", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, null);
            }
        }
    }
}
