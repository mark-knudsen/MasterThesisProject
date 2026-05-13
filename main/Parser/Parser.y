%namespace MyCompiler
%output=Parser.cs
%visibility internal

%union { 
    public object obj; 
    public bool boolVal;
    public double fval;
    public MyCompiler.Node node; 
    public MyCompiler.ExpressionNode expr; 
    public System.Collections.Generic.List<MyCompiler.NamedArgumentNode> unifiedList; 
}

%token <obj> NUMBER STRING ID NULL_LITERAL STRING_LITERAL
%token <boolVal> BOOL_LITERAL
%token <fval> FLOAT_LITERAL
%token PLUS MINUS MULT DIV ASSIGN SEMICOLON COMMA DOT COLON LAMBDA NEWLINE COLUMNS
%token PLUS_ASSIGN MINUS_ASSIGN
%token LPAREN RPAREN LBRACE RBRACE LBRACKET RBRACKET IF ELSE FOR FOREACH IN INC DECR
%token PRINT RANDOM ROUND SQRT POW LOG EXP READCSV TOCSV 
%token REMOVE REMOVERANGE LENGTH MIN MAX MEAN SUM CORR COPY RECORD WHERE MAP FUNC ADD ADDRANGE 
%token DATAFRAME SELECT SHOW

%token INT FLOAT BOOL STRING VOID NULL ARRAY
%token GE LE EQ NE GT LT LOGICAL_AND LOGICAL_OR

%right ASSIGN PLUS_ASSIGN MINUS_ASSIGN
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
%left LBRACKET

%type <node> prog statement statement_list assignment block seperator opt_newlines
%type <node> expr primary_expr
%type <expr> type
%type <unifiedList> unified_list
%type <node> unified_element

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

seperator
    : SEMICOLON
    | NEWLINE
    | SEMICOLON NEWLINE
    | NEWLINE SEMICOLON
    ;

opt_newlines
    : /* empty */
    | opt_newlines NEWLINE
    ;

statement_list
    : /* empty */ { $$ = new SequenceNode(); }
    | statement_list statement { ((SequenceNode)$1).Statements.Add($2); $$ = $1; }
    | statement_list seperator { $$ = $1; }
    ;

statement
    : assignment                             { $$ = $1; }
    | expr                                   { $$ = $1; } 
    | block                                  { $$ = $1; }
    | IF LPAREN expr RPAREN block %prec IF    { $$ = new IfNode($3 as ExpressionNode, $5); }
    | IF LPAREN expr RPAREN block ELSE block  { $$ = new IfNode($3 as ExpressionNode, $5, $7); }
    | FOR LPAREN assignment SEMICOLON expr SEMICOLON assignment RPAREN opt_newlines block
      { $$ = new ForLoopNode($3 as StatementNode, $5 as ExpressionNode, $7 as StatementNode, $10); }
    | FOREACH LPAREN ID IN expr RPAREN opt_newlines block
        { $$ = new ForEachLoopNode(new IdNode((string)$3), $5 as ExpressionNode, $8 ); }
    ;

type
    : INT                 { $$ = new TypeNode("int"); }
    | FLOAT               { $$ = new TypeNode("float"); }
    | BOOL                { $$ = new TypeNode("bool"); }
    | STRING              { $$ = new TypeNode("string"); }   
    | VOID                { $$ = new TypeNode("void"); }
    | NULL                { $$ = new TypeNode("null"); }
    | ARRAY GT type LT    { $$ = new TypeNode("array"); }
    ;

