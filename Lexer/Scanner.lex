%namespace MyCompiler
%scannertype Scanner
%visibility internal
%tokentype Tokens

%%

"#".*          { /* skip comment */ }

"if"            { return (int)Tokens.IF; }
"else"          { return (int)Tokens.ELSE; }
"true"          { yylval.boolVal = true; return (int)Tokens.BOOL_LITERAL; }
"false"         { yylval.boolVal = false; return (int)Tokens.BOOL_LITERAL; }
"print"         { return (int)Tokens.PRINT; }
"random"        { return (int)Tokens.RANDOM; }
"for"           { return (int)Tokens.FOR; }

">="            { return (int)Tokens.GE; }
"<="            { return (int)Tokens.LE; }
"=="            { return (int)Tokens.EQ; }
"!="            { return (int)Tokens.NE; }
">"             { return (int)Tokens.GT; }
"<"             { return (int)Tokens.LT; }
"++"            { return (int)Tokens.INC; }
"--"            { return (int)Tokens.DECR; }

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
";"             { return (int)Tokens.SEMICOLON; }
","             { return (int)Tokens.COMMA; }

[ \t\r\n]       { /* skip */ }
.               { return (int)Tokens.error; }

%%