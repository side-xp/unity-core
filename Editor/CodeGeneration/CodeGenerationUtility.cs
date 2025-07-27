using System;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous utility functions for generating code, following our standards.
    /// </summary>
    public class CodeGenerationUtility
    {

        /// <summary>
        /// Pattern used to detect and remove the auto-generated comment for generated scripts.
        /// </summary>
        private static readonly Regex AutocommentPattern = new Regex(@"(?<autocomment>(//-+)(.+)(//-+)[\n\r]*)", RegexOptions.Singleline);

        /// <summary>
        /// Pattern used to replace excess of blank lines by a single one.
        /// </summary>
        private static readonly Regex MultiBlankLinesPattern = new Regex(@"(?:\s*(?:\r|\n|\r\n)){3,}");

        /// <summary>
        /// Pattern used to replace a sequence of closing brackets to make sure that there's a blank line to separate them.
        /// </summary>
        private static readonly Regex ClosingBracketsPattern = new Regex(@"(?:}(?:\r|\n|\r\n)(?<indent>[\s-[\r\n]]{0,4})})");

        /// <summary>
        /// The options used by default by the script generator.
        /// </summary>
        public static readonly CodeGeneratorOptions CompileUnitOptions = new CodeGeneratorOptions
        {
            BracingStyle = "C",
            BlankLinesBetweenMembers = true
        };

        /// <inheritdoc cref="MakeScriptCompileUnit(Type, out CodeNamespace, out CodeNamespace)"/>
        public static CodeCompileUnit MakeScriptCompileUnit(out CodeNamespace importsNamespace, out CodeNamespace domNamespace)
        {
            return MakeScriptCompileUnit(null, out importsNamespace, out domNamespace);
        }

        /// <summary>
        /// Creates a C# CodeDom compile unit used to generate a script.
        /// </summary>
        /// <param name="relatedType">The type to which the class is related. Used to set the same namespace for the script to generate.</param>
        /// <param name="importsNamespace">Outputs the global namespace used to list the using statements.</param>
        /// <param name="domNamespace">Outputs the namespace where the class is generated. Can be the same as the imports namespace if the related type is not provided or doesn't have a namespace.</param>
        /// <returns>Returns the created CodeDom compile unit.</returns>
        public static CodeCompileUnit MakeScriptCompileUnit(Type relatedType, out CodeNamespace importsNamespace, out CodeNamespace domNamespace)
        {
            CodeCompileUnit domCompileUnit = new CodeCompileUnit();

            // Declare global namespace (meant to add using statements outside the actual script namespace)
            importsNamespace = new CodeNamespace();
            domCompileUnit.Namespaces.Add(importsNamespace);

            domNamespace = importsNamespace;
            // Declare script namespace if applicable
            if (relatedType != null && !string.IsNullOrEmpty(relatedType.Namespace))
            {
                domNamespace = new CodeNamespace(relatedType.Namespace);
                domCompileUnit.Namespaces.Add(domNamespace);
            }

            return domCompileUnit;
        }

        /// <summary>
        /// Generates a script from a given <see cref="CodeCompileUnit"/>.
        /// </summary>
        /// <param name="compileUnit">The compile unit used to generate the script.</param>
        /// <returns>Returns the generated script content.</returns>
        public static string GenerateScript(CodeCompileUnit compileUnit)
        {
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            StringWriter writer = new StringWriter();
            codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = true,
                IndentString = "    ",
            });
            string script = writer.ToString();

            RemoveAutocomment(ref script);
            FixBlankLines(ref script);
            return script;
        }

        /// <summary>
        /// Generates a script file from a given <see cref="CodeCompileUnit"/>.
        /// </summary>
        /// <param name="path">The path to the script file to generate.</param>
        /// <inheritdoc cref="GenerateScript(CodeCompileUnit)"/>
        /// <returns>Returns true if the script has been generated successfully.</returns>
        public static bool GenerateScriptFile(string path, CodeCompileUnit compileUnit)
        {
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            path = path.ToAbsolutePath();

            using (StreamWriter writer = new StreamWriter(path))
            {
                codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, CompileUnitOptions);
            }
            RemoveAutocommentFromFile(path);

            return true;
        }

        /// <summary>
        /// Removes the comment added by C# code DOM provider when generating a script.
        /// </summary>
        /// <param name="scriptContent">The content of the script as a string.</param>
        /// <returns>Returns the script content as a string after removing the auto-comment.</returns>
        public static void RemoveAutocomment(ref string scriptContent)
        {
            scriptContent = AutocommentPattern.Replace(scriptContent, "");
        }

        /// <inheritdoc cref="RemoveAutocomment(string)"/>
        /// <param name="path">The path to the generated file.</param>
        public static void RemoveAutocommentFromFile(string path)
        {
            string generatedContent = File.ReadAllText(path);
            RemoveAutocomment(ref generatedContent);
            File.WriteAllText(path, generatedContent);
        }

        /// <summary>
        /// Removes excess blank lines andd add one between closing brackets.
        /// </summary>
        /// <param name="scriptContent">The content of the script as a string.</param>
        public static void FixBlankLines(ref string scriptContent)
        {
            string previous = null;
            do
            {
                previous = scriptContent;

                scriptContent = MultiBlankLinesPattern.Replace(scriptContent, Environment.NewLine.Repeat(2));
                scriptContent = ClosingBracketsPattern.Replace(scriptContent, $"}}{Environment.NewLine.Repeat(2)}$1}}");
            }
            while (previous != scriptContent);
        }

    }

}