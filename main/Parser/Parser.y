%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
   public object obj; 
    public bool boolVal;
    public double fval;
    public MyCompiler.NodeExpr node; // Add this to hold AST pieces
    public List<MyCompiler.ExpressionNodeExpr> exprList; // for expr_list
}

%token <obj> NUMBER STRING ID
%token <boolVal> BOOL_LITERAL
%token <fval> FLOAT_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON COMMA DOT COLON LAMBDA
%token PLUS_ASSIGN MINUS_ASSIGN
%token LPAREN RPAREN LBRACE RBRACE LBRACKET RBRACKET IF ELSE FOR INC DECR 
%token PRINT RANDOM ROUND FUNC WHERE

%token GE LE EQ NE GT LT 

%nonassoc IF
%nonassoc ELSE

%left GE LE EQ NE GT LT
%left PLUS MINUS
%left MULT DIV
%left LBRACKET /* Add this to give indexing high priority */

%type <node> Prog Statement StatementList Assignment
%type <node> expr
%type <obj> params args  /* Use <obj> for lists */
%type <exprList> expr_list

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
    | StatementList Statement { ((SequenceNodeExpr)$1).Statements.Add($2); $$ = $1; }
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
    | FOR LPAREN Assignment SEMICOLON expr SEMICOLON Assignment RPAREN Statement 
      { $$ = new ForLoopNodeExpr($3 as StatementNodeExpr, $5 as ExpressionNodeExpr, $7 as StatementNodeExpr, $9); }
    | FOR LPAREN Assignment SEMICOLON expr SEMICOLON Assignment RPAREN LBRACE StatementList RBRACE
      { $$ = new ForLoopNodeExpr($3 as StatementNodeExpr, $5 as ExpressionNodeExpr, $7 as StatementNodeExpr, $10); }
    ;

Assignment 
    : ID ASSIGN expr      { $$ = new AssignNodeExpr((string)$1, $3 as ExpressionNodeExpr); } 
    | ID PLUS_ASSIGN expr { 
        // Sugar: x += 1  becomes  x = x + 1
        var id = new IdNodeExpr((string)$1);
        var add = new BinaryOpNodeExpr(id, "+", $3 as ExpressionNodeExpr);
        $$ = new AssignNodeExpr((string)$1, add); 
    }
    | ID MINUS_ASSIGN expr { 
        // Sugar: x -= 1  becomes  x = x - 1
        var id = new IdNodeExpr((string)$1);
        var sub = new BinaryOpNodeExpr(id, "-", $3 as ExpressionNodeExpr);
        $$ = new AssignNodeExpr((string)$1, sub); 
    }
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
    | FLOAT_LITERAL       { $$ = new FloatNodeExpr($1); }
    | STRING              { $$ = new StringNodeExpr((string)$1); }
    
    /* 1. Standard function: Defaults to "Float" */
    | FUNC ID LPAREN params RPAREN LBRACE expr RBRACE 
                          { $$ = new FunctionDefNode((string)$2, "Float", (List<string>)$4, $7 as NodeExpr); }

    /* 2. Typed function: Uses the ID after the colon as the type */
    | FUNC COLON ID ID LPAREN params RPAREN LBRACE expr RBRACE 
                          { $$ = new FunctionDefNode((string)$4, (string)$3, (List<string>)$6, $9 as NodeExpr); }

    | ID LPAREN args RPAREN 
                          { $$ = new FunctionCallNode((string)$1, (List<ExpressionNodeExpr>)$3); }
    | ID                  { $$ = new IdNodeExpr((string)$1); }
    | PRINT LPAREN expr RPAREN 
                          { $$ = new PrintNodeExpr($3 as ExpressionNodeExpr); }
    | RANDOM LPAREN expr COMMA expr RPAREN    
                          { $$ = new RandomNodeExpr($3 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
    | ROUND LPAREN expr COMMA expr RPAREN 
                          { $$ = new RoundNodeExpr($3 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
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

    | LBRACKET expr_list RBRACKET { $$ = new ArrayNodeExpr($2 as List<ExpressionNodeExpr>); } /*[1,2,3] */
    | ID LBRACKET expr RBRACKET 
    { 
        // Cast $1 to string so the IdNodeExpr constructor accepts it
        string idName = (string)$1;
        var idExpr = new IdNodeExpr(idName); 
        
        $$ = new IndexNodeExpr(idExpr, $3 as ExpressionNodeExpr); 
    }
    | LPAREN expr RPAREN  { $$ = $2; }
    | expr DOT WHERE LPAREN expr LAMBDA expr RPAREN
    | LPAREN expr RPAREN

    ;

params
    : /* empty */ { $$ = new List<string>(); }
    | ID { var list = new List<string>(); list.Add((string)$1); $$ = list; }
    | params COMMA ID { var list = (List<string>)$1; list.Add((string)$3); $$ = list; }
    ;

args
    : /* empty */ { $$ = new List<ExpressionNodeExpr>(); }
    | expr { var list = new List<ExpressionNodeExpr>(); list.Add($1 as ExpressionNodeExpr); $$ = list; }
    | args COMMA expr { var list = (List<ExpressionNodeExpr>)$1; list.Add($3 as ExpressionNodeExpr); $$ = list; }
    ;
%%

internal Parser(Scanner s) : base(s) { }