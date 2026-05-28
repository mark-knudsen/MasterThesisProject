%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public bool boolVal;
    public double fval;
    public string strval;
    public int intval;
    public MyCompiler.Node node; 
    public MyCompiler.ExpressionNode expr;
    public List<ExpressionNode> idList;
    public List<ExpressionNode> exprList;
    public List<NamedArgumentNode> dataframeArgList;
    public List<NamedArgumentNode> recordArgList; 
}

%token NULL_LITERAL
%token <intval> NUMBER
%token <strval> STRING_LITERAL ID
%token <boolVal> BOOL_LITERAL
%token <fval> FLOAT_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON COMMA DOT
%token COLON LAMBDA NEWLINE COLUMNS
%token PLUS_ASSIGN MINUS_ASSIGN MULT_ASSIGN DIV_ASSIGN
%token LPAREN RPAREN LBRACE RBRACE LBRACKET RBRACKET
%token IF ELSE FOR FOREACH IN INC DECR
%token PRINT RANDOM ROUND SQRT POW LOG EXP READCSV TOCSV 
%token ADD ADDRANGE REMOVE REMOVERANGE LENGTH MIN MAX MEAN SUM
%token RECORD DATAFRAME SELECT COPY WHERE MAP CORR
%token INT FLOAT BOOL STRING VOID NULL ARRAY
%token GE LE EQ NE GT LT LOGICAL_AND LOGICAL_OR

%right ASSIGN PLUS_ASSIGN MINUS_ASSIGN MULT_ASSIGN DIV_ASSIGN
%nonassoc LOWEST
%nonassoc UNARY
/* %nonassoc IF %nonassoc ELSE */

%left LOGICAL_OR
%left LOGICAL_AND
%left EQ NE
%left GT LT GE LE
%left PLUS MINUS
%left MULT DIV
%left DOT
%left LBRACKET

%type <node> prog statement statement_list assignment block dataframe_arg record_arg
%type <expr> expr record_struct type opt_expr 
%type <idList> id_list
%type <exprList> expr_list 
%type <dataframeArgList> dataframe_arg_list
%type <recordArgList> record_arg_list

/* %type <statement> statement assignment */
/* %type <recordList> record_list */

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
    : /* empty */               { $$ = new SequenceNode(); }
    | statement_list statement  { ((SequenceNode)$1).Statements.Add($2); $$ = $1; }
    | statement_list separator  { $$ = $1; }
    ;

statement
    : assignment                                { $$ = $1; }
    | expr %prec LOWEST                         { $$ = $1; } 
    | IF LPAREN expr RPAREN block %prec IF      { $$ = new IfNode($3, $5); }  
    | IF LPAREN expr RPAREN block ELSE block    { $$ = new IfNode($3, $5, $7); }
    | PRINT LPAREN expr RPAREN                  { $$ = new PrintNode($3); }
    | FOR LPAREN assignment SEMICOLON expr SEMICOLON assignment RPAREN opt_newlines block
    {
        $$ = new ForLoopNode($3 as StatementNode, $5, $7 as StatementNode, $10);
    }        
    | FOREACH LPAREN ID IN expr RPAREN opt_newlines block
    {
        $$ = new ForEachLoopNode(new IdNode($3), $5, $8 );
    }
    ;

type
    : INT                 { $$ = new TypeNode("int"); }
    | FLOAT               { $$ = new TypeNode("float"); }
    | BOOL                { $$ = new TypeNode("bool"); }
    | STRING              { $$ = new TypeNode("string"); }   
    | VOID                { $$ = new TypeNode("void"); }    /* We currently do not use VOID! */
    | NULL                { $$ = new TypeNode("null"); }    /* We currently do not use NULL! */
    | ARRAY LT type GT    { $$ = new TypeNode("array", $3 as TypeNode); }   /* We currently do not use typed array! */
    ;

