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
"print"         { return (int)Tokens.PRINT; }
"random"        { return (int)Tokens.RANDOM; }
"round"         { return (int)Tokens.ROUND; }
"func"          { return (int)Tokens.FUNC; }
"for"           { return (int)Tokens.FOR; }
"where"         { return (int)Tokens.WHERE; }
"=>"            { return (int)Tokens.LAMBDA; }
"add"           { return (int)Tokens.ADD; }
"remove"        { return (int)Tokens.REMOVE; }

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
\"[^\"]*\"      { yylval.obj = yytext.Trim('"'); return (int)Tokens.STRING; }
[a-zA-Z_][a-zA-Z0-9_]* { yylval.obj = yytext; return (int)Tokens.ID; }    

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

[ \t\r\n]       { /* skip */ }
.               { return (int)Tokens.error; }

%%