assignment 
    : ID ASSIGN expr      { $$ = new AssignNode((string)$1, $3 as ExpressionNode); } 
    | ID PLUS_ASSIGN expr { 
        var id = new IdNode((string)$1);
        var add = new BinaryOpNode(id, "+", $3 as ExpressionNode);
        $$ = new AssignNode((string)$1, add); 
    }
    | expr DOT ID PLUS_ASSIGN expr {
        var left = new FieldNode($1 as ExpressionNode, (string)$3);
        var add = new BinaryOpNode(left, "+", $5 as ExpressionNode);
        $$ = new RecordFieldAssignNode($1 as ExpressionNode, (string)$3, add);
    }
    | ID MINUS_ASSIGN expr { 
        var id = new IdNode((string)$1);
        var sub = new BinaryOpNode(id, "-", $3 as ExpressionNode);
        $$ = new AssignNode((string)$1, sub); 
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

unified_list
    : unified_element { $$ = new System.Collections.Generic.List<NamedArgumentNode> { $1 as NamedArgumentNode }; }
    | unified_list COMMA unified_element { $1.Add($3 as NamedArgumentNode); $$ = $1; }
    ;

unified_element
    : ID ASSIGN expr      { $$ = new NamedArgumentNode((string)$1, $3 as ExpressionNode); }
    | ID COLON expr       { $$ = new NamedArgumentNode((string)$1, $3 as ExpressionNode); }
    | COLUMNS ASSIGN expr { $$ = new NamedArgumentNode("columns", $3 as ExpressionNode); }
    | COLUMNS COLON expr  { $$ = new NamedArgumentNode("columns", $3 as ExpressionNode); }
    | expr                { $$ = new NamedArgumentNode(null, $1 as ExpressionNode); } 
    ;

primary_expr
    : BOOL_LITERAL         { $$ = new BooleanNode((bool)$1); }
    | NUMBER               { $$ = new NumberNode((int)$1); }
    | FLOAT_LITERAL        { $$ = new FloatNode($1); }
    | STRING_LITERAL       { $$ = new StringNode((string)$1); }
    | NULL_LITERAL         { $$ = new NullNode(); }
    | ID                   { $$ = new IdNode((string)$1); }
    | type                 { $$ = new TypeLiteralNode($1 as TypeNode); }
    | LPAREN expr RPAREN   { $$ = $2; }
    | LBRACE unified_list RBRACE { $$ = new RecordNode($2); }
    | PRINT LPAREN expr RPAREN          { $$ = new PrintNode($3 as ExpressionNode); }
    | RANDOM LPAREN unified_list RPAREN { $$ = new RandomNode(System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select($3, x => x.Value))); }
    | ROUND LPAREN unified_list RPAREN  { $$ = new RoundNode($3[0].Value, $3[1].Value); }
    | READCSV LPAREN unified_list RPAREN { $$ = new ReadCsvNode(System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select($3, x => x.Value))); }
    | DATAFRAME LPAREN unified_list RPAREN { $$ = new DataframeNode($3); }
    | RECORD LPAREN LBRACE unified_list RBRACE RPAREN { $$ = new RecordNode($4); }
    | SQRT LPAREN expr RPAREN              { $$ = new SqrtNode($3 as ExpressionNode); }
    | POW LPAREN expr COMMA expr RPAREN   { $$ = new PowNode($3 as ExpressionNode, $5 as ExpressionNode); }
    ;

expr
    : primary_expr        { $$ = $1; }
    | MINUS expr          { $$ = new UnaryOpNode("-", $2 as ExpressionNode); }
    | expr PLUS expr      { $$ = new BinaryOpNode($1 as ExpressionNode, "+", $3 as ExpressionNode); }
    | expr MINUS expr     { $$ = new BinaryOpNode($1 as ExpressionNode, "-", $3 as ExpressionNode); }
    | expr MULT expr      { $$ = new BinaryOpNode($1 as ExpressionNode, "*", $3 as ExpressionNode); }
    | expr DIV expr       { $$ = new BinaryOpNode($1 as ExpressionNode, "/", $3 as ExpressionNode); }
    | expr GE expr        { $$ = new ComparisonNode($1 as ExpressionNode, ">=", $3 as ExpressionNode); }    
    | expr LE expr        { $$ = new ComparisonNode($1 as ExpressionNode, "<=", $3 as ExpressionNode); }
    | expr EQ expr        { $$ = new ComparisonNode($1 as ExpressionNode, "==", $3 as ExpressionNode); }    
    | expr NE expr        { $$ = new ComparisonNode($1 as ExpressionNode, "!=", $3 as ExpressionNode); }
    | expr GT expr        { $$ = new ComparisonNode($1 as ExpressionNode, ">", $3 as ExpressionNode); }    
    | expr LT expr        { $$ = new ComparisonNode($1 as ExpressionNode, "<", $3 as ExpressionNode); }
    | LBRACKET unified_list RBRACKET { 
        if (System.Linq.Enumerable.All($2, x => x.Name == null)) 
            $$ = new ArrayNode(System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select($2, x => x.Value)));
        else 
            $$ = new RecordNode($2);
    }
    | expr LBRACKET expr RBRACKET    { $$ = new IndexNode($1 as ExpressionNode, $3 as ExpressionNode); }
    | expr DOT ID %prec LOWER_THAN_LPAREN { $$ = new FieldNode($1 as ExpressionNode, (string)$3); }
    | expr DOT LENGTH                { $$ = new LengthNode($1 as ExpressionNode); }
    | expr DOT SUM                   { $$ = new SumNode($1 as ExpressionNode); }
    
    | expr DOT WHERE LPAREN ID LAMBDA expr RPAREN
    { $$ = new WhereNode(new IdNode((string)$5), $1 as ExpressionNode, $7 as ExpressionNode); }

    | expr DOT MAP LPAREN ID LAMBDA expr RPAREN
    { 
        var assignments = new System.Collections.Generic.List<ExpressionNode> { $7 as ExpressionNode };
        $$ = new MapNode(new IdNode((string)$5), $1 as ExpressionNode, assignments); 
    }
    
    | expr DOT MAP LPAREN ID LAMBDA LPAREN unified_list RPAREN RPAREN
    {
        var assignments = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select($8, x => x.Value));
        $$ = new MapNode(new IdNode((string)$5), $1 as ExpressionNode, assignments);
    }
    ;

%%

internal Parser(Scanner s) : base(s) { }