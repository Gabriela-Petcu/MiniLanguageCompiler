grammar MiniLang;

// Lexer rules
FUNCTION     : 'function';
RETURN       : 'return';
IF           : 'if';
ELSE         : 'else';
FOR          : 'for';
WHILE        : 'while';
INT          : 'int';
FLOAT        : 'float';
DOUBLE       : 'double';
STRING       : 'string';
VOID         : 'void';

PLUS         : '+';
MINUS        : '-';
MULT         : '*';
DIV          : '/';
MOD          : '%';

LT           : '<';
LTE          : '<=';
GT           : '>';
GTE          : '>=';
EQ           : '==';
NEQ          : '!=';

AND          : '&&';
OR           : '||';
NOT          : '!';

ASSIGN       : '=';
PLUS_ASSIGN  : '+=' ;
MINUS_ASSIGN : '-=';
MULT_ASSIGN  : '*=';
DIV_ASSIGN   : '/=';
MOD_ASSIGN   : '%=';

INCREMENT    : '++';
DECREMENT    : '--';

LPAREN       : '(';
RPAREN       : ')';
LBRACE       : '{';
RBRACE       : '}';
COMMA        : ',';
SEMI         : ';';


INT_LITERAL  : [0-9]+;
FLOAT_LITERAL: [0-9]+ '.' [0-9]+;
STRING_LITERAL : '"' (~["\\])* '"';

ID           : [a-zA-Z_][a-zA-Z0-9_]*;


LINE_COMMENT : '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
WS : [ \t\r\n]+ -> skip;



// Parser rules
program : (functionDecl | varDecl | statement)* EOF;

functionDecl : FUNCTION (VOID | INT | FLOAT | DOUBLE | STRING | ID) ID LPAREN paramList? RPAREN block ;

functionCall : ID LPAREN (expression (COMMA expression)*)? RPAREN;

paramList    : param (COMMA param)* ;
param        : (INT | FLOAT | DOUBLE | STRING) ID ;

varDecl      : (INT | FLOAT | DOUBLE | STRING) ID (ASSIGN expression)? SEMI ;

statement
    : varDecl
    | block
    | ifStatement
    | whileStatement
    | forStatement
    | returnStatement
    | expression SEMI
    | SEMI
    ;

block        : LBRACE statement* RBRACE ;

ifStatement
    : IF LPAREN expression RPAREN statement
    | IF LPAREN expression RPAREN statement ELSE statement
    ;

whileStatement : WHILE LPAREN expression RPAREN statement ;

forStatement : FOR LPAREN (varDecl | expression)? expression SEMI expression RPAREN statement ;


returnStatement : RETURN expression? SEMI ;

expression
    : ID                                              # IdentifierExpr
    | expression MULT expression                       # MultiplicationExpr
    | expression DIV expression                        # DivisionExpr
    | expression MOD expression                        # ModulusExpr
    | expression PLUS expression                       # AdditionExpr
    | expression MINUS expression                      # SubtractionExpr
    | expression (LT | LTE | GT | GTE | EQ | NEQ) expression # RelationalExpr
    | expression (AND | OR) expression                # LogicalExpr
    | NOT expression                                  # NotExpr
    | LPAREN expression RPAREN                        # ParenExpr
    | ID (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULT_ASSIGN | DIV_ASSIGN | MOD_ASSIGN) expression # AssignmentExpr
    | ID (INCREMENT | DECREMENT)                      # IncDecExpr
    | expression INCREMENT                            # PostIncrementExpr
    | expression DECREMENT                            # PostDecrementExpr
    | ID LPAREN (expression (COMMA expression)*)? RPAREN # FunctionCallExpr
    | INT_LITERAL                                     # IntLiteralExpr
    | FLOAT_LITERAL                                   # FloatLiteralExpr
    | STRING_LITERAL                                  # StringLiteralExpr
    ;

