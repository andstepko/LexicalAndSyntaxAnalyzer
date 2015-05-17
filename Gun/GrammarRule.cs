using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class GrammarRule
    {
        protected string _productTypeName;
        protected List<List<string>> _altermatives;

        public string ProductTypeName
        {
            get
            { return _productTypeName; }
            set { _productTypeName = value; }
        }

        // MAKE INTELLIGETN SETTER!!
        public List<List<string>> Alternatives
        {
            get
            { return _altermatives; }
            set
            { _altermatives = value; }
        }

        public bool IsLeftRecursive
        {
            get{
                foreach(List<string> alternative in _altermatives)
                {
                    if ((alternative.Count > 0) && (alternative[0].Equals(_productTypeName)))
                    { return true; }
                }
                return false;
            }
        }

        public GrammarRule(string productTypeName, List<List<string>> alternativesTypes)
        {
            _productTypeName = productTypeName;
            _altermatives = alternativesTypes;
        }

        //public GrammarRule(string productTypeName) : this(productTypeName, new List<List<string>>()){}

        public static List<GrammarRule> GrammarRules(List<string> ruleStrings)
        {
            List<GrammarRule> result = new List<GrammarRule>();

            foreach (string ruleString in ruleStrings)
            {
                result.Add(new GrammarRule(ruleString));
            }
            return result;
        }

        public GrammarRule(string text)
        {
            //List<string> words = new List<string>();
            string[] words = text.Split(new char[] {' '});
            int i = 1;

            List<string> wordsList = words.ToList();
            wordsList.RemoveAll(EmptyString);
            words = wordsList.ToArray();

            _productTypeName = words[0];
            int alternativeCount = 0;
            _altermatives = new List<List<string>>();
            _altermatives.Add(new List<string>());
            for (; i < words.Length; i++)
            {
                if (words[i] == "|")
                {
                    alternativeCount++;
                    _altermatives.Add(new List<string>());
                }
                else
                {
                    _altermatives[alternativeCount].Add(words[i]);
                }
            }
        }

        private static bool EmptyString(String s)
        {
            bool result = s.Equals("");
            return result;
        }

        public override string ToString()
        {
            string result = ProductTypeName + "  ==>  ";

            foreach (List<string> alternative in _altermatives)
            {
                foreach (string symbol in alternative)
                {
                    result += symbol + " ";
                }
                result += "| ";
            }
            return result;
        }

        public void Print()
        {
            Console.WriteLine(this.ToString());
        }
    }
}