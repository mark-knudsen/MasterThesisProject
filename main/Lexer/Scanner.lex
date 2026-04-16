%namespace MyCompiler
%scannertype Scanner
%visibility internal
%tokentype Tokens

%%

"#".*          { /* skip comment */ }

"&"             { return (int)Tokens.LOGICAL_AND; } 
"and"           { return (int)Tokens.LOGICAL_AND; } 
"|"             { return (int)Tokens.LOGICAL_OR; } 
"or"            { return (int)Tokens.LOGICAL_OR; }  

"if"            { return (int)Tokens.IF; }
"else"          { return (int)Tokens.ELSE; }
"true"          { yylval.boolVal = true; return (int)Tokens.BOOL_LITERAL; }
"false"         { yylval.boolVal = false; return (int)Tokens.BOOL_LITERAL; }
"NULL"          { return (int)Tokens.NULL_LITERAL; }
"print"         { return (int)Tokens.PRINT; }
"random"        { return (int)Tokens.RANDOM; }
"round"         { return (int)Tokens.ROUND; }
"func"          { return (int)Tokens.FUNC; }
"for"           { return (int)Tokens.FOR; }
"foreach"       { return (int)Tokens.FOREACH; }
"in"            { return (int)Tokens.IN; }

"int"           { return (int)Tokens.INT; }
"float"         { return (int)Tokens.FLOAT; }
"string"        { return (int)Tokens.STRING; }
"void"          { return (int)Tokens.VOID; }
"null"          { return (int)Tokens.NULL; }
"bool"          { return (int)Tokens.BOOL; }
"array"         { return (int)Tokens.ARRAY; }

"copy"          { return (int)Tokens.COPY; }
"where"         { return (int)Tokens.WHERE; }
"map"           { return (int)Tokens.MAP; }
"=>"            { return (int)Tokens.LAMBDA; }
"add"           { return (int)Tokens.ADD; }
"addRange"      { return (int)Tokens.ADDRANGE; }
"remove"        { return (int)Tokens.REMOVE; }
"removeRange"   { return (int)Tokens.REMOVERANGE; }
"length"        { return (int)Tokens.LENGTH; }
"read_csv"      { return (int)Tokens.READCSV; }
"to_csv"        { return (int)Tokens.TOCSV; }
"min"           { return (int)Tokens.MIN; }
"max"           { return (int)Tokens.MAX; }
"mean"          { return (int)Tokens.MEAN; }
"sum"           { return (int)Tokens.SUM; }

"record"        { return (int)Tokens.RECORD; }
"dataframe"     { return (int)Tokens.DATAFRAME; }
"columns"       { return (int)Tokens.COLUMNS; }
"show"          { return (int)Tokens.SHOW; }

">="            { return (int)Tokens.GE; }
"<="            { return (int)Tokens.LE; }
"=="            { return (int)Tokens.EQ; }
"!="            { return (int)Tokens.NE; }
">"             { return (int)Tokens.GT; }
"<"             { return (int)Tokens.LT; }
"++"            { return (int)Tokens.INC; }
"--"            { return (int)Tokens.DECR; }
"+="            { return (int)Tokens.PLUS_ASSIGN; }
"-="            { return (int)Tokens.MINUS_ASSIGN; }

[0-9]+          { yylval.obj = int.Parse(yytext); return (int)Tokens.NUMBER; }
[0-9]+\.[0-9]+  { yylval.fval = double.Parse(yytext, CultureInfo.InvariantCulture); return (int)Tokens.FLOAT_LITERAL; }
\"[^\"]*\"      { yylval.obj = yytext.Trim('"'); return (int)Tokens.STRING_LITERAL; }
[a-zA-Z_][a-zA-Z0-9_\-]* { yylval.obj = yytext; return (int)Tokens.ID; }    

"+"             { return (int)Tokens.PLUS; }
"-"             { return (int)Tokens.MINUS; }
"*"             { return (int)Tokens.MULT; }
"/"             { return (int)Tokens.DIV; }
"="             { return (int)Tokens.ASSIGN; }
"("             { return (int)Tokens.LPAREN; }
")"             { return (int)Tokens.RPAREN; }
"{"             { return (int)Tokens.LBRACE; }
"}"             { return (int)Tokens.RBRACE; }
"["             { return (int)Tokens.LBRACKET; }
"]"             { return (int)Tokens.RBRACKET; }
":"             { return (int)Tokens.COLON; }
";"             { return (int)Tokens.SEMICOLON; }
","             { return (int)Tokens.COMMA; }
"."             { return (int)Tokens.DOT; }

\r\n            { return (int)Tokens.NEWLINE; }   // Windows
\n              { return (int)Tokens.NEWLINE; }   // Unix
\r              { return (int)Tokens.NEWLINE; }   // Old Mac (rare)

[ \t]+          { /* skip spaces/tabs only */ }
.               { return (int)Tokens.error; }

%%