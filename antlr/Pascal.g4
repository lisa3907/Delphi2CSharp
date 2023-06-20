grammar Pascal;

options
{
    language = CSharp;
}

program: programHeading (variableDeclarationPart | (procedureDeclaration SEMI))* mainCompound DOT EOF;

mainCompound: compoundStatement;

programHeading: PROGRAM identifier SEMI;

identifier: IDENT;

block: variableDeclarationPart* compoundStatement;

unsignedInteger: NUM_INT;

string: STRING_LITERAL;

typeIdentifier: INTEGER | STRING;

variableDeclarationPart: VAR variableDeclaration SEMI;

variableDeclaration: identifierList COLON typeIdentifier;

procedureDeclaration: procedureHeader SEMI block;

procedureHeader: PROCEDURE name=identifier (LPAREN groups += parameterGroup (SEMI groups += parameterGroup)* RPAREN)?;

parameterGroup: varToken=VAR? vars += identifier (COMMA vars += identifier)* COLON type=typeIdentifier;

identifierList
   : identifier (COMMA identifier)*
   ;

statement
   : assignmentStatement
   | procedureStatement
   | emptyStatement
   | compoundStatement
   ;

assignmentStatement: assigningVariable ASSIGN expression;
assigningVariable: identifier;

expression returns[string s]: relationExpression { $s = $relationExpression.ctx.s; };
relationExpression returns[string s]: operands += addExpression (operations += (EQUAL | NOT_EQUAL | LT | LE | GE | GT) operands += addExpression)*;

addExpression returns[string s]: operands += mulExpression (operations += (PLUS | MINUS | OR) operands += mulExpression)*;

mulExpression
    returns[string s]
    : operands += signedFactor (operations+=(STAR | DIV | MOD | AND) operands += signedFactor)*;

signedFactor returns[string s] : sign=(PLUS | MINUS)? factor { $s = ($sign)?.Text + $factor.s; };

factor returns[string s]
   : identifier { $s = $identifier.ctx.GetText().ToLower(); }
   | LPAREN relationExpression RPAREN { $s = "(" + $relationExpression.ctx.s + ")"; }
   | unsignedInteger { $s = $unsignedInteger.ctx.GetText(); }
   | string { $s = Util.GetString($string.ctx); }
   | notFactor { $s = $notFactor.ctx.s; }
   ;

notFactor returns[string s]: NOT factor { $s = $factor.ctx.s; };

procedureStatement: name=identifier LPAREN args += relationExpression (COMMA args += relationExpression)* RPAREN;

emptyStatement: ;

compoundStatement: BEGIN statements END;

statements: statement (SEMI statement)*;

fragment A: 'a' | 'A';
fragment B: 'b' | 'B';
fragment C: 'c' | 'C';
fragment D: 'd' | 'D';
fragment E: 'e' | 'E';
fragment F: 'f' | 'F';
fragment G: 'g' | 'G';
fragment H: 'h' | 'H';
fragment I: 'i' | 'I';
fragment J: 'j' | 'J';
fragment K: 'k' | 'K';
fragment L: 'l' | 'L';
fragment M: 'm' | 'M';
fragment N: 'n' | 'N';
fragment O: 'o' | 'O';
fragment P: 'p' | 'P';
fragment Q: 'q' | 'Q';
fragment R: 'r' | 'R';
fragment S: 's' | 'S';
fragment T: 't' | 'T';
fragment U: 'u' | 'U';
fragment V: 'v' | 'V';
fragment W: 'w' | 'W';
fragment X: 'x' | 'X';
fragment Y: 'y' | 'Y';
fragment Z: 'z' | 'Z';

AND: A N D;
BEGIN: B E G I N;
DIV: D I V;
END: E N D;
INTEGER: I N T E G E R;
MOD: M O D;
NOT: N O T;
OR: O R;
PROCEDURE: P R O C E D U R E;
PROGRAM: P R O G R A M;
VAR: V A R;
PLUS: '+';
MINUS: '-';
STAR: '*';
ASSIGN: ':=';
COMMA: ',';
SEMI: ';';
COLON: ':';
EQUAL: '=';
NOT_EQUAL: '<>';
LT: '<';
LE: '<=';
GE: '>=';
GT: '>';
LPAREN: '(';
RPAREN: ')';
AT: '@';
DOT: '.';
STRING: S T R I N G;
WS: [ \t\r\n] -> skip;
COMMENT_1: '(*' .*? '*)' -> skip;
COMMENT_2: '{' .*? '}' -> skip;
IDENT: [a-zA-Z][a-zA-Z0-9_]*;
STRING_LITERAL: '\'' ('\'\'' | ~('\''))* '\'';
NUM_INT: [0-9]+;