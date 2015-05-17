using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class Program
    {
        static void Main(string[] args)
        {
            //machine1.PrintTransitions();
            //Console.WriteLine();
            //Console.WriteLine();

            //machine2.PrintTransitions();
            //Console.WriteLine();
            //Console.WriteLine();

            //StateMachine machine3 = machine1 | machine2;
            //machine3.PrintTransitions();
            //Console.WriteLine();
            //Console.WriteLine();

            //machine3 = StateMachine.Cycle(machine3);
            //machine3.PrintTransitions();
            //Console.WriteLine();
            //Console.WriteLine();

            //StateMachine determined = Determiner.Determined(machine1);
            //determined.PrintTransitions();
            //determined.PrintIsDeterminated();

            //StateMachine m = MachineBuilder.Build(new List<Tuple<string, string>>()
            //{
            //    new Tuple<string, string>("Отчество", "[А-Я][а-я]*[в&и&ч|в&н&а]"),
            //    new Tuple<string, string>("Имя или Фамилия", "[А-Я][а-я]*"),                
            //    new Tuple<string, string>("Год", "19&[0-9]&[0-9]|20&[0-9]&[0-9]"),
            //});

            //string text = "Иванов Иван Иванович 1977 Петрова Евдокия Олеговна 2010";

            //StateMachine m = MachineBuilder.Build(new List<Tuple<string, string>>()
            //{
            //    new Tuple<string, string>("Key words", "do|for|foreach|while|new|switch|case|break|continue|default|static|abstract" +
            //        "|class|private|public|protected|interface|operator|#region|#endregion"),
            //    new Tuple<string, string>("Operators", "[=;{}\\(\\)\\[\\]<>\\+\\-\\*^\\|]|<=|>=|\\+\\+|\\-\\-|\\|\\||==|\\&\\&|\\+=|\\-="),
            //    new Tuple<string, string>("Vars", "[a-zA-Z_$][a-zA-Z0-9_]*"),
            //    new Tuple<string, string>("Numbers", "[0-9]+.*[0-9]*e*[0-9]*"),
            //});

            //string text = "_asdf = 100; class while a[01223] i += 55; ";

            //Executor executor = new Executor(m, text);
            //executor.Execute();
            //executor.PrintLexemes();


            Linker linker = new Linker(
                new List<Tuple<string, string>>()
                {
                        //new Tuple<string, string>("Key words", "do|for|foreach|while|new|switch|case|break|continue|default|static|abstract" +
                        //    "|class|private|public|protected|interface|operator|#region|#endregion"),
                            new Tuple<string, string>("key", "switch|case|break|continue" +
                                        "|operator|#region|#endregion"),
                            new Tuple<string, string>("new", "new"),
                            new Tuple<string, string>("null", "null"),
                            new Tuple<string, string>("return", "return"),
                            new Tuple<string, string>("for", "for"),
                            new Tuple<string, string>("class", "class"),
                            new Tuple<string, string>("interface", "interface"),
                            new Tuple<string, string>("accessLevel", "default|private|public|protected"),
                            new Tuple<string, string>("static", "static"),
                            new Tuple<string, string>("abstract", "abstract"),
                            new Tuple<string, string>("virtual", "virtual"),
                            new Tuple<string, string>("override", "override"),
                            new Tuple<string, string>("foreach", "foreach"),
                            new Tuple<string, string>("in", "in"),
                            new Tuple<string, string>("if", "if"),
                            new Tuple<string, string>("else", "else"),
                            new Tuple<string, string>("do", "do"),
                            new Tuple<string, string>("while", "while"),
                            new Tuple<string, string>("t_f", "true|false"),
                            new Tuple<string, string>("type", "bool|int|double|float|long|char|string|void"),
                            //new Tuple<string, string>("Operators", "[=;{}\\(\\)\\[\\]<>\\+\\-\\*^\\|]|<=|>=|\\" +
                            //    "+\\+|\\-\\-|\\|\\||==|\\&\\&|\\+=|\\-="),
                            new Tuple<string, string>("assi", "\\+=|\\-=|="),
                            new Tuple<string, string>("aSSi", "\\+\\+|\\-\\-"),
                            //new Tuple<string, string>("oper", "[\\+\\-\\*^\\|]|\\+\\+|\\-\\-"),
                            new Tuple<string, string>("oper", "^"),
                            new Tuple<string, string>("+or-", "\\+|\\-"),
                            new Tuple<string, string>("%", "%"),
                            new Tuple<string, string>("*", "\\*"),
                            new Tuple<string, string>("/", "/"),
                            new Tuple<string, string>("{", "{"),
                            new Tuple<string, string>("}", "}"),
                            new Tuple<string, string>("[", "\\["),
                            new Tuple<string, string>("]", "\\]"),
                            new Tuple<string, string>("(", "\\("),
                            new Tuple<string, string>(")", "\\)"),
                            new Tuple<string, string>(";", ";"),
                            new Tuple<string, string>(":", ":"),
                            new Tuple<string, string>(",", ","),
                            new Tuple<string, string>(".", "."),
                            new Tuple<string, string>("cmp", "<=|>=|<|>|=="),
                            new Tuple<string, string>("boolOperBinary", "\\|\\||\\&\\&|\\&|\\|"),
                            new Tuple<string, string>("boolOperUnary", "!"),
                            //new Tuple<string, string>("className", "[A-Z][a-zA-Z0-9_]*"),
                            new Tuple<string, string>("var", "[a-zA-Z_$][a-zA-Z0-9_]*"),
                            new Tuple<string, string>("num", "[0-9]+.*[0-9]*")
                },

                // 53 GrammarRules.
                "GOD                            God GOD | #",
                "God                            CLASS",

                "CLASS                          CLASs | accessLevel CLASs",
                "CLASs                          static CLAss | abstract CLAss | CLAss",
                "CLAss                          class var CLass ",
                "CLass                          { IN_A_CLASS_PLURAL } ",

                "IN_A_CLASS_PLURAL              IN_A_CLASS   IN_A_CLASS_PLURAL | Eps",
                "IN_A_CLASS                     METHOD | Declaration | Declaration&Assignment",

                "METHOD                         accessLevel METHOd | METHOd ",
                "METHOd                         static METHod | abstract METHod | virtual METHod | override METHod | METHod",
                "METHod                         type METhod | var METhod ",
                "METhod                         var ( ArgumDecl_Plural_Eps ) { MEthod } ",
                "MEthod                         IN_A_METHOD_PLURAL",

                "IN_A_METHOD_PLURAL             IN_A_METHOD    IN_A_METHOD_PLURAL | Eps",
                "IN_A_METHOD                    FOR | FOREACH | WHILE | DO_WHILE | IF | Plural_Statement",

                "FOR                            for ( Statement_Eps ; Bool_Plural_Statement_Eps ; Statement_Eps ) { IN_A_METHOD_PLURAL }",
                "FOREACH                        foreach ( type var in var ) { IN_A_METHOD_PLURAL }",
                "WHILE                          while ( Bool_Pl_Sing_Statement ) { IN_A_METHOD_PLURAL }",
                "DO_WHILE                       do { IN_A_METHOD_PLURAL }  while ( Bool_Pl_Sing_Statement )",
                "IF                             If | If  ELSE",
                "If                             if ( Bool_Pl_Sing_Statement ) { IN_A_METHOD_PLURAL }",
                "ELSE                           Else_If | Else",
                "Else_If                        else if ( Bool_Pl_Sing_Statement ) { IN_A_METHOD_PLURAL }",
                "Else                           else { IN_A_METHOD_PLURAL }",

                "Plural_Statement_Eps           Plural_Statement | Eps",
                "Plural_Statement               Statement ; Plural_Statement_Eps | Statement ;",
                "Statement_Eps                  Statement | Eps",
                "Statement                      Assignment | Declaration | Declaration&Assignment | Return",

                "Return                         return null | return Operand_new | return Expression | return Bool_Pl_Sing_Statement",

                "Bool_Plural_Statement_Eps      Bool_Pl_Sing_Statement | Eps",
                "Bool_Pl_Sing_Statement         Bool_Plural_Statement | Bool_Statement",
                "Bool_Plural_Statement          Bool_Statement() boolOperBinary Bool_Plural_Statement | Bool_Statement()",
                "Bool_Statement()               ( Bool_Statement ) | Bool_Value",

                "Bool_Statement                 Operand cmp Operand",
                "Bool_Value                     Bool_Const | Operand",
                "Bool_Const                     t_f",

                "Assignment                     Memory_Cell assi Operand_new | Memory_Cell aSSi | aSSi Memory_Cell",
                "Declaration                    type Memory_Cell | var var",
                "Declaration&Assignment         type Memory_Cell assi Operand_new | var var assi Operand_new",
                
                "Operand_new                    Operand | New",
                "New                            new var ( Argum_Plural_Eps )",
                "Operand                        Memory_Cell | Const_Expression",

                "Memory_Cell                    Memory_Cell_Simple  Memory_Cell_Next_Eps",
                "Memory_Cell_Next_Eps           . Memory_Cell_Simple Memory_Cell_Next_Eps | Eps",
                "Memory_Cell_Simple             var | var [ Operand ]",
                
                "Const_Expression               Const_Expression +or- Const_Expression | num",
                
                "Expression                     Operand_new Expression_Next_Eps",
                "Expression_Next_Eps            +or- Operand_new Expression_Next_Eps | Eps",

                "ArgumDecl_Plural_Eps           ArgumDecl ArgumDecl_Next_Eps | Eps",
                "ArgumDecl_Next_Eps             , ArgumDecl ArgumDecl_Next_Eps | Eps",
                "ArgumDecl                      type var | var var",

                "Argum_Plural_Eps               Operand_new Argum_Next_Eps | Eps",
                "Argum_Next_Eps                 , Operand_new Argum_Next_Eps | Eps"
            );

            //linker.SetPerformSettings(false, true, true, true, true, true);
            //linker.SetPerformSettings(false, false, true, true, true, true);
            linker.SetPerformSettings(false, false, true, false, true, true);
            //linker.SetPerformSettings(false, false, false, false, true, true);
            //linker.SetPerformSettings(false, false, false, false, false, true);


            //linker.Perform("for(int i = 0;true || (a > b) || (15 < 100) && (b <= 10) || (b >= 100) || false && b;i += 1)" +
            //        "{float j = 2.5; double k = 1.5 + 21; j++; --l; if(b){int k[m[i]] = 5 - 8; j++;} if(c > 17 + 3){int k[m[i]] = 5 - 8; j++;} }");
            
            //linker.Perform("foreach(int i in numbers){float j = 2.5; double k = 1.5 + 21; j++; --l;}");

            linker.Perform("private static class MyClass {" +
                                "public abstract string MyMethod(int i1, double d1, int i2)" +
                                "{" +
                                    "Point p = new Point ( new Point( new Point(100, 150) ) ) ;" +
                                    "while(true)" +
                                    "{" +
                                        "Line l = new Line();" +
                                        "l.Points[0].X = b;" +
                                    "}" +
                                    "return new Point() + new Point(10, 15);" +
                                "}" +

                                "public override bool SumMul(int i1, int i2)" +
                                "{" +
                                    "if(i1 > i2)" +
                                    "{" +
                                        "return i1 + i2;" +
                                    "}" +
                                    "else" +
                                    "{" +
                                        "return i1 - 15;" +
                                    "}" +
                                "}" +
                            "}");

            //linker.Perform("private class MyClass {" + 
            //                    "for(int i = 0;true || (a > b) || (15 < 100) && (b <= 10) || (b >= 100) || false && b;i += 1)" +
            //                    "{" +
            //                        "float j = 2.5;" + 
            //                        "double k = 1.5 + 21;" + 
            //                        "j++; --l;" + 
            //                        "if(b)" + 
            //                        "{" + 
            //                            "int k[m[i]] = 5 - 8; j++;" + 
            //                        "}" +
            //                        "if(c > 17 + 3)" +
            //                        "{" +
            //                        "int k[m[i]] = 5 - 8; j++;" +
            //                        "}" +
            //                    "}" +
            //                    "foreach (int i in numbers)" + 
            //                    " {" +
            //                        "float j = 2.5;" +
            //                        "double k = 1.5 + 21;" +
            //                        "j++;" +
            //                        "--l;" +
            //                    "}" +
            //                "}");

            //linker.Perform("private class MyClass {" +
            //                    "while ((a >b) || (c < d))" +
            //                    "{" +
            //                        "if(b)" +
            //                        "{" +
            //                            "while(false)" +
            //                            "{" +
            //                                "int k[m[i]] = 5 - 8; j++;" +
            //                                "foreach (int i in numbers)" +
            //                                " {" +
            //                                    "float j = 2.5;" +
            //                                    "double k = 1.5 + 21;" +
            //                                    "j++;" +
            //                                    "--l;" +
            //                                "}" +
            //                            "}" +
            //                        "}" +
            //                        "do" +
            //                        "{" +
            //                            "int k[m[i]] = 5 - 8; j++;" +
            //                        "}" +
            //                        "while(true || (a > b) || (15 < 100) && (b <= 10) || (b >= 100) || false && b)" +
            //                    "}" +
            //                "}");

            Console.Read();
        }
    }
}
