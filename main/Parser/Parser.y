%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public object obj; 
    public bool boolVal;
    public double fval;
    public MyCompiler.Node node; 
    public MyCompiler.ExpressionNode expr; 
    public List<ExpressionNode> exprList;
    public List<NamedArgumentNode> argList; 
}

%token <obj> NUMBER STRING ID NULL_LITERAL STRING_LITERAL
%token <boolVal> BOOL_LITERAL
%token <fval> FLOAT_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON COMMA DOT COLON LAMBDA NEWLINE COLUMNS
%token PLUS_ASSIGN MINUS_ASSIGN MULT_ASSIGN DIV_ASSIGN
%token LPAREN RPAREN LBRACE RBRACE LBRACKET RBRACKET IF ELSE FOR FOREACH IN INC DECR
%token PRINT RANDOM ROUND SQRT POW LOG EXP READCSV TOCSV 
%token REMOVE REMOVERANGE LENGTH MIN MAX MEAN SUM CORR COPY RECORD WHERE MAP ADD ADDRANGE 
%token DATAFRAME SELECT

%token INT FLOAT BOOL STRING VOID NULL ARRAY
%token GE LE EQ NE GT LT LOGICAL_AND LOGICAL_OR

%right ASSIGN PLUS_ASSIGN MINUS_ASSIGN MULT_ASSIGN DIV_ASSIGN
%nonassoc LOWEST
%nonassoc LOWER_THAN_LPAREN
%nonassoc UNARY
%nonassoc IF
%nonassoc ELSE

%left LOGICAL_OR
%left LOGICAL_AND
%left EQ NE
%left GT LT GE LE
%left PLUS MINUS
%left MULT DIV
%left DOT
%left LBRACKET 

%type <node> prog statement statement_list assignment block separator opt_newlines
%type <node> expr 
%type <expr> type
%type <expr> opt_expr
%type <exprList> expr_list 
%type <argList> arg_list
%type <node> arg

%{
    public MyCompiler.Node RootNode;
%}

%%

prog
    : statement_list { $$ = $1; RootNode = $$; }
    ;

block
    : LBRACE statement_list RBRACE { $$ = $2; }
    ;

separator
    : SEMICOLON
    | NEWLINE
    ;

opt_newlines
    : /* empty */
    | opt_newlines NEWLINE
    ;

statement_list
    : /* empty */                           { $$ = new SequenceNode(); }
    | statement_list statement              { ((SequenceNode)$1).Statements.Add($2); $$ = $1; }
    | statement_list separator              { $$ = $1; }
    ;

    

statement
    : assignment                                { $$ = $1; }
    | expr %prec LOWEST                         { $$ = $1; } 
    | IF LPAREN expr RPAREN block %prec IF      { $$ = new IfNode($3 as ExpressionNode, $5); }
    | IF LPAREN expr RPAREN block ELSE block    { $$ = new IfNode($3 as ExpressionNode, $5, $7); }
    | FOR LPAREN assignment SEMICOLON expr SEMICOLON assignment RPAREN opt_newlines block
    {
        $$ = new ForLoopNode($3 as StatementNode, $5 as ExpressionNode, $7 as StatementNode, $10);
    }        
    | FOREACH LPAREN ID IN expr RPAREN opt_newlines block
    {
        $$ = new ForEachLoopNode(new IdNode((string)$3), $5 as ExpressionNode, $8 );
    }
    ;

type
    : INT                 { $$ = new TypeNode("int"); }
    | FLOAT               { $$ = new TypeNode("float"); }
    | BOOL                { $$ = new TypeNode("bool"); }
    | STRING              { $$ = new TypeNode("string"); }   
    | VOID                { $$ = new TypeNode("void"); }    /* We currently do not use VOID! */
    | NULL                { $$ = new TypeNode("null"); }    /* We currently do not use NULL! */
    | ARRAY LT type GT    { $$ = new TypeNode("array"); }   /* We currently do not use typed array! */
    ;

