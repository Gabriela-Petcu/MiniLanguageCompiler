# MiniLanguageCompiler
A compiler for a custom mini-programming language using ANTLR

MiniLanguageCompiler is a compiler for a custom mini-programming language, developed using ANTLR for lexical, syntactic, and semantic analysis. The compiler processes source code from a text file, extracts lexical units, validates syntax, and reports errors.

📌 Features
✔ Lexical Analysis: Tokenization of keywords, operators, identifiers, and literals.
✔ Syntax Parsing: Identification of function declarations, loops, and conditional statements.
✔ Semantic Analysis: Checks for variable scope, type mismatches, and function consistency.
✔ Error Detection: Reports lexical, syntactic, and semantic errors.
✔ File Output: Stores recognized tokens and parsed structures in separate text files.

🛠️ Technologies Used
✔ Java – Core programming language.
✔ ANTLR – Parser generator for lexical and syntax analysis.
✔ Parsing Algorithms – To validate the structure of the input code.

📝 How It Works
1. Reads the source program from a .txt file.
2. Extracts lexical units (identifiers, keywords, operators).
3. Builds the syntax tree to analyze function declarations, loops, and conditionals.
4. Performs semantic checks and reports errors if found.
5. Outputs structured data in files for tokens, variables, functions, and control structures.
