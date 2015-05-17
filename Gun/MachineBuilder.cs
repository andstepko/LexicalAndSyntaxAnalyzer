using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class MachineBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">Tuple<finalType, regexp></param>
        /// <returns></returns>
        public static StateMachine Build(List<Tuple<string, string>> input)
        {
            StateMachine result;
            StateMachine temp;
            List<StateMachine> machines = new List<StateMachine>();
            List<string> finalTypes = GetFinalTypes(input);

            result = Build(input[0].Item2);

            result.FinalTypesDirty = finalTypes;
            result.SetAllFinalAs(0);
            
            for(int i = 1; i < input.Count; i++)
            {
                Tuple<string, string> tuple = input[i];

                temp = Build(tuple.Item2);
                temp.FinalTypesDirty = finalTypes;
                temp.SetAllFinalAs(i);
                temp = Determiner.Determined(temp);
                result |= temp;
            }
            result = Determiner.Determined(result);
            return result;
        }

        public static StateMachine Build(string regexp)
        {
            List<char> postfix = InfixToPostfixWithInsertions(regexp);
            StateMachine result;
            Stack<StateMachine> stack = new Stack<StateMachine>();
            StateMachine temp1;
            StateMachine temp2;

            //foreach (char c in postfix)
            for(int i = 0; i < postfix.Count; i++)
            {
                char c = postfix[i];

                if(c == '\\')
                {
                    //FIXME
                    if (i + 1 < postfix.Count)
                    {
                        stack.Push(new StateMachine(postfix[i+1]));
                    }
                    i++;
                }
                else if (IsOperand(c))
                {
                    stack.Push(new StateMachine(c));
                }
                else
                {
                    // Operator.
                    switch(c)
                    {
                        case '*':
                            temp1 = stack.Pop();
                            temp1 = StateMachine.Cycle(temp1);
                            stack.Push(temp1);
                            break;
                        case '+':
                            temp1 = stack.Pop();
                            temp1 = +temp1;
                            stack.Push(temp1);
                            break;
                        case '&':
                            temp2 = stack.Pop();
                            temp1 = stack.Pop();
                            temp1 = temp1 & temp2;
                            stack.Push(temp1);
                            break;
                        case '|':
                            temp2 = stack.Pop();
                            temp1 = stack.Pop();
                            temp1 = temp1 | temp2;
                            stack.Push(temp1);
                            break;
                    } // switch                    
                } // Operator
            } // postfix
            result = stack.Pop();
            return result;
        }

    #region InfixToPostfix
        private static List<char> InfixToPostfix(List<char> input)
        {
            List<char> result = new List<char>();
            Stack<char> operators = new Stack<char>();

            //foreach (char c in input)
            for(int i = 0; i < input.Count; i++)
            {
                char c = input[i];

                if ((IsOperand(c)) || (c == '\\'))
                {
                    if (c == '\\')
                    {
                        result.Add(c);
                        i++;
                        c = input[i];
                        result.Add(c);
                    }
                    else
                    {
                        // Just an operand.
                        result.Add(c);
                    }
                }
                else
                {
                    // Not an operand.
                    if (IsOpeningBracket(c))
                    {
                        operators.Push(c);
                        continue;
                    }
                    if (IsClosingBracket(c))
                    {
                        if (c == ')')
                        {
                            while ((operators.Count > 0) && (operators.Peek() != '('))
                            {
                                result.Add(operators.Pop());
                            }
                        }
                        else if (c == ']')
                        {
                            while ((operators.Count > 0) && (operators.Peek() != '['))
                            {
                                result.Add(operators.Pop());
                            }
                        }
                        operators.Pop(); // Opening bracket from stack.
                        continue;
                    } // End of closing bracket.
                    if (IsOperator(c))
                    {
                        while ((operators.Count > 0) && (!PrioritetIsHigher(c, operators.Peek())))
                        {
                            result.Add(operators.Pop());
                        }
                        operators.Push(c);
                    }
                }
            } // Input is over.
            while (operators.Count > 0)
            {
                result.Add(operators.Pop());
            }
            return result;
        }
        private static List<char> InfixToPostfixWithInsertions(string inputString)
        {
            List<char> list = StringToChars(inputString);
            list = InsertOperatorsResolveDashes(list);
            return InfixToPostfix(list);
        }

        private static List<char> InsertOperatorsResolveDashes(List<char> inputList)
        {
            bool multiply = true;
            List<char> result = new List<char>();
            List<char> input = ResolveDashes(inputList);
            //List<char> input = inputList;

            for(int i = 0; i < input.Count; i++)
            {
                char c = input[i];

                if (c == '[')
                {
                    multiply = false;
                    result.Add(c);
                }
                else if (c == ']')
                {
                    multiply = true;
                    result.Add(c);

                    if ((i + 1 < input.Count) && (IsOperand(input[i + 1]) || IsScreen(input[i + 1]) || (input[i + 1] == '[')))
                    {
                        // Possible look ahead.
                        if (multiply)
                        {
                            result.Add('&');
                        }
                        else
                        {
                            result.Add('|');
                        }
                    }
                }


                else if (IsScreen(c))
                {
                    result.Add(c);
                    i++;
                    c = input[i];
                    result.Add(c);
                    // FIXME
                    if ((i+1 < input.Count) && (IsOperand(input[i + 1]) || IsScreen(input[i + 1]) || (c == '[')))
                    {
                        // Possible look ahead.
                        if (multiply)
                        {
                            result.Add('&');
                        }
                        else
                        {
                            result.Add('|');
                        }
                    }
                }


                else if (IsOperand(c))
                {
                    result.Add(c);
                    if ((input.Count > i + 1) && (IsOperand(input[i + 1]) || IsScreen(input[i + 1])))
                    {
                        // Possible look ahead.
                        if (multiply)
                        {
                            result.Add('&');
                        }
                        else
                        {
                            result.Add('|');
                        }
                    } // End of Possible look ahead.                    
                } // End of If Operand.
                else
                {
                    // Assumed an operator
                    result.Add(c);
                    if ((c == '*') || (c == '+'))
                    {
                        // Not and OR or AND operator.
                        if ((i + 1 < input.Count) && (IsOperand(input[i + 1]) || IsScreen(input[i + 1]) || (input[i + 1] == '[')))
                        {
                            // Possible look ahead.
                            if (multiply)
                            {
                                result.Add('&');
                            }
                            else
                            {
                                result.Add('|');
                            }
                        }
                    }
                }
            } // End of for
            return result;
        } // InsertOperatorsResolveDashes

        private static List<char> ResolveDashes(List<char> input)
        {
            List<char> result = new List<char>();

            for(int i = 0; i < input.Count; i++)
            {
                char c = input[i];

                if (c != '-')
                {
                    result.Add(c);
                }
                else
                {
                    // Got dash.
                    if ((result.Count > 0) && (IsScreen(result[result.Count - 1])))
                    {
                        // Is screened!
                        result.Add(c);
                    }
                    else
                    {
                        char start = result[result.Count - 1];
                        start++;
                        char finish = input[i + 1];
                        for (char cur = start; cur < finish; cur++)
                        {
                            result.Add('|');
                            result.Add(cur);
                        }
                        result.Add('|');
                    }
                }
            }
            return result;
        }

        private static bool IsOperand(char c)
        {
            return !(IsOperator(c) || IsOpeningBracket(c) || IsClosingBracket(c) || IsScreen(c));
        }

        private static bool IsScreen(char c)
        { return c == '\\'; }

        private static bool IsOperator(char c)
        {
            return ((c == '&') || (c == '|') || (c == '*') || (c == '+'));
        }

        private static bool IsOpeningBracket(char c)
        {
            return ((c == '[') || (c == '('));
        }

        private static bool IsClosingBracket(char c)
        {
            return ((c == ']') || (c == ')'));
        }

        private static bool PrioritetIsHigher(char c1, char c2)
        {
            List<char> prioritets = new List<char>() { '|', '&', '+', '*'};
            return prioritets.IndexOf(c1) > prioritets.IndexOf(c2);
        }
    #endregion

        private static List<string> GetFinalTypes(List<Tuple<string, string>> input)
        {
            List<string> result = new List<string>();

            foreach (Tuple<string, string> tuple in input)
            {
                result.Add(tuple.Item1);
            }
            return result;
        }

        private static List<char> StringToChars(string input)
        {
            List<char> result = new List<char>();

            foreach (char c in input)
            {
                result.Add(c);
            }
            return result;
        }
    }
}