assignment 
    : ID ASSIGN expr      { $$ = new AssignNode((string)$1, $3 as ExpressionNode); } 
    | ID PLUS_ASSIGN expr
    { 
        var id = new IdNode((string)$1);
        var add = new BinaryOpNode(id, "+", $3 as ExpressionNode);
        $$ = new AssignNode((string)$1, add); 
    }
    | expr DOT ID PLUS_ASSIGN expr
    {
        var left = new FieldNode($1 as ExpressionNode, (string)$3);
        var add = new BinaryOpNode(left, "+", $5 as ExpressionNode);
        $$ = new RecordFieldAssignNode($1 as ExpressionNode, (string)$3, add);
    }
    | ID MINUS_ASSIGN expr
    { 
        var id = new IdNode((string)$1);
        var sub = new BinaryOpNode(id, "-", $3 as ExpressionNode);
        $$ = new AssignNode((string)$1, sub); 
    }
    | expr DOT ID MINUS_ASSIGN expr
    {
        var left = new FieldNode($1 as ExpressionNode, (string)$3);
        var sub = new BinaryOpNode(left, "-", $5 as ExpressionNode);
        $$ = new RecordFieldAssignNode($1 as ExpressionNode, (string)$3, sub);
    }
    | ID MULT_ASSIGN expr
    {
        var id = new IdNode((string)$1);
        var mul = new BinaryOpNode(id, "*", $3 as ExpressionNode);
        $$ = new AssignNode((string)$1, mul);
    }

    | ID DIV_ASSIGN expr
    {
        var id = new IdNode((string)$1);
        var div = new BinaryOpNode(id, "/", $3 as ExpressionNode);
        $$ = new AssignNode((string)$1, div);
    }
    | ID INC              { $$ = new IncrementNode(new IdNode((string)$1)); }
    | ID DECR             { $$ = new DecrementNode(new IdNode((string)$1)); }
    | expr LBRACKET expr RBRACKET ASSIGN expr    
    { 
        $$ = new IndexAssignNode($1 as ExpressionNode, $3 as ExpressionNode, $6 as ExpressionNode);
    }
    | expr DOT ID ASSIGN expr
    {
        $$ = new RecordFieldAssignNode($1 as ExpressionNode, (string)$3, $5 as ExpressionNode);
    }
    ;


expr
    : BOOL_LITERAL         { $$ = new BooleanNode((bool)$1); }
    | NUMBER               { $$ = new NumberNode((int)$1); }
    | FLOAT_LITERAL        { $$ = new FloatNode($1); }
    | STRING_LITERAL       { $$ = new StringNode((string)$1); }
    | NULL_LITERAL         { $$ = new NullNode(); }      /* We currently do not use NULL_LITERAL! */
    | ID                   { $$ = new IdNode((string)$1); }
    | type                 { $$ = new TypeLiteralNode($1 as TypeNode); }

    | LPAREN expr RPAREN   { $$ = $2; }    /* ( 2+2 ) */
    | LBRACKET expr_list RBRACKET { $$ = new ArrayNode($2 as List<ExpressionNode>); } /*[1,2,3] */
    | LBRACE arg_list RBRACE { $$ = new RecordNode($2 as List<NamedArgumentNode>); }  /*{ name: "Bob", age: 23 } */

    /* Under your expr rules */
    | expr LBRACKET opt_expr COLON opt_expr RBRACKET { $$ = new SliceNode($1 as ExpressionNode, $3, $5); }


    /* Built-ins */
    | PRINT LPAREN expr RPAREN                      { $$ = new PrintNode($3 as ExpressionNode); }
    | RANDOM LPAREN expr_list RPAREN                { $$ = new RandomNode($3); }
    | ROUND LPAREN expr_list RPAREN                 { $$ = new RoundNode($3); }
    | READCSV LPAREN expr COMMA expr RPAREN         { $$ = new ReadCsvNode(new List<ExpressionNode>{$3 as ExpressionNode, $5 as ExpressionNode}); }
    | READCSV LPAREN expr RPAREN                    { $$ = new ReadCsvNode(new List<ExpressionNode>{$3 as ExpressionNode}); }
    | TOCSV LPAREN expr COMMA expr RPAREN           { $$ = new ToCsvNode($3 as ExpressionNode, $5 as ExpressionNode); }
    | DATAFRAME LPAREN arg_list RPAREN              { $$ = new DataframeNode($3); }
    | RECORD LPAREN LBRACE arg_list RBRACE RPAREN   { $$ = new RecordNode($4); }
    
    /* Math & Logic */
    | SQRT LPAREN expr RPAREN               { $$ = new SqrtNode($3 as ExpressionNode); }
    | EXP LPAREN expr RPAREN                { $$ = new ExponentialMathFuncNode($3 as ExpressionNode); }
    | POW LPAREN expr COMMA expr RPAREN     { $$ = new PowNode($3 as ExpressionNode, $5 as ExpressionNode); }
    | LOG LPAREN expr RPAREN                { $$ = new LogNode($3 as ExpressionNode); }
    | expr LOGICAL_AND expr                 { $$ = new LogicalOpNode($1 as ExpressionNode, "&&", $3 as ExpressionNode); }
    | expr LOGICAL_OR expr                  { $$ = new LogicalOpNode($1 as ExpressionNode, "||", $3 as ExpressionNode); }
    | MINUS expr %prec UNARY                { $$ = new UnaryOpNode("-", $2 as ExpressionNode); }
    | expr PLUS expr                        { $$ = new BinaryOpNode($1 as ExpressionNode, "+", $3 as ExpressionNode); }
    | expr MINUS expr                       { $$ = new BinaryOpNode($1 as ExpressionNode, "-", $3 as ExpressionNode); }
    | expr MULT expr                        { $$ = new BinaryOpNode($1 as ExpressionNode, "*", $3 as ExpressionNode); }
    | expr DIV expr                         { $$ = new BinaryOpNode($1 as ExpressionNode, "/", $3 as ExpressionNode); }
    | expr GE expr                          { $$ = new ComparisonNode($1 as ExpressionNode, ">=", $3 as ExpressionNode); }
    | expr LE expr                          { $$ = new ComparisonNode($1 as ExpressionNode, "<=", $3 as ExpressionNode); }
    | expr EQ expr                          { $$ = new ComparisonNode($1 as ExpressionNode, "==", $3 as ExpressionNode); }
    | expr NE expr                          { $$ = new ComparisonNode($1 as ExpressionNode, "!=", $3 as ExpressionNode); }
    | expr GT expr                          { $$ = new ComparisonNode($1 as ExpressionNode, ">", $3 as ExpressionNode); }
    | expr LT expr                          { $$ = new ComparisonNode($1 as ExpressionNode, "<", $3 as ExpressionNode); }

    /* Accessors */
    | expr LBRACKET expr RBRACKET           { $$ = new IndexNode($1 as ExpressionNode, $3 as ExpressionNode); }
    | expr DOT ID %prec LOWER_THAN_LPAREN   { $$ = new FieldNode($1 as ExpressionNode, (string)$3); }
    | expr DOT LENGTH                       { $$ = new LengthNode($1 as ExpressionNode); }
    | expr DOT MIN                          { $$ = new MinNode($1 as ExpressionNode); }
    | expr DOT MAX                          { $$ = new MaxNode($1 as ExpressionNode); }
    | expr DOT MEAN                         { $$ = new MeanNode($1 as ExpressionNode); }
    | expr DOT SUM                          { $$ = new SumNode($1 as ExpressionNode); }
    | expr DOT COPY                         { $$ = new CopyNode($1 as ExpressionNode); }
    | expr DOT COLUMNS                      { $$ = new ColumnsNode($1 as ExpressionNode); }
    | expr DOT CORR LPAREN expr RPAREN      { $$ = new CorrelationNode($1 as ExpressionNode, $5 as ExpressionNode); }

    /* Functional methods */
    | expr DOT ADD LPAREN expr RPAREN       { $$ = new AddNode($1 as ExpressionNode, $5 as ExpressionNode); }
    | expr DOT ADDRANGE LPAREN expr RPAREN  { $$ = new AddRangeNode($1 as ExpressionNode, $5 as ExpressionNode); }
    | expr DOT REMOVE LPAREN expr RPAREN    { $$ = new RemoveNode($1 as ExpressionNode, $5 as ExpressionNode); }
    | expr DOT REMOVERANGE LPAREN expr RPAREN { $$ = new RemoveRangeNode($1 as ExpressionNode, $5 as ExpressionNode); }
    | expr DOT WHERE LPAREN ID LAMBDA expr RPAREN
    {
        $$ = new WhereNode(new IdNode((string)$5), $1 as ExpressionNode, $7 as ExpressionNode); 
    }
    | expr DOT MAP LPAREN ID LAMBDA expr_list RPAREN  
    {
        $$ = new MapNode(new IdNode((string)$5), $1 as ExpressionNode, $7);
    }
    | expr DOT SELECT LPAREN expr_list RPAREN
    {
        $$ = new MapNode(new IdNode("__show_x"), $1 as ExpressionNode, new List<ExpressionNode> { new ArrayNode($5) });
    }
    ;
    