assignment 
    : ID ASSIGN expr      { $$ = new AssignNode($1, $3); } 
    | ID PLUS_ASSIGN expr
    { 
        var id = new IdNode($1);
        var add = new BinaryOpNode(id, "+", $3);
        $$ = new AssignNode($1, add); 
    }
    | expr DOT ID PLUS_ASSIGN expr
    {
        var left = new FieldNode($1, $3);
        var add = new BinaryOpNode(left, "+", $5);
        $$ = new RecordFieldAssignNode($1, $3, add);
    }
    | ID MINUS_ASSIGN expr
    { 
        var id = new IdNode($1);
        var sub = new BinaryOpNode(id, "-", $3);
        $$ = new AssignNode($1, sub); 
    }
    | expr DOT ID MINUS_ASSIGN expr
    {
        var left = new FieldNode($1, $3);
        var sub = new BinaryOpNode(left, "-", $5);
        $$ = new RecordFieldAssignNode($1, $3, sub);
    }
    | ID MULT_ASSIGN expr
    {
        var id = new IdNode($1);
        var mul = new BinaryOpNode(id, "*", $3);
        $$ = new AssignNode($1, mul);
    }

    | ID DIV_ASSIGN expr
    {
        var id = new IdNode($1);
        var div = new BinaryOpNode(id, "/", $3);
        $$ = new AssignNode($1, div);
    }
    | ID INC              { $$ = new IncrementNode(new IdNode($1)); }
    | ID DECR             { $$ = new DecrementNode(new IdNode($1)); }

    | expr LBRACKET expr RBRACKET ASSIGN expr    { $$ = new IndexAssignNode($1, $3, $6); }
    | expr DOT ID ASSIGN expr                    { $$ = new RecordFieldAssignNode($1, $3, $5); }
    ;

expr
    : BOOL_LITERAL                  { $$ = new BooleanNode($1); }
    | NUMBER                        { $$ = new NumberNode($1); }
    | FLOAT_LITERAL                 { $$ = new FloatNode($1); }
    | STRING_LITERAL                { $$ = new StringNode($1); }
    | NULL_LITERAL                  { $$ = new NullNode(); }   /* We currently do not use NULL_LITERAL! */
    | ID                            { $$ = new IdNode($1); }

    | LPAREN expr RPAREN            { $$ = $2; }    /* ( 2+2 ) */
    | LBRACKET expr_list RBRACKET   { $$ = new ArrayNode($2); } /*[1,2,3] */    
    | type LBRACKET expr_list RBRACKET   { $$ = new ArrayNode($3, $1 as TypeNode); } /*[1,2,3] */    
    | expr LBRACKET opt_expr COLON opt_expr RBRACKET    { $$ = new SliceNode($1, $3, $5); }

    /* Built-ins */
    | RANDOM LPAREN expr_list RPAREN                    { $$ = new RandomNode($3); }
    | ROUND LPAREN expr_list RPAREN                     { $$ = new RoundNode($3); }
    | READCSV LPAREN expr COMMA record_arg RPAREN       { $$ = new ReadCsvNode(new List<Node>{$3, $5 as NamedArgumentNode}); }
    | READCSV LPAREN expr RPAREN                        { $$ = new ReadCsvNode(new List<Node>{$3}); }
    | TOCSV LPAREN expr COMMA expr RPAREN               { $$ = new ToCsvNode($3, $5); }
    | DATAFRAME LPAREN dataframe_arg_list RPAREN        { $$ = new DataframeNode($3); }
    | record_struct                                     { $$ = $1; }
    
    /* Math & Logic */
    | SQRT LPAREN expr RPAREN               { $$ = new SqrtNode($3); }
    | EXP LPAREN expr RPAREN                { $$ = new ExponentialMathFuncNode($3); }
    | POW LPAREN expr COMMA expr RPAREN     { $$ = new PowNode($3, $5); }
    | LOG LPAREN expr RPAREN                { $$ = new LogNode($3); }
    | expr LOGICAL_AND expr                 { $$ = new LogicalOpNode($1, "&&", $3); }
    | expr LOGICAL_OR expr                  { $$ = new LogicalOpNode($1, "||", $3); }
    | MINUS expr %prec UNARY                { $$ = new UnaryOpNode("-", $2); }
    | expr PLUS expr                        { $$ = new BinaryOpNode($1, "+", $3); }
    | expr MINUS expr                       { $$ = new BinaryOpNode($1, "-", $3); }
    | expr MULT expr                        { $$ = new BinaryOpNode($1, "*", $3); }
    | expr DIV expr                         { $$ = new BinaryOpNode($1, "/", $3); }
    | expr GE expr                          { $$ = new ComparisonNode($1, ">=", $3); }
    | expr LE expr                          { $$ = new ComparisonNode($1, "<=", $3); }
    | expr EQ expr                          { $$ = new ComparisonNode($1, "==", $3); }
    | expr NE expr                          { $$ = new ComparisonNode($1, "!=", $3); }
    | expr GT expr                          { $$ = new ComparisonNode($1, ">", $3); }
    | expr LT expr                          { $$ = new ComparisonNode($1, "<", $3); }

    /* Accessors */
    | expr LBRACKET expr RBRACKET           { $$ = new IndexNode($1, $3); }
    | expr DOT ID                           { $$ = new FieldNode($1, $3); }
    | expr DOT LENGTH                       { $$ = new LengthNode($1); }
    | expr DOT MIN                          { $$ = new MinNode($1); }
    | expr DOT MAX                          { $$ = new MaxNode($1); }
    | expr DOT MEAN                         { $$ = new MeanNode($1); }
    | expr DOT SUM                          { $$ = new SumNode($1); }
    | expr DOT COPY LPAREN RPAREN           { $$ = new CopyNode($1); }
    | expr DOT COLUMNS                      { $$ = new ColumnsNode($1); }

    /* Functional methods */
    | expr DOT ADD LPAREN expr RPAREN             { $$ = new AddNode($1, $5); }   
    | expr DOT ADDRANGE LPAREN expr RPAREN        { $$ = new AddRangeNode($1, $5); }
    | expr DOT REMOVE LPAREN expr RPAREN          { $$ = new RemoveNode($1, $5); }
    | expr DOT REMOVERANGE LPAREN expr RPAREN     { $$ = new RemoveRangeNode($1, $5); }
    | expr DOT CORR LPAREN expr RPAREN            { $$ = new CorrelationNode($1, $5); }

    | expr DOT WHERE LPAREN ID LAMBDA expr RPAREN { $$ = new WhereNode(new IdNode($5), $1, $7); }
    | expr DOT MAP LPAREN ID LAMBDA expr RPAREN   { $$ = new MapNode(new IdNode($5), $1, $7); }
    | expr DOT SELECT LPAREN id_list RPAREN
    {
        $$ = new MapNode(
            new IdNode("__show_x"),
            $1,
            new ArrayNode(
                $5 as List<ExpressionNode>
            )
        );
    }
    ;

