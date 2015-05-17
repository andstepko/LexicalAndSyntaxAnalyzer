using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class StateMachine
    {
    #region Vars
        private static int _defaultNoIndex = -1;
        private static char _defaultNoCommand = '@';
        private static String _defaultNoFinalType = "NO_TYPE";
        private static int _defaultPrintBlockLength = 5;
        private static List<int> _defaultEmptyTransition = new List<int>() { -1 };
        private static char _defaultPrintingSpace = ' ';
        private static int _defaultFinishState = 0;

        private int _startingVertex;
        //private int _devilVertex;
        private List<String> _finalTypes = new List<string>();
        private List<Tuple<int, int>> _finalVertexes = new List<Tuple<int,int>>();
        private List<char> _commands=  new List<char>();
        List<List<List<int>>> _transitions = new List<List<List<int>>>();
    #endregion Vars

    #region Constructors
        public StateMachine(int inputStartVertex,
            List<char> inputcommands,
            List<Tuple<int, int>> inputFinalVertexes, List<String> inputFinishTypes,            
            List<Tuple<int, char, int>> inputTransitions) // 1) sourceVertex; 2) command; 3) destinationVertex
        {
            //_finalTypes = new List<string>();
            //_finalVertexes = new List<Tuple<int, int>>();
            //_commands = new List<char>();
            //_transitions = new List<List<List<int>>>();

            StartingVertex = inputStartVertex;
            _commands = inputcommands;
            _finalTypes = new List<String>(inputFinishTypes);

            // Fill FinalVertexes one by one.
            AddFinalRange(inputFinalVertexes);

            // Fill transitions one by one.
            foreach (Tuple<int, char, int> transition in inputTransitions)
            {
                AddTransition(transition.Item1, transition.Item2, transition.Item3);
            }
        }

        // Without StartingVertex. (StartingVertex = 0)
        public StateMachine(List<char> inputcommands,
            List<Tuple<int, int>> inputFinalVertexes, List<String> inputFinishTypes,
            List<Tuple<int, char, int>> inputTransitions) // 1) sourceVertex; 2) command; 3) destinationVertex
        : this(0,
            inputcommands,
            inputFinalVertexes, inputFinishTypes,            
            inputTransitions) // 1) sourceVertex; 2) command; 3) destinationVertex
        {}

        // Without initial startingTerminalOrEmpty alphabet(commands).
        public StateMachine(List<Tuple<int, int>> inputFinalVertexes, List<String> inputFinishTypes,
            List<Tuple<int, char, int>> inputTransitions) // 1) sourceVertex; 2) command; 3) destinationVertex
            : this(0,
                new List<char>(),
                inputFinalVertexes, inputFinishTypes,
                inputTransitions) // 1) sourceVertex; 2) command; 3) destinationVertex
        { }

        public StateMachine(List<Char> terminalAlphabet)
        {
            _commands = new List<char>(terminalAlphabet);
            StartingVertex = 0;
        }

        public StateMachine(StateMachine machine)
        {
            StartingVertex = machine.StartingVertex;
            _commands = machine._commands;
            _finalTypes = machine._finalTypes;
            _finalVertexes = machine._finalVertexes;
            _transitions = machine._transitions;
        }

        public StateMachine(char command, List<string> finalTypes)
        {
            StartingVertex = 0;
            _finalTypes = finalTypes;
            _finalVertexes = new List<Tuple<int, int>>() { new Tuple<int, int>(1, 0) };
            AddCommand(command);
            AddVertex(1);
            AddTransition(0, command, 1);
        }

        public StateMachine(char command) : this(command, new List<string>() { _defaultNoFinalType }) { }
    #endregion

    #region Properties
        public List<char> TerminalAlphabet
        {
            get
            {
                return new List<char>(_commands);
            }
        }

        public List<int> Vertexes
        {
            get
            {
                List<int> result = new List<int>();

                for (int i = 0; i < _transitions.Count; i++)
                {
                    result.Add(i);
                }
                return result;
            }
        }

        public List<int> FinalVertexes
        {
            get
            {
                List<int> result = new List<int>();

                foreach(Tuple<int, int> tuple in _finalVertexes)
                {
                    result.Add(tuple.Item1);
                }
                return result;
            }
        }

        public List<string> FinalTypesDirty
        {
            get
            {
                return _finalTypes;
            }
            set
            {
                _finalTypes = value;
            }
        }

        public int AlphabetLength
        {
            get
            {
                return _commands.Count;
            }
        }

        public int DefaultNoIndex
        {
            get
            {
                return _defaultNoIndex;
            }
        }

        public int StartingVertex
        {
            get
            {
                return _startingVertex;
            }
            set
            {
                AddVertex(value);
                _startingVertex = value;
            }
        }

        public bool IsDetermined
        {
            get 
            {
                foreach (List<List<int>> vertexTransition in _transitions)
                {
                    foreach (List<int> transition in vertexTransition)
                    {
                        if (transition.Count > 1)
                            return false;
                    }
                }

                return true;
            }
        }
    #endregion

    #region Print
        public static void Print(StateMachine machine)
        {
            machine.PrintTransitions();
        }

        public void PrintCommands()
        {
            // Print the startingTerminalOrEmpty alphabet.
            foreach (char c in _commands)
            {
                Console.Write(c + " ");
            }
        }

        public void PrintTransitions()
        {
            // Print first line.
            PrintEmpty();
            foreach (char c in _commands)
            {
                PrintChar(c);
                PrintEmpty();
            }
            PrintLineSeparator();
            PrintLineSeparator();

            for (int i = 0; i < _transitions.Count; i++)
            {
                // Print 1 VertexTransition in 1 line.
                // Print next index of Vertex.
                PrintInteger(i);

                foreach (List<int> transition in _transitions[i])
                {
                    PrintRangeInteger(transition);
                    PrintEmpty();
                }
                PrintLineSeparator();
            }
        }

        public void RrintIsDetermined()
        {
            if (IsDetermined)
            { Console.WriteLine("The machine is {0}determined", ""); }
            else
            { Console.WriteLine("The machine is {0}determined", "NOT "); }
        }

        public static void PrintInteger(int input)
        {
            PrintStringCertainWidth(input.ToString(), _defaultPrintBlockLength);
        }

        public static void PrintRangeInteger(List<int> input)
        {
            string result = "";
            int i = 0;

            for (; i < input.Count - 1; i++ )
            {
                result += input[i].ToString();
                result += " ";
            }
            if (i < input.Count)
            {
                result += input[i].ToString();
            }

            PrintStringCertainWidth(result, _defaultPrintBlockLength);
        }

        public static void PrintChar(char input)
        {
            PrintStringCertainWidth(input.ToString(), 5);
        }

        public static void PrintStringCertainWidth(string inputInfo, int inputWidth)
        {
            int emptySpaces = inputWidth - inputInfo.Length;

            PrintSpaces((emptySpaces / 2) + (emptySpaces % 2));
            Console.Write(inputInfo);
            PrintSpaces(emptySpaces / 2);
        }

        public static void PrintEmpty()
        {
            PrintSpaces(_defaultPrintBlockLength);
        }

        public static void PrintSpaces(int inputNumber)
        {
            for (int i = 0; i < inputNumber; i++)
            {
                Console.Write(_defaultPrintingSpace);
            }
        }

        public static void PrintLineSeparator()
        {
            Console.WriteLine();
        }
    #endregion

    #region Getters
        public char GetCommandByIndex(int index)
        {
            if (index < _commands.Count)
            {
                return _commands[index];
            }
            return _defaultNoCommand;
        }
        public int GetIndexByCommand(char inputChar)
        {
            if (_commands.Contains(inputChar))
            {
                return _commands.IndexOf(inputChar);
            }
            else
            {
                return _defaultNoIndex;
            }
        }

        public String GetFinalTypeByIndex(int inputIndexOfType)
        {
            if (inputIndexOfType < _finalTypes.Count)
            {
                return _finalTypes[inputIndexOfType];
            }
            else
            {
                return _defaultNoFinalType;
            }
        }

        public int GetIndexOfFinishType(String typeName)
        {
            if (_finalTypes.Contains(typeName))
            {
                return _finalTypes.IndexOf(typeName);
            }
            else
            {
                return _defaultNoIndex;
            }
        }

        public int GetDestinationVertex(int inputSourceVertexIndex, char inputCommand)
        {
            return GetDestinationVertex(inputSourceVertexIndex, GetIndexByCommand(inputCommand));
        }
        public int GetDestinationVertex(int inputSourceVertexIndex, int inputCommandIndex)
        {
            List<int> list = GetDestinationVetexes(inputSourceVertexIndex, inputCommandIndex);
            if(list.Count < 1)
            {
                return DefaultNoIndex;
            }
            return list[0];
        }

        public List<int> GetDestinationVetexes(int sourceVertex, int command)
        {
            if (_transitions.Count <= sourceVertex)
            {
                return new List<int>();
            }

            if (!CommandExists(command))
            {
                return new List<int>();
            }

            return _transitions[sourceVertex][command];
        }

        public int GetIndexOfTypeOfFinalVertex(int inputVertex)
        {
            if (!IsFinalVertex(inputVertex))
            {
                return _defaultNoIndex;
            }
            else
            {
                int i = 0;
                for (; i < _finalVertexes.Count; i++)
                {
                    if (_finalVertexes[i].Item1 == inputVertex)
                    {
                        break;
                    }
                }
                return _finalVertexes[i].Item2;
            }
        }
    #endregion

        public bool SetAllFinalAs(int typeIndex, string typeName)
        {
            if (SetAllFinalAs(typeIndex))
            {
                _finalTypes = new List<string>() { typeName };
                return true;
            }
            return false;
        }
        public bool SetAllFinalAs(int typeIndex)
        {
            if (_finalVertexes.Count == 0)
            {
                return false;
            }
            for(int i = 0; i < _finalVertexes.Count; i++)
            {
                _finalVertexes[i] = new Tuple<int, int>(_finalVertexes[i].Item1, typeIndex);
            }
            //_finalTypes = new List<string>() { typeName };
            return true;
        }

    #region is__ and Existance checks
        public bool IsFinalVertex(int input)
        {
            for (int i = 0; i < _finalVertexes.Count; i++)
            {
                if (_finalVertexes[i].Item1 == input)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsStartingVertex(int input)
        {
            return input == StartingVertex;
        }

        //public bool IsVertexAchivable(int vertex)

        public bool CommandExists(char inputCommand)
        {
            int resultIndex;

            resultIndex = GetIndexByCommand(inputCommand);
            if (resultIndex != _defaultNoIndex)
            {
                // Such a command exists.
                return true;
            }
            return false;
        }
        public bool CommandExists(int inputIndex)
        {
            return ((inputIndex != _defaultNoIndex) && (inputIndex < _commands.Count) && (inputIndex > -1));
        }

        public bool VertexExists(int inputIndex)
        {
            return (_transitions.Count > inputIndex);
        }

        public bool VertexesExist(List<int> vertexes)
        {
            bool result = true;

            foreach (int vertex in vertexes)
            {
                result &= VertexExists(vertex);
            }
            return result;
        }

        public bool FinishTypeExists(int index)
        {
            return _finalTypes.Count > index;
        }
        public bool FinishTypeExists(string typeName)
        {
            int index = GetIndexOfFinishType(typeName);

            if (index == _defaultNoIndex)
            {
                return false;
            }
            return true;
        }
    #endregion

    #region Add Methods
        //public bool AddTransitions(int sourceVertex, char command, List<int> inputDestinationVertexes)
        //{
        //    // Adds transitions from the souecVertex to ALL of the destinationVertexes by command.
        //    bool result = false;
        //    List<int> destinationVertexes = new List<int>(inputDestinationVertexes);

        //    foreach (int destinationVertex in destinationVertexes)
        //    {
        //        result = result | AddTransition(sourceVertex, command, destinationVertex);
        //    }
        //    return result;
        //}

        public bool AddTransition(int sourceVertex, char command, int destinationVertex)
        {
            // Adds a new transition from the sourceVertex to ONE destinationVertex.
            int commandIndex;

            AddCommand(command);
            commandIndex = GetIndexByCommand(command);
            if (!VertexExists(sourceVertex))
            {
                AddVertex(sourceVertex);
            }
            if (!VertexExists(destinationVertex))
            {
                if (destinationVertex != _defaultNoIndex)
                { AddVertex(destinationVertex); }
            }

            if (_transitions[sourceVertex][commandIndex].Contains(destinationVertex))
            {
                // Such a transition already exists.
                return false;
            }
            else
            {
                if ((_transitions[sourceVertex][commandIndex].Count == 1) &&
                    (_transitions[sourceVertex][commandIndex][0] == _defaultEmptyTransition[0]))
                {
                    // It is gonna be the first transition for THIS vertex and THIS command.
                    _transitions[sourceVertex][commandIndex] = new List<int>();
                }

                _transitions[sourceVertex][commandIndex].Add(destinationVertex);
                return true;
            }
        }
        public bool AddTransition(int sourceVertex, int inputCommand, int inputDestinationVertex)
        {
            if (!CommandExists(inputCommand))
            {
                return false;
            }
            return AddTransition(sourceVertex, GetCommandByIndex(inputCommand), inputDestinationVertex);
        }
        
        public bool AddTransitions(int inputSourceVertex, List<char> inputCommands, int inputDestinationVertes)
        {
            bool result = false;

            foreach (char command in inputCommands)
            {
                result |= AddTransition(inputSourceVertex, command, inputDestinationVertes);
            }
            return result;
        }
        public bool AddTransitions(int sourceVertex, char firstChar, char lastChar, int destinationVertex)
        {
            bool result = false;

            for (char c = firstChar; c <= lastChar; c++)
            {
                result |= AddTransition(sourceVertex, c, destinationVertex);
            }
            return result;
        }

        public bool AddExcaptTransitions(int inputSourceVertex, char inputCommand, int inputDestinationVertex)
        {
            bool result = false;

            foreach (char command in _commands)
            {
                if (command != inputCommand)
                {
                    result |= AddTransition(inputSourceVertex, command, inputDestinationVertex);
                }
            }
            return result;
        }

        public bool AddFinal(int index)
        {
            return AddFinal(index, _defaultFinishState);
        }
        public bool AddFinal(int vertexIndex, int typeIndex)
        {
            // Works only if such a finalVertex hasn't existed before.
            int i = 0;

            while ((i < _finalVertexes.Count) && (_finalVertexes[i].Item1 < vertexIndex))
            {
                i++;
            }

            if (i == _finalVertexes.Count)
            {
                // Got end of list.
                _finalVertexes.Add(new Tuple<int, int>(vertexIndex, typeIndex));
            }
            else if (_finalVertexes[i].Item1 == vertexIndex)
            {
                // Already exists.
                return false;
            }
            else
            {
                _finalVertexes.Insert(i, new Tuple<int, int>(vertexIndex, typeIndex));
            }
            return true;
        }
        public bool AddFinal(int vertexIndex, string typeName)
        {
            if (!AddFinishType(typeName))
            {
                return false;
            }

            int typeIndex = GetIndexOfFinishType(typeName);
            AddFinal(vertexIndex, typeIndex);
            return true;
        }

        public bool AddFinishType(string newFinishType)
        {
            if (GetIndexOfFinishType(newFinishType) == _defaultNoIndex)
            {
                _finalTypes.Add(newFinishType);
                return true;
            }
            return false;
        }

        private bool AddFinalRange(List<Tuple<int, int>> inputTuples)
        {
            bool result = false;

            foreach (Tuple<int, int> tuple in inputTuples)
            {
                result = result | AddFinal(tuple.Item1, tuple.Item2);
            }
            return result;
        }
        private bool AddFinalRange(List<int> inputIndexes)
        {
            bool result = false;

            foreach (int index in inputIndexes)
            {
                result = result | AddFinal(index);
            }
            return result;
        }

        private void AddEmptyVertex()
        {
            List<List<int>> vertexTransitions = new List<List<int>>();
            for (int i = 0; i < _commands.Count; i++)
            {
                vertexTransitions.Add(new List<int>(_defaultEmptyTransition));
            }
            _transitions.Add(new List<List<int>>(vertexTransitions));
        }

        private bool AddVertex(int inputIndex)
        {
            if (VertexExists(inputIndex))
            {
                return false;
            }
            else
            {
                int i = _transitions.Count;

                while (i < inputIndex)
                {
                    AddEmptyVertex();
                    i++;
                }
                // Adding the goal Vertex.
                AddEmptyVertex();
                return true;
            }
        }

        private bool AddRangeVertexes(List<int> inputVertexes)
        {
            bool result = false;

            foreach (int vertex in inputVertexes)
            {
                if (!VertexExists(vertex))
                {
                    AddVertex(vertex);
                    result = true;
                }
            }
            return result;
        }

        private bool AddCommand(char inputCommand)
        {
            if (CommandExists(inputCommand))
            {
                return false;
            }
            else
            {
                // We must create this command.
                _commands.Add(inputCommand);
                foreach (List<List<int>> vertexTransition in _transitions)
                {
                    vertexTransition.Add(new List<int>(_defaultEmptyTransition));
                }
                return true;
            }
        }
        private bool AddCommands(List<char> commands)
        {
            bool result = false;

            foreach (char command in commands)
            {
                result |= AddCommand(command);
            }
            return result;
        }
    #endregion

    #region Delete Methods
        private bool DeleteFinalVertexes()
        {
            if (_finalVertexes.Count > 0)
            {
                _finalVertexes = new List<Tuple<int, int>>();
                return true;
            }
            return false;
        }
    #endregion

        #region operators
        public static StateMachine operator |(StateMachine machine1, StateMachine machine2)
        {
            // Types of final vertexes must be equal.
            // Start vetex must be == 0.
            StateMachine result = new StateMachine(machine1);
            int oldVertexCount = machine1.Vertexes.Count;

            // Copy transitions of machine2 into result.          
            result = Copy(result, machine2);

            if (machine2.IsFinalVertex(machine2.StartingVertex))
            {
                // If startingVertex of machine2 is final.
                result.AddFinal(result.StartingVertex, machine2.GetIndexOfTypeOfFinalVertex(machine2.StartingVertex));
            }

            // Transitions of the StartingVertex of the machine2.
            for (int commandIndex = 0; commandIndex < machine2._commands.Count; commandIndex++)
            {
                char commandChar = machine2.GetCommandByIndex(commandIndex);

                foreach (int destinationVertex in machine2._transitions[0][commandIndex])
                {
                    if (destinationVertex != _defaultNoIndex)
                    {
                        int newVertexIndex = destinationVertex - 1 + oldVertexCount;
                        result.AddTransition(result.StartingVertex, commandChar, newVertexIndex);
                    }
                }
            }

            return result;
        } // operator |

        public static StateMachine operator &(StateMachine machine1, StateMachine machine2)
        {
            StateMachine result = new StateMachine(machine1);
            int oldVertexCount = machine1.Vertexes.Count;
            List<int> oldFinalVertexes = new List<int>();

            foreach (Tuple<int, int> finalVertex in result._finalVertexes)
            {
                // Remember old finishing verteses to build transitions from them.
                oldFinalVertexes.Add(finalVertex.Item1);
            }
            if (!machine2.IsFinalVertex(machine2.StartingVertex))
            {
                // All the old final vertexes of result must stop to be final!   
                result.DeleteFinalVertexes();
            }

            result = Copy(result, machine2);

            foreach (int oldFinal in oldFinalVertexes)
            {
                for (int commandIndex = 0; commandIndex < machine2._commands.Count; commandIndex++)
                {
                    char commandChar = machine2.GetCommandByIndex(commandIndex);

                    foreach (int destinationVertex in machine2._transitions[0][commandIndex])
                    {
                        if (destinationVertex != _defaultNoIndex)
                        {
                            int newVertexIndex = destinationVertex - 1 + oldVertexCount;
                            result.AddTransition(oldFinal, commandChar, newVertexIndex);
                        }
                    }
                }
            }
            
            return result;
        } // operator &

        public static StateMachine operator |(StateMachine machine, char command)
        {
            StateMachine machine2 = new StateMachine(command);
            return machine | machine2;
        }

        public static StateMachine operator &(StateMachine machine, char command)
        {
            StateMachine machine2 = new StateMachine(command);
            return machine & machine2;
        }

        public static StateMachine Cycle(StateMachine machine)
        {
            StateMachine result = new StateMachine(machine);

            result = +result;
            result.AddFinal(result.StartingVertex, result._finalVertexes[0].Item2);
            return result;
        }

        public static StateMachine operator +(StateMachine machine)
        {
            StateMachine result = new StateMachine(machine);

            foreach (int finalVertex in result.FinalVertexes)
            {
                for (int commandIndex = 0; commandIndex < machine._commands.Count; commandIndex++)
                {
                    foreach (int destinationVertex in machine._transitions[0][commandIndex])
                    {
                        if (destinationVertex != _defaultNoIndex)
                        {
                            result.AddTransition(finalVertex, commandIndex, destinationVertex);
                        }
                    }
                }
            }
            //result.AddFinal(result.StartingVertex, result._finalVertexes[0].Item2);

            return result;
        }

        private static StateMachine Copy(StateMachine machine1, StateMachine machine2)
        {            
            StateMachine result = new StateMachine(machine1);
            int oldVertexCount = machine1.Vertexes.Count;

            result.AddCommands(machine2.TerminalAlphabet);
            
            // Copy transitions of machine2 into result.
            for (int i = 0; i < machine2.Vertexes.Count - 1; i++)
            {
                for (int commandIndex = 0; commandIndex < machine2._commands.Count; commandIndex++)
                {
                    char commandChar = machine2.GetCommandByIndex(commandIndex);

                    foreach (int destinationVertex in machine2._transitions[i + 1][commandIndex])
                    {
                        //if (destinationVertex != _defaultNoIndex)
                        //{
                        //    int newDestination = destinationVertex - 1 + oldVertexCount;
                        //    result.AddTransition(oldVertexCount + i, commandChar, newDestination);
                        //}
                        int newDestination = -1;
                        if (destinationVertex != _defaultNoIndex)
                        {
                            newDestination = destinationVertex - 1 + oldVertexCount;
                        }
                        result.AddTransition(oldVertexCount + i, commandChar, newDestination);
                        /////// End of test
                    }
                }
                if (machine2.IsFinalVertex(i + 1))
                {
                    result.AddFinal(oldVertexCount + i, machine2.GetIndexOfTypeOfFinalVertex(i + 1));
                }
            }
            return result;
        }
    #endregion

        public static List<Tuple<int, char, int>> StringsToTripleTuples(string input)
        {
            List<Tuple<int, char, int>> result = new List<Tuple<int, char, int>>();
            List<string> words = new List<string>();
            List<char> currWord = new List<char>();
            foreach (char c in input)
            {
                if (c == ' ')
                {
                    if (currWord.Count > 0)
                    {
                        string currWordString = "";
                        foreach (char letter in currWord)
                        {
                            currWordString += letter;
                        }
                        words.Add(currWordString);
                        currWord = new List<char>();
                    }
                }
                else
                {
                    currWord.Add(c);
                }
            }
            if (currWord.Count > 0)
            {
                string currWordString = "";
                foreach (char letter in currWord)
                {
                    currWordString += letter;
                }
                words.Add(currWordString);
                currWord = new List<char>();
            }

            for(int i = 0; i < words.Count / 3; i++)
            {
                result.Add(new Tuple<int, char, int>(Convert.ToInt32(words[i*3]), words[i*3 +1][0], Convert.ToInt32(words[i*3 +2])));
            }
            return result;
        }
    }
}