using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;

public class ErrorHandler : BaseErrorListener, IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    private readonly List<string> errors = new List<string>();
    private readonly HashSet<string> globalVariables = new HashSet<string>();
    private readonly HashSet<string> definedFunctions = new HashSet<string>();

    public List<string> GetErrors() => errors;

    // Listener pentru erori lexicale și sintactice
    // Implementarea pentru erori lexicale (IAntlrErrorListener<int>)
    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        string errorMessage = $"[Eroare lexicala] Linia {line}, Coloana {charPositionInLine}: {msg}";
        errors.Add(errorMessage);
    }


    // Metodă pentru analiza semantică: variabile globale
    public void CheckGlobalVariable(string varName)
    {
        // Verificăm dacă variabila a fost deja definită
        if (!globalVariables.Add(varName)) // .Add returnează false dacă varName este deja în set
        {
            errors.Add($"[Eroare semantica] Variabila globala '{varName}' este definita de mai multe ori.");
        }
    }


    // Metodă pentru analiza semantică: funcții
    public void CheckFunction(string funcName, List<string> paramTypes, int line)
    {
        // Creăm semnătura completă a funcției (nume + tipuri parametri)
        string funcSignature = $"{funcName}({string.Join(", ", paramTypes)})";

        // Verificăm dacă semnătura există deja
        if (definedFunctions.Contains(funcSignature))
        {
            errors.Add($"[Eroare semantica] Linia {line}: Functia '{funcName}' este deja definita cu aceeasi lista de parametri.");
        }
        else
        {
            definedFunctions.Add(funcSignature);
        }
    }


    // Metodă pentru analiza semantică: utilizarea variabilelor
    public void CheckVariableUsage(string varName)
    {
        if (!globalVariables.Contains(varName))
        {
            errors.Add($"[Eroare semantica] Variabila '{varName}' nu este definita.");
        }
    }

    // Salvarea erorilor într-un fișier
    public void SaveErrorsToFile(string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            foreach (string error in errors)
            {
                writer.WriteLine(error);
            }
        }
        //Console.WriteLine($"Erorile au fost salvate in fisierul '{filePath}'.");
    }
    public void CheckTypeCompatibility(string varType, string value, int line)
    {
        bool isCompatible = false;

        // Reguli simple de compatibilitate
        if ((varType == "int" && int.TryParse(value, out _)) ||
            (varType == "float" && float.TryParse(value, out _)) ||
            (varType == "double" && double.TryParse(value, out _)) ||
            (varType == "string" && value.StartsWith("\"") && value.EndsWith("\"")))
        {
            isCompatible = true;
        }

        if (!isCompatible)
        {
            errors.Add($"[Eroare semantica] Linia {line}: Tipul variabilei '{varType}' nu este compatibil cu valoarea '{value}'.");
        }
    }
}
