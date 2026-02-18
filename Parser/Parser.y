%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public object obj; 
    public bool boolVal;
    public MyCompiler.NodeExpr node; // Add this to hold AST pieces
    public List<MyCompiler.ExpressionNodeExpr> exprList; // for expr_list
}

%token <obj> NUMBER STRING ID
%token <boolVal> BOOL_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON COMMA LPAREN RPAREN LBRACKET RBRACKET IF ELSE PRINT RANDOM FOR INC DECR
%token GE LE EQ NE GT LT 

%nonassoc IF
%nonassoc ELSE

%left PLUS MINUS
%left MULT DIV
%left GE LE EQ NE GT LT

%type <node> expr Statement StatementList Prog Assignment

%type <exprList> expr_list


/* This tells the parser which C# variable stores the final tree */
%{
    public MyCompiler.NodeExpr RootNode;
%}

%%

Prog
    : StatementList { $$ = $1; RootNode = $$; }
    ;

StatementList
    : Statement { $$ = new SequenceNodeExpr(); ((SequenceNodeExpr)$$).Statements.Add($1); }
    | StatementList SEMICOLON Statement { ((SequenceNodeExpr)$1).Statements.Add($3); $$ = $1; }
    | StatementList SEMICOLON { $$ = $1; }
    ;

Statement
    : Assignment          { $$ = $1; }
    | expr                { $$ = $1; }
    | IF LPAREN expr RPAREN Statement %prec IF
      { $$ = new IfNodeExpr($3 as ExpressionNodeExpr, $5); }
    | IF LPAREN expr RPAREN Statement ELSE Statement
      { $$ = new IfNodeExpr($3 as ExpressionNodeExpr, $5, $7); }
    | FOR LPAREN Assignment SEMICOLON expr SEMICOLON Assignment RPAREN Statement { $$ = new ForLoopNodeExpr($3 as StatementNodeExpr, $5 as ExpressionNodeExpr, $7 as StatementNodeExpr, $9 as ExpressionNodeExpr); }
    ;

Assignment 
    : ID ASSIGN expr      { $$ = new AssignNodeExpr((string)$1, $3 as ExpressionNodeExpr); } 
    | ID INC              { $$ = new IncrementNodeExpr((string)$1); }
    | ID DECR             { $$ = new DecrementNodeExpr((string)$1); }
    ;

expr_list
    : expr                     { $$ = new List<ExpressionNodeExpr> { $1 as ExpressionNodeExpr }; }
    | expr_list COMMA expr     { $1.Add($3 as ExpressionNodeExpr); $$ = $1; }
    ;

expr
    : BOOL_LITERAL        { $$ = new BooleanNodeExpr((bool)$1); }
    | NUMBER              { $$ = new NumberNodeExpr((int)$1); }
    | STRING              { $$ = new StringNodeExpr((string)$1); }
    | ID                  { $$ = new IdNodeExpr((string)$1); }
    | PRINT LPAREN expr RPAREN 
                          { $$ = new PrintNodeExpr($3 as ExpressionNodeExpr); }
    | RANDOM LPAREN expr COMMA expr RPAREN    
                          { $$ = new RandomNodeExpr($3 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
    | expr PLUS expr      { $$ = new BinaryOpNodeExpr($1 as ExpressionNodeExpr, "+", $3 as ExpressionNodeExpr); }
    | expr MINUS expr     { $$ = new BinaryOpNodeExpr($1 as ExpressionNodeExpr, "-", $3 as ExpressionNodeExpr); }
    | expr MULT expr      { $$ = new BinaryOpNodeExpr($1 as ExpressionNodeExpr, "*", $3 as ExpressionNodeExpr); }
    | expr DIV expr       { $$ = new BinaryOpNodeExpr($1 as ExpressionNodeExpr, "/", $3 as ExpressionNodeExpr); }

    | expr GE expr        { $$ = new ComparisonNodeExpr($1 as ExpressionNodeExpr, ">=", $3 as ExpressionNodeExpr); }    
    | expr LE expr        { $$ = new ComparisonNodeExpr($1 as ExpressionNodeExpr, "<=", $3 as ExpressionNodeExpr); }
    | expr EQ expr        { $$ = new ComparisonNodeExpr($1 as ExpressionNodeExpr, "==", $3 as ExpressionNodeExpr); }    
    | expr NE expr        { $$ = new ComparisonNodeExpr($1 as ExpressionNodeExpr, "!=", $3 as ExpressionNodeExpr); }
    | expr GT expr        { $$ = new ComparisonNodeExpr($1 as ExpressionNodeExpr, ">", $3 as ExpressionNodeExpr); }    
    | expr LT expr        { $$ = new ComparisonNodeExpr($1 as ExpressionNodeExpr, "<", $3 as ExpressionNodeExpr); }

    | LBRACKET expr_list RBRACKET { $$ = new ArrayNodeExpr($2 as List<ExpressionNodeExpr>); }

    | LPAREN expr RPAREN  { $$ = $2; }
    ;

%%

internal Parser(Scanner s) : base(s) { }