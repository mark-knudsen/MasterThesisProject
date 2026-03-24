%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public object obj; 
    public bool boolVal;
    public double fval;
    public MyCompiler.Type type; 
    public MyCompiler.NodeExpr node; // Add this to hold AST pieces
    public List<MyCompiler.ExpressionNodeExpr> exprList; // for expr_list
}

%token <obj> NUMBER STRING ID
%token <boolVal> BOOL_LITERAL
%token <fval> FLOAT_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON COMMA DOT COLON LAMBDA
%token PLUS_ASSIGN MINUS_ASSIGN
%token LPAREN RPAREN LBRACE RBRACE LBRACKET RBRACKET IF ELSE FOR FOREACH IN INC DECR
%token PRINT RANDOM ROUND WHERE MAP FUNC ADD ADDRANGE REMOVE REMOVERANGE LENGTH MIN MAX MEAN SUM READCSV COPY RECORD ADDFIELD REMOVEFIELD

%token INT FLOAT BOOL STRING VOID ARRAY

%token GE LE EQ NE GT LT LOGICAL_AND LOGICAL_OR

%nonassoc LOWER_THAN_LPAREN
%nonassoc IF
%nonassoc ELSE

%left LOGICAL_OR
%left LOGICAL_AND
%left EQ NE
%left GT LT GE LE
%left PLUS MINUS
%left MULT DIV
%left DOT
%left LBRACKET /* Add this to give indexing high priority */

%type <node> Prog Statement StatementList Assignment
%type <node> expr
%type <type> Type
%type <obj> params args  /* Use <obj> for lists */
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
    
    | FOREACH LPAREN ID IN expr RPAREN LBRACE StatementList RBRACE
        { $$ = new ForEachLoopNodeExpr(new IdNodeExpr((string)$3), $5 as ExpressionNodeExpr, $8 ); }
    ;

Type
    : INT                 { $$ = new IntType(); }
    | FLOAT               { $$ = new FloatType(); }
    | BOOL                { $$ = new BoolType(); }
    | STRING              { $$ = new StringType(); }    
    | VOID                { $$ = new VoidType(); }
    | ARRAY GT Type LT    { $$ = new ArrayType($3); }
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
    
    | expr DOT ID ASSIGN expr
    {
        $$ = new RecordFieldAssignNodeExpr(
            $1 as ExpressionNodeExpr,
            (string)$3,
            $5 as ExpressionNodeExpr
        );
    }
    ;

expr_list
    : expr                     { $$ = new List<ExpressionNodeExpr> { $1 as ExpressionNodeExpr }; }
    | expr_list COMMA expr     { $1.Add($3 as ExpressionNodeExpr); $$ = $1; }
    ;

expr
    : BOOL_LITERAL        { $$ = new BooleanNodeExpr((bool)$1); }
    | NUMBER              { $$ = new NumberNodeExpr((int)$1); }
    | FLOAT_LITERAL       { $$ = new FloatNodeExpr($1); }
    | MINUS expr          { $$ = new UnaryOpNodeExpr("-", $2 as ExpressionNodeExpr); }
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

    | expr LOGICAL_AND expr { $$ = new LogicalOpNodeExpr($1 as ExpressionNodeExpr, "&&", $3 as ExpressionNodeExpr); }  
    | expr LOGICAL_OR expr  { $$ = new LogicalOpNodeExpr($1 as ExpressionNodeExpr, "||", $3 as ExpressionNodeExpr); }  

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
    | expr DOT ADD LPAREN expr RPAREN       { $$ = new AddNodeExpr($1 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
    | expr DOT ADDRANGE LPAREN expr RPAREN  { $$ = new AddRangeNodeExpr($1 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
    | expr DOT REMOVE LPAREN expr RPAREN    { $$ = new RemoveNodeExpr($1 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
    | expr DOT REMOVERANGE LPAREN expr RPAREN    { $$ = new RemoveRangeNodeExpr($1 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }
    | expr DOT LENGTH                       { $$ = new LengthNodeExpr($1 as ExpressionNodeExpr); }
    | expr DOT MIN                          { $$ = new MinNodeExpr($1 as ExpressionNodeExpr); }
    | expr DOT MAX                          { $$ = new MaxNodeExpr($1 as ExpressionNodeExpr); }
    | expr DOT MEAN                         { $$ = new MeanNodeExpr($1 as ExpressionNodeExpr); }
    | expr DOT SUM                          { $$ = new SumNodeExpr($1 as ExpressionNodeExpr); }

    | expr DOT WHERE LPAREN ID LAMBDA expr RPAREN
    {
        $$ = new WhereNodeExpr(
            new IdNodeExpr((string)$5),
            $1 as ExpressionNodeExpr,
            $7 as ExpressionNodeExpr
        );
    }
    | expr DOT MAP LPAREN ID LAMBDA expr RPAREN
    {
        $$ = new MapNodeExpr(
            new IdNodeExpr((string)$5),
            $1 as ExpressionNodeExpr,
            $7 as ExpressionNodeExpr
        );
    }
   // | expr DOT COPY                          { $$ = new CopyArrayNodeExpr($1 as ExpressionNodeExpr); }

    | expr DOT READCSV LPAREN expr RPAREN    { $$ = new ReadCsvNodeExpr($1 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }

    | RECORD LPAREN expr COMMA expr RPAREN   { $$ = new RecordNodeExpr($3 as ExpressionNodeExpr, $5 as ExpressionNodeExpr); }

    | expr DOT ID %prec LOWER_THAN_LPAREN
    {
        $$ = new RecordFieldNodeExpr($1 as ExpressionNodeExpr, (string)$3);
    }

   // | expr DOT COPY                           { $$ = new CopyRecordNodeExpr($1 as ExpressionNodeExpr); }
    | expr DOT COPY                           { $$ = new CopyNodeExpr($1 as ExpressionNodeExpr); }
    | expr DOT ADDFIELD LPAREN ID COMMA expr RPAREN
                                              { $$ = new AddFieldNodeExpr($1 as ExpressionNodeExpr, (string)$5, $7 as ExpressionNodeExpr); }
    | expr DOT REMOVEFIELD LPAREN ID RPAREN  { $$ = new RemoveFieldNodeExpr($1 as ExpressionNodeExpr, (string)$5); }
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