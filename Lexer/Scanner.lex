%namespace MyCompiler
%scannertype Scanner
%visibility internal
%tokentype Tokens

%%

"if"            { return (int)Tokens.IF; }
"else"          { return (int)Tokens.ELSE; }
"true"          { yylval.boolVal = true; return (int)Tokens.BOOL_LITERAL; }
"false"         { yylval.boolVal = false; return (int)Tokens.BOOL_LITERAL; }


">="            { return (int)Tokens.GE; }
"<="            { return (int)Tokens.LE; }
"=="            { return (int)Tokens.EQ; }
"!="            { return (int)Tokens.NE; }
">"             { return (int)Tokens.GT; }
"<"             { return (int)Tokens.LT; }

[0-9]+          { yylval.obj = int.Parse(yytext); return (int)Tokens.NUMBER; }
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

[ \t\r\n]       { /* skip */ }
.               { return (int)Tokens.error; }

%%