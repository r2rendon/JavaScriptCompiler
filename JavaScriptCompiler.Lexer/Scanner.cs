using JavaScriptCompiler.Core;
using JavaScriptCompiler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaScriptCompiler.Lexer
{
    public class Scanner : IScanner
    {
        private Input input;
        private readonly Dictionary<string, TokenType> keywords;
        private readonly Dictionary<string, TokenType> stringConstants;

        public Scanner(Input input)
        {
            this.input = input;
            this.keywords = new Dictionary<string, TokenType>
            {
                { "if", TokenType.IfKeyword  },
                { "else", TokenType.ElseKeyword },
                { "int", TokenType.IntKeyword },
                { "float", TokenType.FloatKeyword },
                { "string", TokenType.StringKeyword },
                { "class", TokenType.ClassKeyword },
                { "datetime", TokenType.DateTimeKeyword },
                { "foreach", TokenType.ForeachKeyword },
                { "in", TokenType.InKeyword },
                { "bool", TokenType.BoolKeyword },
                { "while", TokenType.WhileKeyword },
                { "list", TokenType.ListKeyword },
            };
            this.stringConstants = new Dictionary<string, TokenType>
            {
                { "true", TokenType.TrueKeyword  },
                { "else", TokenType.FalseKeyword },
            };
        }

        public Token GetNextToken()
        {
            var lexeme = new StringBuilder();
            var currentChar = GetNextChar();
            while (true)
            {
                while (char.IsWhiteSpace(currentChar) || currentChar == '\n')
                {
                    currentChar = GetNextChar();
                }
                if (char.IsLetter(currentChar))
                {
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsLetterOrDigit(currentChar) || currentChar == '.')
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    if (this.keywords.ContainsKey(lexeme.ToString()))
                    {
                        return new Token
                        {
                            TokenType = this.keywords[lexeme.ToString()],
                            Column = input.Position.Column,
                            Line = input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    }
                    else if (this.stringConstants.ContainsKey(lexeme.ToString()))
                    {
                        return new Token
                        {
                            TokenType = this.stringConstants[lexeme.ToString()],
                            Column = input.Position.Column,
                            Line = input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    }

                    return new Token
                    {
                        TokenType = TokenType.Identifier,
                        Column = input.Position.Column,
                        Line = input.Position.Line,
                        Lexeme = lexeme.ToString(),
                    };
                }
                else if (char.IsDigit(currentChar))
                {
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsDigit(currentChar))
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    if (currentChar != '.')
                    {
                        return new Token
                        {
                            TokenType = TokenType.IntConstant,
                            Column = input.Position.Column,
                            Line = input.Position.Line,
                            Lexeme = lexeme.ToString(),
                        };
                    }

                    currentChar = GetNextChar();
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsDigit(currentChar))
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }
                    return new Token
                    {
                        TokenType = TokenType.FloatConstant,
                        Column = input.Position.Column,
                        Line = input.Position.Line,
                        Lexeme = lexeme.ToString(),
                    };

                }
                else switch (currentChar)
                    {
                        case '/':
                            {
                                currentChar = PeekNextChar();
                                if (currentChar != '*')
                                {
                                    lexeme.Append(currentChar);
                                    return new Token
                                    {
                                        TokenType = TokenType.Division,
                                        Column = input.Position.Column,
                                        Line = input.Position.Line,
                                        Lexeme = lexeme.ToString()
                                    };
                                }
                                while (true)
                                {
                                    currentChar = GetNextChar();
                                    while (currentChar == '*')
                                    {
                                        currentChar = GetNextChar();
                                    }

                                    if (currentChar == '/')
                                    {
                                        currentChar = GetNextChar();
                                        break;
                                    }
                                }
                                break;
                            }
                        case '<':
                            lexeme.Append(currentChar);
                            var nextChar = PeekNextChar();
                            if (nextChar != '=')
                            {
                                return new Token
                                {
                                    TokenType = TokenType.LessThan,
                                    Column = input.Position.Column,
                                    Line = input.Position.Line,
                                    Lexeme = lexeme.ToString().Trim()
                                };
                            }

                            lexeme.Append(nextChar);
                            GetNextChar();
                            return new Token
                            {
                                TokenType = TokenType.LessOrEqualThan,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        case '>':
                            lexeme.Append(currentChar);
                            nextChar = PeekNextChar();
                            if (nextChar != '=')
                            {
                                return new Token
                                {
                                    TokenType = TokenType.GreaterThan,
                                    Column = input.Position.Column,
                                    Line = input.Position.Line,
                                    Lexeme = lexeme.ToString().Trim()
                                };
                            }

                            lexeme.Append(nextChar);
                            GetNextChar();
                            return new Token
                            {
                                TokenType = TokenType.GreaterOrEqualThan,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        case '+':
                            lexeme.Append(currentChar);
                            nextChar = PeekNextChar();
                            if (nextChar != '+')
                            {
                                return new Token
                                {
                                    TokenType = TokenType.Plus,
                                    Column = input.Position.Column,
                                    Line = input.Position.Line,
                                    Lexeme = lexeme.ToString().Trim()
                                };
                            }

                            lexeme.Append(nextChar);
                            GetNextChar();
                            return new Token
                            {
                                TokenType = TokenType.Increment,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        case '-':
                            lexeme.Append(currentChar);
                            nextChar = PeekNextChar();
                            if (nextChar != '-')
                            {
                                return new Token
                                {
                                    TokenType = TokenType.Minus,
                                    Column = input.Position.Column,
                                    Line = input.Position.Line,
                                    Lexeme = lexeme.ToString().Trim()
                                };
                            }

                            lexeme.Append(nextChar);
                            GetNextChar();
                            return new Token
                            {
                                TokenType = TokenType.Decrement,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        case '(':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.LeftParens,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case ')':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.RightParens,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '*':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.Asterisk,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case ';':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.SemiColon,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '=':
                            {
                                lexeme.Append(currentChar);
                                currentChar = GetNextChar();
                                if (currentChar != '=')
                                {
                                    return new Token
                                    {
                                        TokenType = TokenType.Assignation,
                                        Column = input.Position.Column,
                                        Line = input.Position.Line,
                                        Lexeme = lexeme.ToString().Trim()
                                    };
                                }
                                
                                lexeme.Append(currentChar);
                                return new Token
                                {
                                    TokenType = TokenType.Equal,
                                    Column = input.Position.Column,
                                    Line = input.Position.Line,
                                    Lexeme = lexeme.ToString().Trim()
                                };
                            }
                        case '\'':
                            {
                                lexeme.Append(currentChar);
                                currentChar = GetNextChar();
                                while (currentChar != '\'')
                                {
                                    lexeme.Append(currentChar);
                                    currentChar = GetNextChar();
                                }
                                lexeme.Append(currentChar);
                                return new Token
                                {
                                    TokenType = TokenType.StringConstant,
                                    Column = input.Position.Column,
                                    Line = input.Position.Line,
                                    Lexeme = lexeme.ToString()
                                };
                            }
                        case '\0':
                            return new Token
                            {
                                TokenType = TokenType.EOF,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = string.Empty
                            };
                        case '{':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.OpenBrace,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '}':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.CloseBrace,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case ',':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.Comma,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '%':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.Mod,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '!':
                            lexeme.Append(currentChar);
                            nextChar = PeekNextChar();
                            var tokenType = TokenType.Distinct;
                            if (nextChar == '=')
                            {
                                lexeme.Append(nextChar);
                                GetNextChar();
                                tokenType = TokenType.NotEqual;
                            }

                            return new Token
                            {
                                TokenType = tokenType,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        case '&':
                            lexeme.Append(currentChar);
                            currentChar = GetNextChar();
                            if (currentChar != '&')
                            {
                                throw new ApplicationException($"Caracter {lexeme} invalido en la columna: {input.Position.Column}, fila: {input.Position.Line}");
                            }
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.And,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '|':
                            lexeme.Append(currentChar);
                            currentChar = GetNextChar();
                            if (currentChar != '|')
                            {
                                throw new ApplicationException($"Caracter {lexeme} invalido en la columna: {input.Position.Column}, fila: {input.Position.Line}");
                            }
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.Or,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '[':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.LeftBracket,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case ']':
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.RightBracket,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        case '"':
                            lexeme.Append(currentChar);
                            currentChar = GetNextChar();
                            while (currentChar != '"')
                            {
                                lexeme.Append(currentChar);
                                currentChar = GetNextChar();
                            }
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.StringConstant,
                                Column = input.Position.Column,
                                Line = input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        default:
                            throw new ApplicationException($"Caracter {lexeme} invalido en la columna: {input.Position.Column}, fila: {input.Position.Line}");
                    }
            }
        }

        private char GetNextChar()
        {
            var next = input.NextChar();
            input = next.Reminder;
            return next.Value;
        }

        private char PeekNextChar()
        {
            var next = input.NextChar();
            return next.Value;
        }
    }
}