id_list
    : ID                { $$ = new List<ExpressionNode>{ new IdNode($1) }; }
    | id_list COMMA ID  { $1.Add(new IdNode($3)); $$ = $1; }
    ;

record_struct
    : RECORD LPAREN LBRACE record_arg_list RBRACE RPAREN   { $$ = new RecordNode($4); }    /* record({...})  */
    | LBRACE record_arg_list RBRACE         { $$ = new RecordNode($2); }                   /* ID={...}   */
    ;

/* --- Dataframe Specific Arguments (Strictly Named mappings or columns) --- */
dataframe_arg_list
    : dataframe_arg                          { $$ = new List<NamedArgumentNode> { $1 as NamedArgumentNode }; }
    | dataframe_arg_list COMMA dataframe_arg { $1.Add($3 as NamedArgumentNode); $$ = $1; }
    ;

dataframe_arg
    : ID ASSIGN expr         { $$ = new NamedArgumentNode($1, $3); }
    | expr                   { $$ = new NamedArgumentNode(null, $1); } 
    ;

/* --- Record Specific Arguments (Allows positional structural values) --- */
record_arg_list
    : record_arg                         { $$ = new List<NamedArgumentNode> { $1 as NamedArgumentNode }; }
    | record_arg_list COMMA record_arg   { $1.Add($3 as NamedArgumentNode); $$ = $1; }
    ;

record_arg
    : ID ASSIGN expr         { $$ = new NamedArgumentNode($1, $3); }
    | ID COLON type          { $$ = new NamedArgumentNode($1, new TypeLiteralNode($3 as TypeNode)); }
    | expr                   { $$ = new NamedArgumentNode(null, $1); } 
    ;
 
/* Helper for optional expressions */
opt_expr
    : /* empty */ { $$ = null; }
    | expr        { $$ = $1; }
    ;

expr_list
    : /* empty*/             { $$ = new List<ExpressionNode>(); }
    | expr                   { $$ = new List<ExpressionNode> { $1 }; }
    | expr_list COMMA expr   { $1.Add($3); $$ = $1; }
    ;

%%

internal Parser(Scanner s) : base(s) { }