%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public object obj; 
    public bool boolVal;
    public MyCompiler.Node node; // Add this to hold AST pieces
}

%token <obj> NUMBER STRING ID
%token <boolVal> BOOL_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON LPAREN RPAREN IF ELSE

%nonassoc IF
%nonassoc ELSE

%left PLUS MINUS
%left MULT DIV
%left GE LE EQ NE GT LT

%type <node> expr Statement Prog

/* This tells the parser which C# variable stores the final tree */
%{
    public MyCompiler.Node RootNode;
%}

%%

Prog
    : /* empty */              { $$ = new SequenceNode(); RootNode = $$; }
    | Prog Statement           { ((SequenceNode)$1).Statements.Add($2); $$ = $1; RootNode = $$; }
    | Prog Statement SEMICOLON { ((SequenceNode)$1).Statements.Add($2); $$ = $1; RootNode = $$; }
    ;


Statement
    : ID ASSIGN expr      { $$ = new AssignNode((string)$1, $3 as ExpressionNode); }
    | expr                { $$ = $1; }
    | IF LPAREN expr RPAREN Statement %prec IF
      { $$ = new IfNode($3 as ExpressionNode, $5); }
    | IF LPAREN expr RPAREN Statement ELSE Statement
      { $$ = new IfNode($3 as ExpressionNode, $5, $7); }
    ;

expr
    : BOOL_LITERAL        { $$ = new BooleanNode((bool)$1); }
    | NUMBER              { $$ = new NumberNode((int)$1); }
    | STRING              { $$ = new StringNode((string)$1); }
    | ID                  { $$ = new IdNode((string)$1); }
    | expr PLUS expr      { $$ = new BinaryOpNode($1 as ExpressionNode, "+", $3 as ExpressionNode); }
    | expr MINUS expr     { $$ = new BinaryOpNode($1 as ExpressionNode, "-", $3 as ExpressionNode); }
    | expr MULT expr      { $$ = new BinaryOpNode($1 as ExpressionNode, "*", $3 as ExpressionNode); }
    | expr DIV expr       { $$ = new BinaryOpNode($1 as ExpressionNode, "/", $3 as ExpressionNode); }

    | expr GE expr       { $$ = new ComparisonNode($1 as ExpressionNode, ">=", $3 as ExpressionNode); }    
    | expr LE expr       { $$ = new ComparisonNode($1 as ExpressionNode, "<=", $3 as ExpressionNode); }
    | expr EQ expr       { $$ = new ComparisonNode($1 as ExpressionNode, "==", $3 as ExpressionNode); }    
    | expr NE expr       { $$ = new ComparisonNode($1 as ExpressionNode, "!=", $3 as ExpressionNode); }
    | expr GT expr       { $$ = new ComparisonNode($1 as ExpressionNode, ">", $3 as ExpressionNode); }    
    | expr LT expr       { $$ = new ComparisonNode($1 as ExpressionNode, "<", $3 as ExpressionNode); }

    | LPAREN expr RPAREN  { $$ = $2; }
    ;

%%

internal Parser(Scanner s) : base(s) { }
