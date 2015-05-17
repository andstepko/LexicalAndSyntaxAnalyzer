using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class Gramatics
    {
        public static List<double> WrongStringOutput = new List<double>() { -1 };
        private static string EndOfLine = "#";
        private static int PrintingItemWidth = 8;

        private List<string> _terminalTypes; // List of types of tokens.
        private List<string> _nonterminalTypes;
        private string _emptyStringType;
        private List<string> _emptyStringAlternative;
        private List<List<Tuple<List<string>, double>>> _controlTable;
        private List<GrammarRule> _grammarRules;
        private int _leftSyntheticCount = 1;
        private int _startSyntheticCount = 1;
        private List<List<string>> _firsts = new List<List<string>>();
        private List<List<string>> _follows = new List<List<string>>();
        private List<string> _allowingEmpty = new List<string>();

        private List<GrammarRule> GrammarRules
        {
            get
            {
                return _grammarRules;
            }
            set
            {
                _grammarRules = new List<GrammarRule>();
                foreach (GrammarRule grammarRule in value)
                {
                    InsertNewGrammarRule(_grammarRules.Count, grammarRule);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="grammarRules">Only the first grammarRule must finish with #.</param>
        /// <param name="terminalSymbolsTypes">Names of the types of terminal symbols.</param>
        /// <param name="emptyStringType">The name of emptyString type.</param>
        public Gramatics(List<GrammarRule> grammarRules, List<string> terminalSymbolsTypes, string emptyStringType)
        {
            _emptyStringType = emptyStringType;
            _emptyStringAlternative = new List<string>() { _emptyStringType };

            _terminalTypes = terminalSymbolsTypes;
            _terminalTypes.Add(EndOfLine);

            GrammarRules = grammarRules;
            FixLeftRecursions();

            UpdateNonTerminals(GrammarRules);

            FillFirstFollows();

            BuildControlTable();

            Factorize();

            FixEmpties();

            BuildControlTable();
        }

        public Gramatics(List<string> grammarRuleStrings, List<string> terminalSymbolsTypes, string emptyStringType)
            : this(GrammarRule.GrammarRules(grammarRuleStrings), terminalSymbolsTypes, emptyStringType) { }

        #region Is
        public bool IsTypeOfTerminal(string type)
        {
            foreach(string currType in _terminalTypes)
            {
                if(currType == type)
                    return true;
            }
            if (type == EndOfLine)
            {
                return true;
            }
            return false;
        }

        public bool IsTypeOfNonterminal(string type)
        {
            foreach(string currType in _nonterminalTypes)
            {
                if(currType == type)
                    return true;
            }
            return false;
        }
        #endregion Is

        public bool ExistsGrammarRule(string nonTerminalType)
        {
            if (GetGrammarRule(nonTerminalType) != null)
            {
                return true;
            }
            return false;
        }

        private bool AllowsEmptyString(string nonTerminalType, List<string> callStack)
        {
            GrammarRule grammarRule = GetGrammarRule(nonTerminalType);

            return AllowsEmptyString(grammarRule, callStack);
        }
        private bool AllowsEmptyString(GrammarRule grammarRule, List<string> callStack)
        {
            if (grammarRule == null)
            { return false; }

            if (_allowingEmpty.Contains(grammarRule.ProductTypeName))
            {
                return true;
            }

            if (callStack.Contains(grammarRule.ProductTypeName))
            {
                return false;
            }
            callStack.Add(grammarRule.ProductTypeName);

            if (ContainsEmptyString(grammarRule))
            {
                //DeleteFromStack(callStack, grammarRule.ProductTypeName);
                MakeAllowingEmpty(grammarRule.ProductTypeName);
                return true;
            }

            foreach (List<string> alternative in grammarRule.Alternatives)
            {
                ///// IF Alternative is on the top of the stack! (New method)
                //if(StackStartsWith(callStack, alternative))
                //{
                //    return true;
                //}

                if (AllowsEmptyString(alternative, callStack))
                {
                    //DeleteFromStack(callStack, grammarRule.ProductTypeName);
                    MakeAllowingEmpty(grammarRule.ProductTypeName);
                    return true;
                }
            }
            return false;
        }
        private bool AllowsEmptyString(List<string> alternative, List<string> callStack)
        {
            if ((alternative.Count == 1) && (alternative[0].Equals(_emptyStringType)))
            {
                return true;
            }

            foreach (string symbol in alternative)
            {
                if (IsTypeOfTerminal(symbol))
                {
                    return false;
                }
                if (IsTypeOfNonterminal(symbol))
                {
                    //if((callStack.Count > 0) && (symbol == callStack[0]) && (alternative[alternative.Count - 1] == symbol))
                    //{
                    //    // Do nothing!
                    //}
                    if (!AllowsEmptyString(symbol, callStack))
                    {
                        return false;
                    }
                }
            }
            //DeleteFromStack(callStack, alternative);
            return true;
        }

        private bool MakeAllowingEmpty(string nonTerminal)
        {
            if (_allowingEmpty.Contains(nonTerminal))
            {
                return false;
            }
            _allowingEmpty.Add(nonTerminal);
            return true;
        }

        private bool ContainsEmptyString(string nonTerminalType)
        {
            GrammarRule grammarRule = GetGrammarRule(nonTerminalType);

            return ContainsEmptyString(grammarRule);
        }
        private bool ContainsEmptyString(GrammarRule grammarRule)
        {
            if (grammarRule == null)
            { return false; }

            foreach (List<string> alternative in grammarRule.Alternatives)
            {
                if ((alternative.Count == 1) && (alternative[0].Equals(_emptyStringType)))
                {
                    // Can be just emptyString.
                    return true;
                }
            }
            return false;
        }

        private bool StopConsistingEmptyString(GrammarRule grammarRule)
        {
            //int index = grammarRule.Alternatives.IndexOf(_emptyStringAlternative);
            int index = GetAlternativeIndex(grammarRule, _emptyStringAlternative);

            if (index == -1)
            {
                return false;
            }
            while (index > -1)
            {
                grammarRule.Alternatives.RemoveAt(index);
                index = GetAlternativeIndex(grammarRule, _emptyStringAlternative);
            }
            return true;
        }

        #region Get
        private GrammarRule GetGrammarRule(string nonTerminalType)
        {
            foreach (GrammarRule grammarRule in GrammarRules)
            {
                if (grammarRule.ProductTypeName == nonTerminalType)
                {
                    return grammarRule;
                }
            }
            return null;
        }

        private int GetNonTerminalIndex(string nonTerminalType)
        {
            return _nonterminalTypes.IndexOf(nonTerminalType);
        }

        private int GetTerminalIndex(string terminalType)
        {
            return _terminalTypes.IndexOf(terminalType);
        }

        private int GetTerminalIndex(Tuple<string, string> lexeme)
        {
            return GetTerminalIndex(lexeme.Item2);
        }

        private int GetGrammarRuleIndex(GrammarRule grammarRule)
        {
            return GrammarRules.IndexOf(grammarRule);
        }

        private int GetAlternativeIndex(GrammarRule grammarRule, List<string> alternative)
        {
            for(int i = 0; i < grammarRule.Alternatives.Count; i++)
            {
                if(ListsAreEqual(alternative, grammarRule.Alternatives[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private Tuple<List<string>, double> GetTableCell(string nonTerminal, string terminal)
        {
            int i = GetNonTerminalIndex(nonTerminal);
            int j = GetTerminalIndex(terminal);

            if ((i == -1) || (j == -1))
            {
                return null;
            }
            return _controlTable[i][j];
        }
        #endregion Get

        private void UpdateNonTerminals() { UpdateNonTerminals(GrammarRules); }
        private void UpdateNonTerminals(List<GrammarRule> grammarRules)
        {
            _nonterminalTypes = new List<string>();
            foreach (GrammarRule rule in grammarRules)
            {
                _nonterminalTypes.Add(rule.ProductTypeName);
            }
        }

        private void InsertNewGrammarRule(int index, GrammarRule grammarRule)
        {
            _grammarRules.Insert(index, grammarRule);
            UpdateNonTerminals();
        }

        #region Knut-Foster
        // TODO
        private void GrammarRulesToKnutFoster()
        {
            FixLeftRecursions();
            Factorize();
            FixEmpties();
        }

        private bool FixLeftRecursions()
        {
            int leftRecursiveGrammarIndex = FirstLeftRecursionIndex();

            if (leftRecursiveGrammarIndex == -1)
            {
                return false;
            }
            while (leftRecursiveGrammarIndex != -1)
            {
                Tuple<GrammarRule, GrammarRule> newGrammarRules = InsteadOfLeftRecursive(GrammarRules[leftRecursiveGrammarIndex]);
                GrammarRules[leftRecursiveGrammarIndex] = newGrammarRules.Item1;
                InsertNewGrammarRule(leftRecursiveGrammarIndex + 1, newGrammarRules.Item2);


                BuildControlTable();

                leftRecursiveGrammarIndex = FirstLeftRecursionIndex();
            }
            return true;
        }

        private Tuple<GrammarRule, GrammarRule> InsteadOfLeftRecursive(GrammarRule grammarRule)
        {
            if (!grammarRule.IsLeftRecursive)
            {
                return null;
            }

            GrammarRule newRule = new GrammarRule(NewLeftRuleName(), new List<List<string>>());
            List<List<string>> leftRecursiveAlternatives = new List<List<string>>();

            // Delete leftRecursive rules, fill leftRecursiveAlternatives.
            for (int i = 0; i < grammarRule.Alternatives.Count; i++)
            {
                if ((grammarRule.Alternatives[i].Count > 0) && (grammarRule.Alternatives[i][0].Equals(grammarRule.ProductTypeName)))
                {
                    // Alternative is leftRecursive.
                    grammarRule.Alternatives[i].RemoveAt(0);
                    leftRecursiveAlternatives.Add(new List<string>(grammarRule.Alternatives[i]));
                    grammarRule.Alternatives.RemoveAt(i);
                    i--;
                }
            }
            if (grammarRule.Alternatives.Count == 0)
            {
                grammarRule.Alternatives.Add(new List<string> { newRule.ProductTypeName });
            }

            // Fill newRule.
            int alternativesCount = leftRecursiveAlternatives.Count;
            for (int i = 0; i < alternativesCount; i++)
            {
                List<string> newAlternative = new List<string>(leftRecursiveAlternatives[i]);

                //newAlternative.Add(newRule.ProductTypeName);
                newRule.Alternatives.Add(newAlternative);

                newAlternative = new List<string>(newAlternative);
                newAlternative.Add(newRule.ProductTypeName);
                newRule.Alternatives.Add(newAlternative);
            }

            // Fill grammarRule.
            alternativesCount = grammarRule.Alternatives.Count;
            for (int i = 0; i < alternativesCount; i++)
            {
                List<string> newAlternative = new List<string>(grammarRule.Alternatives[i]);

                newAlternative.Add(newRule.ProductTypeName);
                grammarRule.Alternatives.Add(newAlternative);
            }

            string oldName = grammarRule.ProductTypeName.ToString();
            grammarRule.ProductTypeName = NewLeftRuleName(grammarRule.ProductTypeName);

            RenameGrammarRule(newRule, oldName, grammarRule.ProductTypeName);
            RenameGrammarRules(oldName, grammarRule.ProductTypeName);

            ClearEqualAlternatives(grammarRule);
            ClearEqualAlternatives(newRule);

            return new Tuple<GrammarRule, GrammarRule>(grammarRule, newRule);
        }

        private bool Factorize()
        {
            int unfactorizedGrammarIndex = FirstUnfactorizedRule();

            if (unfactorizedGrammarIndex == -1)
            {
                return false;
            }
            while (unfactorizedGrammarIndex != -1)
            {
                string unfactorisingSymbol = UnfactorisingSymbol(GrammarRules[unfactorizedGrammarIndex]);

                Tuple<GrammarRule, GrammarRule> newGrammarRules = InsteadOfUnfactorized(GrammarRules[unfactorizedGrammarIndex],
                    unfactorisingSymbol);

                GrammarRules[unfactorizedGrammarIndex] = newGrammarRules.Item1;
                //GrammarRules.Insert(unfactorizedGrammarIndex + 1, newGrammarRules.Item2);

                InsertNewGrammarRule(unfactorizedGrammarIndex + 1, newGrammarRules.Item2);

                InsertFirstFollow(unfactorizedGrammarIndex + 1, newGrammarRules.Item2.ProductTypeName);

                BuildControlTable();

                unfactorizedGrammarIndex = FirstUnfactorizedRule();
            }
            return true;
        }

        private Tuple<GrammarRule, GrammarRule> InsteadOfUnfactorized(GrammarRule grammarRule, string startingSymbol)
        {
            if (grammarRule.ProductTypeName == "Bool_Plural_Statement_Eps")
            { }




            if (UnfactorisingSymbol(grammarRule) == null)
            {
                return null;
            }
            
            GrammarRule newRule = new GrammarRule(NewUnfactorizedRuleName(), new List<List<string>>());
            List<List<string>> unfactorizedAlternatives = new List<List<string>>();

            for (int i = 0; i < grammarRule.Alternatives.Count; i++)
            {
                if ((grammarRule.Alternatives[i].Count > 0) && (!grammarRule.Alternatives[i][0].Equals(startingSymbol)) &&
                    (CanStartWith(grammarRule.Alternatives[i], startingSymbol)))
                {
                    // Doesn't start with the particular terminal, but this terminal is digged in the starting nonterminal.
                    //List<string> newAlternative = new List<string>(grammarRule.Alternatives[i]);

                    //newAlternative = UnpackAlternative(newAlternative, startingSymbol);
                    //grammarRule.Alternatives[i] = newAlternative;

                    ///////////////////////////////////////////////////////

                    List<List<string>> newAlternatives = UnpackAlternative(grammarRule.Alternatives[i], startingSymbol);

                    if ((newAlternatives != null) && (newAlternatives.Count > 0))
                    {
                        grammarRule.Alternatives[i] = newAlternatives[0];

                        int j = 1;

                        for (; j < newAlternatives.Count; j++)
                        {
                            grammarRule.Alternatives.Insert(i + j, newAlternatives[j]);
                        }
                    }
                }
                if ((grammarRule.Alternatives[i].Count > 0) && (grammarRule.Alternatives[i][0].Equals(startingSymbol)))
                {
                    // Alternative starts with startingSymbol.
                    grammarRule.Alternatives[i].RemoveAt(0);
                    if (grammarRule.Alternatives[i].Count > 0)
                    {
                        unfactorizedAlternatives.Add(new List<string>(grammarRule.Alternatives[i]));
                    }
                    else
                    {
                        unfactorizedAlternatives.Add(_emptyStringAlternative);
                    }
                    grammarRule.Alternatives.RemoveAt(i);
                    i--;
                }
            }

            grammarRule.Alternatives.Insert(0, new List<string>() { startingSymbol, newRule.ProductTypeName});
            newRule.Alternatives = new List<List<string>>(unfactorizedAlternatives);

            string oldName = grammarRule.ProductTypeName.ToString();
            grammarRule.ProductTypeName = NewUnfactorizedRuleName(grammarRule.ProductTypeName);
            
            RenameGrammarRule(newRule, oldName, grammarRule.ProductTypeName);
            RenameGrammarRules(oldName, grammarRule.ProductTypeName);

            ClearEqualAlternatives(grammarRule);
            ClearEqualAlternatives(newRule);

            return new Tuple<GrammarRule, GrammarRule>(grammarRule, newRule);
        }

        private bool FixEmpties()
        {
            bool result = false;

            foreach (GrammarRule grammarRule in GrammarRules)
            {
                result |= FixEmpties(grammarRule);
            }
            return result;
        }
        private bool FixEmpties(GrammarRule grammarRule)
        {
            if (!ContainsEmptyString(grammarRule))
            {
                return false;
            }
            else
            {
                // Consists { Eps } alternative.
                foreach (List<string> alternative in grammarRule.Alternatives)
                {
                    if (AllowsEmptyString(alternative , new List<string>()))
                    { }

                    if ((!ListsAreEqual(alternative, _emptyStringAlternative)) && (AllowsEmptyString(alternative, new List<string>())))
                    {
                        // Found alternative, which is not { Eps }, but allows empty string.
                        StopConsistingEmptyString(grammarRule);
                        return true;
                    }
                }
                return false;
            }
        }

        private int FirstLeftRecursionIndex()
        {
            int index = 0;

            foreach (GrammarRule grammarRule in GrammarRules)
            {
                if (grammarRule.IsLeftRecursive)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        private string UnfactorisingSymbol(GrammarRule grammarRule)
        {
            List<string> firsts = new List<string>();

            for (int i = 0; i < grammarRule.Alternatives.Count; i++)
            {
                List<string> alternative = grammarRule.Alternatives[i];

                foreach(string first in firsts)
                {
                    if (CanStartWith(alternative, first))
                    {
                        if (AlternativesStartingWith(grammarRule, alternative[0]) > 1)
                        {
                            return alternative[0];
                        }
                        else
                        {
                            return first;
                        }
                    }
                }
                // Current alternative is OK.
                // Add firsts of current alternative to storage.
                firsts.AddRange(GetFirst(alternative));
            }
            return null;
        }

        private int FirstUnfactorizedRule()
        {
            int index = 0;

            foreach (GrammarRule grammarRule in GrammarRules)
            {
                if (UnfactorisingSymbol(grammarRule) != null)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        private List<List<string>> UnpackAlternative(List<string> symbols, string startingSymbol)
        {
            if (symbols[0] == "Bool_Statement()")
            { }
            if(IsTypeOfNonterminal(startingSymbol)){
                if (!CanStartWith(symbols, startingSymbol))
                { return null; }
            }

            List<string> source = new List<string>(symbols);
            List<List<string>> result = new List<List<string>>();
            
            result.Add(source);

            while ((result.Count > 0) && (result[0].Count > 0) && (!result[0][0].Equals(startingSymbol)))
            {
                string top = result[0][0];
                if (IsTypeOfNonterminal(top))
                {
                    //Tuple<List<string>, double> cell = GetTableCell(top, startingSymbol);
                    //result[0].RemoveAt(0);
                    //result.InsertRange(0, cell.Item1);

                    List<string> suffix = new List<string>(result[0]);
                    suffix.RemoveAt(0);
                    List<List<string>> newAlternatives = new List<List<string>>();

                    foreach (List<string> alternative in GetGrammarRule(top).Alternatives)
                    {
                        newAlternatives.Add(new List<string>(alternative));
                    }

                    foreach (List<string> alternative in newAlternatives)
                    {
                        alternative.AddRange(suffix);
                    }

                    result.RemoveAt(0);
                    result.InsertRange(0, newAlternatives);
                }
                else
                {
                    // Top is a terminal.
                    if (!top.Equals(startingSymbol))
                    //{ return null; }
                    {
                        return result;
                    }
                }
            }

            if ((result.Count > 0) && (result[0].Count > 0) && (result[0][0].Equals(startingSymbol)))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private string NewUnfactorizedRuleName()
        {
            //int i = 1;
            //string result = NewUnfactorizedRuleName(i.ToString());

            //while (ExistsGrammarRule(result))
            //{
            //    i++;
            //    result = NewUnfactorizedRuleName(i.ToString());
            //}
            //return result;
            string result = NewUnfactorizedRuleName(_startSyntheticCount.ToString());
            _startSyntheticCount++;

            return result;
        }
        private static string NewUnfactorizedRuleName(string oldName)
        {
            string prefix = "s";

            return NewRuleName(prefix, oldName);
        }

        private string NewLeftRuleName()
        {
            //int i = 1;
            //string result = NewLeftRuleName(i.ToString());

            //while (ExistsGrammarRule(result))
            //{
            //    i++;
            //    result = NewLeftRuleName(i.ToString());
            //}
            string result = NewLeftRuleName(_leftSyntheticCount.ToString());
            _leftSyntheticCount++;

            return result;
        }
        private static string NewLeftRuleName(string oldName)
        {
            string prefix = "l";

            return NewRuleName(prefix, oldName);
        }

        private static string NewRuleName(string newPrefix, string oldName)
        {
            return newPrefix + oldName;
        }

        private bool RenameGrammarRules(string oldName, string newName)
        {
            bool result = false;

            foreach (GrammarRule rule in GrammarRules)
            {
                result |= RenameGrammarRule(rule, oldName, newName);
            }
            return result;
        }

        private bool RenameGrammarRule(GrammarRule rule, string oldName, string newName)
        {
            bool result = false;

            foreach (List<string> alternative in rule.Alternatives)
            {
                for (int i = 0; i < alternative.Count; i++)
                {
                    if (alternative[i].Equals(oldName))
                    {
                        alternative[i] = newName;
                        result = true;
                    }
                }
            }
            return result;
        }

        private bool ClearEqualAlternatives(GrammarRule rule)
        {
            bool result = false;

            for(int i = 0; i < rule.Alternatives.Count; i++)
            {
                for (int j = i + 1; j < rule.Alternatives.Count; j++)
                {
                    if(ListsAreEqual(rule.Alternatives[i], rule.Alternatives[j]))
                    {
                        rule.Alternatives.RemoveAt(j);
                        result = true;
                        j--;
                    }
                }
            }
            return result;
        }

        private static bool ClearEqualElements(List<string> list)
        {
            bool result = false;

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Equals(list[j]))
                    {
                        list.RemoveAt(j);
                        result = true;
                        j--;
                    }
                }
            }
            return result;
        }

        private static bool ListsAreEqual(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }
            return true;
        }

        private static List<string> Intersect(List<string> list1, List<string> list2)
        {
            List<string> result = new List<string>();

            foreach (string s1 in list1)
            {
                foreach (string s2 in list2)
                {
                    if (s1.Equals(s2))
                    {
                        result.Add(s1);
                    }
                }
            }
            return result;
        }

        private static bool StackStartsWith(List<string> stack, List<string> start)
        {
            if (start.Count < start.Count)
            {
                return false;
            }

            for(int i = 0; i < start.Count; i++)
            {
                if (start[i] != stack[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool DeleteFromStack(List<string> stack, string element)
        {
            bool result = false;

            for (int i = 0; i < stack.Count; i++)
            {
                if (stack[i] == element)
                {
                    //for (int j = i; j < stack.Count; j++)
                    //{
                    //    stack.RemoveAt(j);
                    //}
                    stack.RemoveAt(i);
                    i--;
                    result = true;
                }
            }
            return result;
        }
        private static bool DeleteFromStack(List<string> stack, List<string> alternative)
        {
            bool result = false;

            foreach (string symbol in alternative)
            {
                result |= DeleteFromStack(stack, symbol);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grammarRule"></param>
        /// <param name="symbol"></param>
        /// <returns>How many alternatives in grammarRule start with symbol.</returns>
        private static int AlternativesStartingWith(GrammarRule grammarRule, string symbol)
        {
            int resultCount = 0;

            foreach(List<string> alternative  in grammarRule.Alternatives)
            {
                if ((alternative.Count > 0) && (alternative[0].Equals(symbol)))
                {
                    resultCount++;
                }
            }
            return resultCount;
        }
        #endregion Knut-Foster

        #region FirstFollow
        // DESCRIBE
        private List<string> GetFirst(string nonTerminalType)
        {
            if (nonTerminalType == "sss18")
            { }



            List<string> result = new List<string>();
            GrammarRule grammarRule;

            if(!IsTypeOfNonterminal(nonTerminalType))
            {
                return result;
            }
            grammarRule = GetGrammarRule(nonTerminalType);
            if (grammarRule == null)
            {
                return result;
            }

            foreach (List<string> alternative in grammarRule.Alternatives)
            {
                result.AddRange(GetFirst(alternative));
            }

            ClearEqualElements(result);

            return result;
        }
        private List<string> GetFirst(List<string> symbolList)
        {
            List<string> result = new List<string>();

            if (symbolList.Count <= 0)
            {
                return result;
            }

            if (IsTypeOfTerminal(symbolList[0]))
            {
                result.Add(symbolList[0]);
            }
            if (IsTypeOfNonterminal(symbolList[0]))
            {
                result.AddRange(GetFirst(symbolList[0]));
            }


            ///////////////////////////////////////////
            int i = 0;
            while ((IsTypeOfNonterminal(symbolList[i])) && AllowsEmptyString(symbolList[i], new List<string>()))
            {
                // While top of the alternative-stack allows emptyString.
                i++;
                if (i >= symbolList.Count)
                {
                    break;
                }
                if (!IsTypeOfNonterminal(symbolList[i]))
                {
                    // Not a nonTerminal symbol.
                    break;
                }
                result.AddRange(GetFirst(symbolList[i]));
            }




            return result;
        }

        private List<string> GetSavedFirst(string nonTerminalType)
        {
            int index = GetNonTerminalIndex(nonTerminalType);

            if (index >= _firsts.Count)
            {
                return null;
            }
            return _firsts[index];
        }

        private List<string> GetFollow(string nonTerminalType, List<string> callStack)
        {
            List<string> result = new List<string>();

            if (callStack.Contains(nonTerminalType))
            {
                return new List<string>();
            }
            else
            {
                callStack.Add(nonTerminalType);
            }

            foreach (GrammarRule grammarRule in GrammarRules)
            {
                foreach (List<string> alternative in grammarRule.Alternatives)
                {
                    for (int i = 0; i < alternative.Count; i++)
                    {
                        if (alternative[i] == nonTerminalType)
                        {
                            // We've found the nonTerminalType in the grammarRule in the alternative in [i] position ([i] Symbol).
                            if (i + 1 < alternative.Count)
                            {
                                // Found our nonTerminalType is NOT the last Symbol in the alternative.
                                string nextSymbol = alternative[i + 1];

                                if (IsTypeOfNonterminal(nextSymbol))
                                {
                                    result.AddRange(GetFirst(nextSymbol));
                                    if ((AllowsEmptyString(nextSymbol, new List<string>())) && (nextSymbol != nonTerminalType))
                                    {
                                        result.AddRange(GetFollow(nextSymbol, callStack));
                                    }
                                }
                                if (IsTypeOfTerminal(nextSymbol))
                                {
                                    result.Add(nextSymbol);
                                }
                            }
                            else
                            {
                                // Found our nonTerminalType, that is THE last in the alternative.
                                if (grammarRule.ProductTypeName != nonTerminalType)
                                {
                                    result.AddRange(GetFollow(grammarRule.ProductTypeName, callStack));
                                }
                            }
                        } // Found our nonTerminalType.
                    } // Alternative symbols iterator.
                } // Alternative iterator.
            } // GrammarRules iterator.
            ClearEqualElements(result);
            return result;
        }

        private List<string> GetSavedFollow(string nonTerminalType)
        {
            int index = GetNonTerminalIndex(nonTerminalType);

            if (index >= _follows.Count)
            {
                return null;
            }
            return _follows[index];
        }

        private void FillFirstFollows()
        {
            for(int i = 0; i < _nonterminalTypes.Count; i++)
            {
                _firsts.Add(new List<string>());
                _firsts[i] = GetFirst(_nonterminalTypes[i]);

                _follows.Add(new List<string>());
                _follows[i] = GetFollow(_nonterminalTypes[i], new List<string>());
            }
        }

        private void InsertFirstFollow(int index, string nonTerminal)
        {
            _firsts.Insert(index, GetFirst(nonTerminal));
            _follows.Insert(index, GetFollow(nonTerminal, new List<string>()));
        }
        #endregion FirstFollow

        private bool CanStartWith(List<string> symbolList, string startingTerminalOrEmpty)
        {
            if (GetFirst(symbolList).Contains(startingTerminalOrEmpty))
            {
                return true;
            }
            if (startingTerminalOrEmpty == _emptyStringType)
            {
                //if ((symbolList.Count > 0) && (symbolList[0] == _emptyStringType))
                //{
                //    // Altermative starts with _emptyStringType (Eps).
                //    return true;
                //}
                if(AllowsEmptyString(symbolList, new List<string>()))
                {
                    return true;
                }
            }
            return false;
        }

        private Tuple<List<string>, double> WhichAlternative(GrammarRule grammarRule, string startingTerminalOrEmpty)
        {
            int count = 0;

            foreach (List<string> alternative in grammarRule.Alternatives)
            {
                if(CanStartWith(alternative, startingTerminalOrEmpty))
                {
                    return new Tuple<List<string>, double>(alternative,
                        GetGrammarRuleIndex(grammarRule) + ShiftComaLeft(count));
                }
                count++;
            }

            count = 0;
            if (AllowsEmptyString(grammarRule, new List<string>()))
            {
                // Determine, which alternative ALLOWS _emptyString.
                foreach (List<string> alternative in grammarRule.Alternatives)
                {
                    if (AllowsEmptyString(alternative, new List<string>()))
                    {
                        return new Tuple<List<string>, double>(alternative,
                            GetGrammarRuleIndex(grammarRule) + ShiftComaLeft(count));
                    }
                    count++;
                }
            }

            //if (AllowsEmptyString(grammarRule))
            //{
            //    return new Tuple<List<string>, double>(_emptyStringAlternative,
            //        GetGrammarRuleIndex(grammarRule) + ShiftComaLeft(count));
            //}

            return null;
        }

        private double ShiftComaLeft(int input)
        {
            double result = (double) input;

            while (Math.Truncate(result) > 0)
            {
                result /= 10;
            }
            return result;
        }

        private void BuildControlTable()
        {
            // Empy Stub.
            List<Tuple<List<string>, double>> controlTableRow = new List<Tuple<List<string>,double>>();
            for(int i = 0; i < _terminalTypes.Count; i++)
            {
                controlTableRow.Add(null);
            }

            List<List<Tuple<List<string>, double>>>  controlTable = new List<List<Tuple<List<string>, double>>>();

            for(int i = 0; i < _nonterminalTypes.Count; i++)
            {
                // Empy Stub.
                controlTable.Add(new List<Tuple<List<string>, double>>(controlTableRow));

                string nonTerminalType = _nonterminalTypes[i];
                List<string> firsts = GetFirst(nonTerminalType);

                foreach (string startingTerminal in firsts)
                {
                    // Table cell.
                    int terminalIndex = GetTerminalIndex(startingTerminal);

                    controlTable[i][terminalIndex] = WhichAlternative(GetGrammarRule(_nonterminalTypes[i]),
                        startingTerminal);
                }



                if (nonTerminalType == "sss18")
                { }



                if(AllowsEmptyString(nonTerminalType, new List<string>()))
                {
                    List<string> follows = GetFollow(nonTerminalType, new List<string>());

                    Tuple<List<string>, double> cellPattern = WhichAlternative(GetGrammarRule(_nonterminalTypes[i]),
                        _emptyStringType);

                    foreach (string startingTerminal in follows)
                    {
                        int terminalIndex = GetTerminalIndex(startingTerminal);

                        //controlTable[i][terminalIndex] = cellPattern;
                        if (controlTable[i][terminalIndex] == null)
                        {
                            controlTable[i][terminalIndex] = cellPattern;
                        }
                    }
                }
            }
            _controlTable = controlTable;
        }

        /// <summary>
        /// Tuple<text, lexemeType>
        /// </summary>
        /// <param name="inputLexemes">The product of Lexical analyzer.Tuple<text, type></param>
        /// <returns>Alternatives.</returns>
        public List<double> Process(List<Tuple<string, string>> inputLexemes)
        {
            List<double> result = new List<double>();
            List<Tuple<string, string>> text = new List<Tuple<string, string>>(inputLexemes);
            List<string> stack = new List<string>(new List<string>() { GrammarRules[0].ProductTypeName });
            int lexemeIndex = 0;

            text.Add(new Tuple<string, string>("", EndOfLine));

            while (true)
            {
                if (stack.Count < 4)
                { }
                if (IsTypeOfTerminal(stack[0]) || stack[0].Equals(_emptyStringType))
                {
                    if (stack[0] == text[lexemeIndex].Item2)
                    {
                        if (stack[0] == EndOfLine)
                        {
                            return result;
                        }
                        else
                        {
                            // Викид =)
                            stack.RemoveAt(0);
                            lexemeIndex++;
                            if (lexemeIndex == 40)
                            { }
                        }
                    }
                    else if (stack[0] == _emptyStringType)
                    {
                        stack.RemoveAt(0);
                    }
                    else
                    {
                        result.Add(-1);
                        return result;
                    }
                } // Top of the stack is terminal.
                else
                {
                    // Have nonTerminal on the top of stack.
                    string nonTerminal = stack[0];
                    List<string> alternative;
                    Tuple<List<string>, double> tableCell = GetTableCell(nonTerminal, text[lexemeIndex].Item2);

                    if (tableCell == null)
                    {
                        // Cell is empty.
                        result.Add(-1);
                        return result;
                    }
                    Tuple<List<string>, double> cell = GetTableCell(nonTerminal, text[lexemeIndex].Item2);
                    alternative = cell.Item1;
                    stack.RemoveAt(0);
                    stack.InsertRange(0, alternative);
                    result.Add(cell.Item2);
                }
            }
        }

        public void PrintGrammarRules()
        {
            int count = 0;

            foreach (GrammarRule rule in GrammarRules)
            {
                Console.Write(count.ToString() + "  ");
                rule.Print();
                count++;
            }
        }

        public void PrintGrammarRule(int grammarIndex)
        {
            if ((grammarIndex < 0) || (grammarIndex >= GrammarRules.Count))
            { 
                Console.WriteLine(-1); 
            }
            else
            {
                GrammarRules[grammarIndex].Print();
            }
        }

        public void PrintControlTable()
        {
            PrintTerminals();
            //foreach(List<Tuple<List<string>, double>> row in _controlTable)
            for(int i = 0; i < _nonterminalTypes.Count; i++)
            {
                List<Tuple<List<string>, double>> row = _controlTable[i];

                StateMachine.PrintStringCertainWidth(_nonterminalTypes[i], PrintingItemWidth);
                foreach (Tuple<List<string>, double> cell in row)
                {
                    if (cell != null)
                    {
                        //StateMachine.PrintStringCertainWidth(cell.Item2.ToString(), PrintingItemWidth);
                        StateMachine.PrintStringCertainWidth(ListToOneString(cell.Item1), PrintingItemWidth);
                    }
                    else
                    {
                        StateMachine.PrintStringCertainWidth("", PrintingItemWidth);
                    }
                }
                Console.WriteLine();
            }
        }

        private void PrintTerminals()
        {
            StateMachine.PrintStringCertainWidth("", PrintingItemWidth);
            foreach(string s in _terminalTypes)
            {
                StateMachine.PrintStringCertainWidth(s, PrintingItemWidth);
            }
            StateMachine.PrintLineSeparator();
        }

        private static string ListToOneString(List<string> list)
        {
            string result = "";

            foreach (string s in list)
            {
                result += s + " ";
            }
            return result;
        }
    }
}