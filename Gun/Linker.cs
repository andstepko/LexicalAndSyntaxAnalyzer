using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class Linker
    {
        private Gramatics _gramatics;
        private StateMachine _stateMachine;
        private Executor _executor;
        private List<Tuple<string, string>> _lexemes;

        private bool _showInputText = false;
        private bool _showLexemes = false;
        private bool _showGrammarRules = false;
        private bool _showControlTable = false;
        private bool _showSyntaxOutput = false;
        private bool _showResult = true;

        public Linker (List<Tuple<string, string>> typeRegExps, params string[] grammarRulesStrings)
        {
            _stateMachine = MachineBuilder.Build(typeRegExps);
            _gramatics = new Gramatics(grammarRulesStrings.ToList(), GetFirstItems(typeRegExps), "Eps");
        }

        public void SetPerformSettings(bool showInputText, bool showLexemes, bool showGrammarRules,
            bool showControlTable, bool showSyntaxOutput, bool showResult)
        {
            _showInputText = showInputText;
            _showLexemes = showLexemes;
            _showGrammarRules = showGrammarRules;
            _showControlTable = showControlTable;
            _showSyntaxOutput = showSyntaxOutput;
            _showResult = showResult;
        }

        public void Perform(string text)
        {
            List<double> doublesList = new List<double>();
            bool validated;

            if (_showInputText)
            { 
                PrintText(text); 
            }

            doublesList = Validate(text);
            validated = doublesList[doublesList.Count - 1] != -1;

            if (_showLexemes)
            {
                PrintLexemes();
            }

            if (_showGrammarRules)
            {
                PrintGrammarRules();
            }

            if (_showControlTable)
            {
                PrintControlTable();
            }

            if(_showSyntaxOutput)
            {
                PrintSyntaxOutput(doublesList);
            }

            if (_showResult)
            {
                PrintResult(validated);
            }
        }

        private void PrintText(string text)
        {
            Console.WriteLine("InputText: ");
            Console.WriteLine(text);
            Console.WriteLine();
        }

        private void PrintLexemes()
        {
            Console.WriteLine("Lexemes:");
            _executor.PrintLexemes();
            Console.WriteLine();
        }

        public void PrintGrammarRules()
        {
            Console.WriteLine("           Grammar rules:");
            Console.WriteLine();
            _gramatics.PrintGrammarRules();
            Console.WriteLine();
        }

        public void PrintControlTable()
        {
            Console.WriteLine("                 Control Table:");
            Console.WriteLine();
            _gramatics.PrintControlTable();
            Console.WriteLine();
            Console.WriteLine();
        }

        private void PrintSyntaxOutput(List<double> doublesList)
        {
            Console.WriteLine("     Syntax Output:");
            PrintDoubles(doublesList);
            Console.WriteLine();
        }

        private void PrintResult(bool validated)
        {
            Console.WriteLine("String is {0}validated{1}", validated ? "" : "NOT ", validated ? "!!!" : ". =(");
        }

        public List<double> Validate(string text)
        {
            List<double> doublesList = new List<double>();

            _executor = new Executor(_stateMachine, text);
            _lexemes = _executor.Execute();

            doublesList = _gramatics.Process(_lexemes);

            return doublesList;
        }

        private List<string> GetFirstItems(List<Tuple<string, string>> inputList)
        {
            List<string> result = new List<string>();

            foreach(Tuple<string, string> tuple in inputList){
                result.Add(tuple.Item1);
            }
            return result;
        }

        private List<string> GetSecondItems(List<Tuple<string, string>> inputList)
        {
            List<string> result = new List<string>();

            foreach (Tuple<string, string> tuple in inputList)
            {
                result.Add(tuple.Item2);
            }
            return result;
        }

        private void PrintDoubles(List<double> list)
        {
            foreach (double d in list)
            {
                PrintDouble(d);
            }
            Console.WriteLine();
        }

        private void PrintDouble(double d)
        {
            //Console.Write(d.ToString(String.Format("F1")) + " ");
            Console.Write(d.ToString(String.Format("F1")) + " ");
            _gramatics.PrintGrammarRule((int)Math.Truncate(d));
        }
    }
}