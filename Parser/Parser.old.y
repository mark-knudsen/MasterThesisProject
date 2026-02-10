%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public object obj; 
    public MyCompiler.Node node; // Add this to hold AST pieces
}

%token <obj> NUMBER STRING ID
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON LPAREN RPAREN

%left PLUS MINUS
%left MULT DIV

%type <node> expr Statement Prog

/* This tells the parser which C# variable stores the final tree */
%{
    public MyCompiler.Node RootNode;
%}

%%

Prog
    : /* empty */ { $$ = new SequenceNode(); RootNode = $$; }
    | Prog Statement SEMICOLON { ((SequenceNode)$1).Statements.Add($2); $$ = $1; RootNode = $$; }
    | Prog Statement           { ((SequenceNode)$1).Statements.Add($2); $$ = $1; RootNode = $$; }
    ;


Statement
    : ID ASSIGN expr      { $$ = new AssignNode((string)$1, $3); }
    | expr                { $$ = $1; }
    | IF LPAREN expr RPAREN Statement %prec IF
      { $$ = new IfNode($3, $5); }
    | IF LPAREN expr RPAREN Statement ELSE Statement
      { $$ = new IfNode($3, $5, $7); }
    ;

expr
    : NUMBER              { $$ = new NumberNode((int)$1); }
    | STRING              { $$ = new StringNode((string)$1); }
    | ID                  { $$ = new IdNode((string)$1); }
    | expr PLUS expr      { $$ = new BinaryOpNode($1, "+", $3); }
    | expr MINUS expr     { $$ = new BinaryOpNode($1, "-", $3); }
    | expr MULT expr      { $$ = new BinaryOpNode($1, "*", $3); }
    | expr DIV expr       { $$ = new BinaryOpNode($1, "/", $3); }
    | LPAREN expr RPAREN  { $$ = $2; }
    ;

%%

internal Parser(Scanner s) : base(s) { }


/*
%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { public object obj; }

%token <obj> NUMBER STRING ID
%token PLUS MINUS MULT DIV ASSIGN LPAREN RPAREN SEMICOLON

%left PLUS MINUS
%left MULT DIV

%type <obj> expr Statement

%{
    public static System.Collections.Generic.Dictionary<string, object> Vars = new();
%}

%%

/* The top-level rule: a program is a list of statements */
Prog
    : /* empty */
    | Prog Statement SEMICOLON
    ;

Statement
    : ID ASSIGN expr      { Vars[(string)$1] = $3; Console.WriteLine($1 + " = " + $3); $$ = $3; }
    | expr                { Console.WriteLine("Result: " + $1); $$ = $1; }
    ;

expr
    : NUMBER              { $$ = $1; }
    | STRING              { $$ = $1; }
    | ID                  { $$ = Vars.ContainsKey((string)$1) ? Vars[(string)$1] : 0; }
    | expr PLUS expr      { 
                            if($1 is int && $3 is int) $$ = (int)$1 + (int)$3;
                            else $$ = $1.ToString() + $3.ToString(); 
                          }
    | expr MINUS expr     { $$ = (int)$1 - (int)$3; }
    | expr MULT expr      { $$ = (int)$1 * (int)$3; }
    | expr DIV expr       { $$ = (int)$1 / (int)$3; }
    | LPAREN expr RPAREN  { $$ = $2; }
    ;

%%

internal Parser(Scanner s) : base(s) { }
*/