/* Helper for optional expressions */
opt_expr
    : /* empty */ { $$ = null; }
    | expr        { $$ = $1 as ExpressionNode; }
    ;

expr_list
    : /* empty*/             { $$ = new List<ExpressionNode>(); }
    | expr                   { $$ = new List<ExpressionNode> { $1 as ExpressionNode }; }
    | expr_list COMMA expr   { $1.Add($3 as ExpressionNode); $$ = $1; }
    ;

arg_list
    : arg                    { $$ = new List<NamedArgumentNode> { $1 as NamedArgumentNode }; }
    | arg_list COMMA arg     { $1.Add($3 as NamedArgumentNode); $$ = $1; }
    ;

arg
    : ID ASSIGN expr         { $$ = new NamedArgumentNode((string)$1, $3 as ExpressionNode); }
    | ID COLON expr          { $$ = new NamedArgumentNode((string)$1, $3 as ExpressionNode); }
    /* --- Explicitly handle type assignments like name: string --- */
    /* | ID ASSIGN type         { $$ = new NamedArgumentNode((string)$1, new TypeLiteralNode($3 as TypeNode)); }*/
    /* | ID COLON type          { $$ = new NamedArgumentNode((string)$1, new TypeLiteralNode($3 as TypeNode)); }*/
    /* --- Re-introduced fallback rule for raw positional expressions {"Alice", 25} --- */
    | expr                   { $$ = new NamedArgumentNode(null, $1 as ExpressionNode); } 
    ;

%%

internal Parser(Scanner s) : base(s) { }