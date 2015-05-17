using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class Symbol
    {
        protected string _text;
        protected string _type;

        public string Text
        {
            get { return _text; }
        }

        public string Type
        {
            get { return _type; }
        }

        public Symbol(string type, string text)
        {
            _type = type;
            _text = text;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            Symbol s = obj as Symbol;
            if ((System.Object)s == null)
                return false;

            return (_text == s._text) && (_type == s._type);
        }

        public static bool SameType(Symbol symbol1, Symbol symbol2)
        {
            return (symbol1.GetType() == symbol2.GetType()) && (symbol1.Type == symbol2.Type);
        }
        public bool SameType(Symbol symbol)
        {
            return SameType(this, symbol);
        }
    }
}
