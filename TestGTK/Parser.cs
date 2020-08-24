using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestGTK
{
    public class Parser
    {
        public class SyntaxException : Exception
        {
            public SyntaxException(String message, int pc)
                : base(message)
            {
                Pc = pc;
            }

            public int Pc { get; }
        }

        class Expression
        {
            public Expression(int _pc, int _repeat, IEnumerable<Expression> _subExpressions=null)
            {
                pc = _pc;
                repeat = _repeat;
                subExpressions = _subExpressions;
            }

            int pc;
            int repeat;
            public IEnumerable<Expression> subExpressions;

            public IEnumerable<int> GetTrajectory()
            {
                for (int i = 0; i < repeat; i++)
                    if (subExpressions == null)
                        yield return pc;
                    else
                        foreach (var expression in subExpressions)
                            foreach (var p in expression.GetTrajectory())
                                yield return p;
            }

            override public String ToString()
            {
                String s = "";
                String r = repeat > 1 ? repeat.ToString() : "";
                if (subExpressions == null)
                    s += r + "[" + pc + "]";
                else
                {
                    s += r + "(";
                    foreach (var expression in subExpressions)
                        s += expression.ToString();
                    s += ")";

                }
                return s;
            }
        }

        public static IEnumerable<int> Parse(String source)
        {
            Parser parser = new Parser(source);
            int length = 0;
            Expression expression = new Expression(0, 1, parser.ParseAll(0, out length));
            Match match = Regex.Match(source.Substring(length), @"^(\s*)(.)");
            if (match.Success)
                throw new SyntaxException(ErrorForRunawayChar(match.Groups[2].Value), length + match.Groups[1].Length);
            else
                return expression.GetTrajectory();
        }

        static String ErrorForRunawayChar(string c)
        {
            if (Regex.IsMatch(c, @"\d"))
                return "Number must be followed by expression";
            if (c == ")")
                return "Unmatched bracket";
            return String.Format("Unknown token '{0}'", c);
        }

        Parser(String _source)
        {
            source = _source;
        }

        String source;

        bool TryParse(int pc, out Expression expression, out int length)
        {
            Match match = Regex.Match(source.Substring(pc), @"^(\s*\d*\s*)([fhv\(])");
            if (match.Success)
            {
                int repeat = int.TryParse(match.Groups[1].Value, out repeat) ? repeat : 1;
                pc += match.Groups[1].Length;
                length = match.Value.Length;
                if (match.Groups[2].Value == "(")
                {
                    pc += 1;
                    int subLength = 0;
                    var subExpressions = ParseAll(pc, out subLength);
                    Match endMatch = Regex.Match(source.Substring(pc + subLength), @"^(\s*)(.)");
                    if (endMatch.Success)
                    {
                        if (endMatch.Groups[2].Value == ")")
                        {
                            expression = new Expression(pc, repeat, subExpressions);
                            length += subLength + endMatch.Length;
                        }
                        else
                            throw new SyntaxException(ErrorForRunawayChar(endMatch.Groups[2].Value), pc + subLength + endMatch.Groups[1].Length);
                    }
                    else
                        throw new SyntaxException("Missing end bracket", pc + subLength);
                }
                else
                {
                    expression = new Expression(pc, repeat);
                }
                return true;
            }
            else
            {
                expression = null;
                length = 0;
                return false;
            }
        }

        IEnumerable<Expression> ParseAll(int pc, out int length)
        {
            Expression expression = null;
            var expressions = new List<Expression>();
            int subLength = 0;
            length = 0;
            while (TryParse(pc, out expression, out subLength))
            {
                expressions.Add(expression);
                pc += subLength;
                length += subLength;
            }
            return expressions;
        }
    }
}
