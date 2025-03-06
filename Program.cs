using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace project1
{
    internal class Program
    {
        static void Main()
        {
            string filePath = @"C:\Users\user\Desktop\tema2\project1\bin\Debug\net8.0\input.txt";
            string errorFilePath = @"C:\Users\user\Desktop\tema2\project1\bin\Debug\net8.0\errors.txt";

            try
            {
                // Citirea fișierului de intrare
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Fișierul '{filePath}' nu a fost găsit.");
                    return;
                }

                string code = File.ReadAllText(filePath);
                Console.WriteLine("Continutul fisierului:");
                Console.WriteLine(code);

                // Crearea parser-ului și a arborelui sintactic
                ErrorHandler errorHandler = new ErrorHandler();
                var parser = CreateParser(code, errorHandler);

                var programTree = parser.program();

                if (programTree == null || programTree.ChildCount == 0)
                {
                    Console.WriteLine("Arborele generat este gol. Verificati erorile de sintaxa.");
                    errorHandler.SaveErrorsToFile("errors.txt");
                    return;
                }

                //Console.WriteLine("\nArborele generat:");
                //Console.WriteLine(programTree.ToStringTree(parser));

                // Analiza semantică
                AnalyzeSemantics(programTree, errorHandler);

                // Raportarea erorilor
                var errors = errorHandler.GetErrors();
                if (errors.Count > 0)
                {
                    Console.WriteLine("\nAu fost gasite urmatoarele erori:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }

                    errorHandler.SaveErrorsToFile(errorFilePath);
                    Console.WriteLine($"\nErorile au fost salvate in fisierul '{"errors.txt"}'.");
                }
                else
                {
                    Console.WriteLine("\nNu au fost gasite erori.");
                }


                // Extragerea funcțiilor și variabilelor globale
                var functions = ExtractFunctions(programTree);
                SaveToFile("functions.txt", functions, "Functiile");

                var globalVariables = ExtractGlobalVariables(programTree);
                SaveToFile("global_variables.txt", globalVariables, "Variabilele globale");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"A aparut o eroare: {ex.Message}");
            }
        }

        static MiniLangParser CreateParser(string code, ErrorHandler errorHandler)
        {
            AntlrInputStream inputStream = new AntlrInputStream(code);
            MiniLangLexer lexer = new MiniLangLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            MiniLangParser parser = new MiniLangParser(tokenStream);

            // Adaugă ascultătorul pentru erori lexicale
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorHandler); // Gestionarea erorilor lexicale

            // Adaugă ascultătorul pentru erori sintactice
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorHandler);

            return parser;
        }
        static void AnalyzeSemantics(MiniLangParser.ProgramContext programTree, ErrorHandler errorHandler)
        {
            // HashSet pentru a stoca funcțiile definite
            var definedFunctions = new HashSet<string>();

            // Colectăm funcțiile definite
            foreach (var child in programTree.children)
            {
                if (child is MiniLangParser.FunctionDeclContext funcDecl)
                {
                    string funcName = funcDecl.GetChild(1).GetText(); // Numele funcției
                    var paramTypes = funcDecl.paramList()?.param()
                        .Select(param => param.GetChild(0).GetText())
                        .ToList() ?? new List<string>();

                    errorHandler.CheckFunction(funcName, paramTypes, funcDecl.Start.Line);
                    definedFunctions.Add($"{funcName}({string.Join(", ", paramTypes)})"); // Adăugăm funcția cu semnătura completă
                }
            }

            // Verificăm declarațiile de variabile și utilizarea lor
            foreach (var child in programTree.children)
            {
                if (child is MiniLangParser.VarDeclContext varDecl)
                {
                    HandleGlobalVariable(varDecl, errorHandler);
                }
                else if (child is MiniLangParser.FunctionDeclContext funcDecl)
                {
                    HandleFunction(funcDecl, errorHandler);
                }
                else if (child is MiniLangParser.ExpressionContext expr)
                {
                    HandleExpression(expr, definedFunctions, errorHandler);
                }
            }
        }

        static void HandleGlobalVariable(MiniLangParser.VarDeclContext varDecl, ErrorHandler errorHandler)
        {
            // Identificăm tipul și numele variabilei
            string varType = varDecl.GetChild(0).GetText(); // Tipul variabilei
            string varName = varDecl.GetChild(1).GetText(); // ID-ul este al doilea copil

            // Verificăm dacă variabila este globală
            if (IsGlobalDeclaration(varDecl))
            {
                errorHandler.CheckGlobalVariable(varName);
            }

            // Verificăm compatibilitatea tipului cu valoarea inițializată
            string value = varDecl.GetChild(3)?.GetText() ?? "null"; // Valoarea atribuită (dacă există)
            if (value != "null") // Verificăm doar dacă variabila este inițializată
            {
                errorHandler.CheckTypeCompatibility(varType, value, varDecl.Start.Line);
            }
        }

        static void HandleFunction(MiniLangParser.FunctionDeclContext funcDecl, ErrorHandler errorHandler)
        {
            string funcName = funcDecl.GetChild(1).GetText(); // Numele funcției

            // Obține parametrii funcției
            var paramNames = funcDecl.paramList()?.param()
                .Select(param => param.GetChild(1).GetText())
                .ToHashSet() ?? new HashSet<string>();

            var localVariables = new HashSet<string>();

            foreach (var statement in funcDecl.block()?.statement() ?? Enumerable.Empty<MiniLangParser.StatementContext>())
            {
                foreach (var subNode in statement.children)
                {
                    if (subNode is MiniLangParser.VarDeclContext localVarDecl)
                    {
                        string varName = localVarDecl.GetChild(1).GetText();

                        // Verifică dacă variabila locală are același nume ca un parametru
                        if (paramNames.Contains(varName))
                        {
                            errorHandler.GetErrors().Add($"[Eroare semantica] Linia {localVarDecl.Start.Line}: Variabila locala '{varName}' din functia '{funcName}' are acelasi nume ca un parametru.");
                        }

                        // Verifică duplicarea variabilelor locale
                        if (!localVariables.Add(varName))
                        {
                            errorHandler.GetErrors().Add($"[Eroare semantica] Linia {localVarDecl.Start.Line}: Variabila locala '{varName}' este definita de mai multe ori in functia '{funcName}'.");
                        }
                    }
                }
            }
        }

        static void HandleExpression(MiniLangParser.ExpressionContext expr, HashSet<string> definedFunctions, ErrorHandler errorHandler)
        {
            foreach (var subNode in expr.children)
            {
                if (subNode is MiniLangParser.FunctionCallExprContext funcCallExpr) // Nodurile apelurilor de funcții
                {
                    string funcName = funcCallExpr.GetChild(0).GetText(); // Obține numele funcției apelate
                    if (!definedFunctions.Contains(funcName)) // Verificăm dacă funcția este definită
                    {
                        errorHandler.GetErrors().Clear(); // Eliminăm toate celelalte erori
                        errorHandler.GetErrors().Add($"[Eroare semantica] Linia {funcCallExpr.Start.Line}: Functia '{funcName}' este apelata, dar nu a fost definita.");
                        return; // Ne oprim după ce raportăm această eroare
                    }
                }
            }
        }



        static List<string> ExtractFunctions(MiniLangParser.ProgramContext programTree)
        {
            var functions = new List<string>();
            foreach (var child in programTree.children)
            {
                if (child is MiniLangParser.FunctionDeclContext functionDecl)
                {
                    string functionName = functionDecl.GetChild(1).GetText(); // ID-ul funcției
                    string returnType = functionDecl.GetChild(0).GetText(); // Tipul returnat
                    bool isRecursive = functionDecl.block()?.GetText().Contains(functionName) ?? false;
                    string functionType = isRecursive ? "recursiva" : "iterativa";

                    string parameters = "Fara parametri";
                    if (functionDecl.paramList() != null)
                    {
                        parameters = string.Join(", ", functionDecl.paramList()
                            .param()
                            .Select(param => $"{param.GetChild(0).GetText()} {param.GetChild(1).GetText()}"));
                    }

                    var functionInfo = $"Functie: {functionName}\n" +
                                       $"Tip: {functionType}\n" +
                                       $"Tip returnare: {returnType}\n" +
                                       $"Parametri: {parameters}\n";

                    functions.Add(functionInfo);
                }
            }
            return functions;
        }

        static List<string> ExtractGlobalVariables(MiniLangParser.ProgramContext programTree)
        {
            var globalVariables = new List<string>();
            foreach (var node in programTree.children)
            {
                if (node is MiniLangParser.VarDeclContext varDecl)
                {
                    string type = varDecl.GetChild(0).GetText();
                    string name = varDecl.GetChild(1).GetText(); // ID-ul este al doilea copil
                    string value = varDecl.GetChild(3)?.GetText() ?? "null"; // Valoarea atribuită (dacă există)
                    globalVariables.Add($"<Tip: {type}, Nume: {name}, Valoare: {value}>");
                }
            }
            return globalVariables;
        }

        static void SaveToFile(string filePath, List<string> content, string itemType)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.NewLine = "\r\n";
                foreach (var item in content)
                {
                    writer.WriteLine(item.TrimEnd());
                    writer.WriteLine("----------------------");
                }
            }
            Console.WriteLine($"\n{itemType} au fost salvate in fisierul '{filePath}'.");
        }

        static bool IsGlobalDeclaration(MiniLangParser.VarDeclContext varDecl)
        {
            var parent = varDecl.Parent;
            while (parent != null)
            {
                if (parent is MiniLangParser.FunctionDeclContext)
                {
                    return false; // Variabila este în interiorul unei funcții, deci nu este globală
                }
                parent = parent.Parent;
            }
            return true; // Variabila este globală dacă nu aparține unei funcții
        }
    }
}