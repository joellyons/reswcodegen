using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.VSPackage.CustomTool
{
    public class CodeDomCodeGenerator : CodeGenerator, IDisposable
    {
        private readonly TypeAttributes? classAccessibility;
        private readonly string className;
        private readonly CodeNamespace codeNamespace;
        private readonly CodeCompileUnit compileUnit;
        private readonly CodeDomProvider provider;

        public CodeDomCodeGenerator(IResourceParser resourceParser,
                                    string className,
                                    string defaultNamespace,
                                    CodeDomProvider codeDomProvider = null,
                                    TypeAttributes? classAccessibility = null)
            : base(resourceParser, defaultNamespace)
        {
            this.className = className;
            this.classAccessibility = classAccessibility;
            compileUnit = new CodeCompileUnit();
            provider = codeDomProvider ?? new CSharpCodeProvider();
            codeNamespace = new CodeNamespace(defaultNamespace);
        }

        public override string GenerateCode()
        {
            codeNamespace.Comments.Add(
                new CodeCommentStatement("--------------------------------------------------------------------------------------------------"));
            codeNamespace.Comments.Add(new CodeCommentStatement("<auto-generatedInfo>"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tThis code was generated by ResW File Code Generator (http://bit.ly/reswcodegen)"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tResW File Code Generator was written by Christian Resma Helle"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tand is under GNU General Public License version 2 (GPLv2)"));
            codeNamespace.Comments.Add(new CodeCommentStatement(string.Empty));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tThis code contains a helper class exposing property representations"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tof the string resources defined in the specified .ResW file"));
            codeNamespace.Comments.Add(new CodeCommentStatement(string.Empty));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tGenerated: " + DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            codeNamespace.Comments.Add(new CodeCommentStatement("</auto-generatedInfo>"));
            codeNamespace.Comments.Add(
                new CodeCommentStatement("--------------------------------------------------------------------------------------------------"));

            codeNamespace.Imports.Add(new CodeNamespaceImport("Windows.ApplicationModel.Resources"));

            var targetClass = new CodeTypeDeclaration(className)
            {
                IsClass = true,
                IsPartial = true,
                TypeAttributes = TypeAttributes.Sealed | (classAccessibility ?? TypeAttributes.Public),
                Attributes = MemberAttributes.Public | MemberAttributes.Static | MemberAttributes.Final
            };

            const string resourceLoaderType = "ResourceLoader";
            const string resourceLoaderFieldName = "resourceLoader";
            var resourceLoaderField = new CodeMemberField(resourceLoaderType, resourceLoaderFieldName)
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static | MemberAttributes.Final
            };
            targetClass.Members.Add(resourceLoaderField);

            var resourceLoaderProperty = new CodeMemberProperty
            {
                Name = "Resource",
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                HasGet = true,
                Type = new CodeTypeReference(resourceLoaderType)
            };

            resourceLoaderProperty.Comments.Add(new CodeCommentStatement("<summary>", true));
            resourceLoaderProperty.Comments.Add(new CodeCommentStatement("Get or set ResourceLoader implementation", true));
            resourceLoaderProperty.Comments.Add(new CodeCommentStatement("</summary>", true));
            resourceLoaderProperty.GetStatements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression(resourceLoaderFieldName),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(className), "Initialize")))));
            resourceLoaderProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "resourceLoader")));
            resourceLoaderProperty.SetStatements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression(resourceLoaderFieldName),
                    new CodePropertySetValueReferenceExpression()));
            targetClass.Members.Add(resourceLoaderProperty);

            var initializeResourceLoader = new CodeMemberMethod
            {
                Name = "Initialize",
                Attributes = MemberAttributes.Public | MemberAttributes.Static
            };

            var executingAssemblyVar = new CodeVariableDeclarationStatement(typeof(string), "executingAssemblyName");
            var executingAssemblyInit = new CodeAssignStatement(new CodeVariableReferenceExpression("executingAssemblyName"),
                                                                new CodeSnippetExpression("Windows.UI.Xaml.Application.Current.GetType().AssemblyQualifiedName"));
            var executingAssemblySplit = new CodeVariableDeclarationStatement(typeof(string[]), "executingAssemblySplit");
            var executingAssemblyInit2 = new CodeAssignStatement(new CodeVariableReferenceExpression("executingAssemblySplit"),
                                                                 new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("executingAssemblyName"),
                                                                                                "Split",
                                                                                                new CodePrimitiveExpression(',')));
            var executingAssemblyInit3 = new CodeAssignStatement(new CodeVariableReferenceExpression("executingAssemblyName"),
                                                                 new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("executingAssemblySplit"),
                                                                                                new CodePrimitiveExpression(1)));

            var currentAssemblyVar = new CodeVariableDeclarationStatement(typeof(string), "currentAssemblyName");
            var currentAssemblyInit = new CodeAssignStatement(new CodeVariableReferenceExpression("currentAssemblyName"),
                                                              new CodePropertyReferenceExpression(new CodeTypeOfExpression(className), "AssemblyQualifiedName"));
            var currentAssemblySplit = new CodeVariableDeclarationStatement(typeof(string[]), "currentAssemblySplit");
            var currentAssemblyInit2 = new CodeAssignStatement(new CodeVariableReferenceExpression("currentAssemblySplit"),
                                                               new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("currentAssemblyName"),
                                                                                              "Split",
                                                                                              new CodePrimitiveExpression(',')));
            var currentAssemblyInit3 = new CodeAssignStatement(new CodeVariableReferenceExpression("currentAssemblyName"),
                                                               new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("currentAssemblySplit"),
                                                                                              new CodePrimitiveExpression(1)));

            var coreWindowTrueStatement = new CodeConditionStatement(
                new CodeSnippetExpression("executingAssemblyName.Equals(currentAssemblyName)"),
                new CodeStatement[] // true
                {
                    new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ResourceLoader"),
                                                                           "GetForCurrentView",
                                                                           new CodeSnippetExpression("\"" + className + "\"")))
                },
                new CodeStatement[] // false
                {
                    new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ResourceLoader"),
                                                                           "GetForCurrentView",
                                                                           new CodeSnippetExpression("currentAssemblyName + \"/" + className + "\"")))
                });
            var coreWindowFalseStatement = new CodeConditionStatement(
                new CodeSnippetExpression("executingAssemblyName.Equals(currentAssemblyName)"),
                new CodeStatement[] // true
                {
                    new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ResourceLoader"),
                                                                           "GetForViewIndependentUse",
                                                                           new CodeSnippetExpression("\"" + className + "\"")))
                },
                new CodeStatement[] // false
                {
                    new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ResourceLoader"),
                                                                           "GetForViewIndependentUse",
                                                                           new CodeSnippetExpression("currentAssemblyName + \"/" + className + "\"")))
                });
            var createResourceLoader = new CodeConditionStatement(
                new CodeSnippetExpression("Windows.UI.Core.CoreWindow.GetForCurrentThread() != null"),
                new CodeStatement[] { coreWindowTrueStatement },
                new CodeStatement[] { coreWindowFalseStatement });

            initializeResourceLoader.Statements.Add(executingAssemblyVar);
            initializeResourceLoader.Statements.Add(executingAssemblyInit);
            initializeResourceLoader.Statements.Add(executingAssemblySplit);
            initializeResourceLoader.Statements.Add(executingAssemblyInit2);
            initializeResourceLoader.Statements.Add(executingAssemblyInit3);
            initializeResourceLoader.Statements.Add(currentAssemblyVar);
            initializeResourceLoader.Statements.Add(currentAssemblyInit);
            initializeResourceLoader.Statements.Add(currentAssemblySplit);
            initializeResourceLoader.Statements.Add(currentAssemblyInit2);
            initializeResourceLoader.Statements.Add(currentAssemblyInit3);
            initializeResourceLoader.Statements.Add(createResourceLoader);
            targetClass.Members.Add(initializeResourceLoader);

            var resources = ResourceParser.Parse();
            foreach (var item in resources)
            {
                if (string.IsNullOrEmpty(item.Name))
                    continue;

                var property = new CodeMemberProperty
                {
                    Name = item.Name.Replace(".", "_").Trim(),
                    Attributes = MemberAttributes.Public | MemberAttributes.Static,
                    HasGet = true,
                    Type = new CodeTypeReference(typeof(string))
                };

                property.Comments.Add(new CodeCommentStatement("<summary>", true));
                property.Comments.Add(new CodeCommentStatement("Localized resource similar to \"" + (item.Value ?? item.Name) + "\"", true));
                property.Comments.Add(new CodeCommentStatement("</summary>", true));
                property.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeFieldReferenceExpression(null, "Resource"),
                            "GetString",
                            new CodePrimitiveExpression(item.Name.Replace(".", "/")))));

                targetClass.Members.Add(property);
            }

            codeNamespace.Types.Add(targetClass);
            compileUnit.Namespaces.Add(codeNamespace);

            return GenerateCodeFromCompileUnit();
        }

        private string GenerateCodeFromCompileUnit()
        {
            var options = new CodeGeneratorOptions { BracingStyle = "C" };

            var code = new StringBuilder();

            using (var writer = new StringWriter(code))
                provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);

            return code.ToString();
        }

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
        }

        ~CodeDomCodeGenerator()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (disposed)
                return;
            disposed = true;

            if (dispose)
                provider.Dispose();
        }

        #endregion
    }
}