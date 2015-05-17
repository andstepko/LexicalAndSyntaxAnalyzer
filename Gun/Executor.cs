using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class Executor
    {
        private int _defaultNoIndex;
        private StateMachine _stateMachine;
        private string _text;
        private int _currentTextPosition;
        private int _currentVertex;
        private string _currentLexeme;
        private List<Tuple<string, string>> _result;

        public Executor(StateMachine inputMachine, string inputText)
        {
            _stateMachine = inputMachine;

            if (!inputMachine.IsDetermined)
            {
                Determiner.Determined(_stateMachine);
            }

            _text = inputText;
            _defaultNoIndex = _stateMachine.DefaultNoIndex;
            _currentTextPosition = 0;
            _result = new List<Tuple<string, string>>();
        }

        public List<Tuple<string, string>> Execute()
        {
            SetMachineToStart();

            while (_currentTextPosition < _text.Length)
            {
                ExecuteOneLexeme();
            }

            return _result;
        }

        private void ExecuteOneLexeme()
        {
            while (_currentTextPosition < _text.Length)
            {
                int destinationVertex = NextState(_text[_currentTextPosition]);

                if (destinationVertex == _defaultNoIndex)
                {
                    //FinishOneLexeme();
                    //_currentTextPosition++;
                    break;
                }
                else
                {
                    // Destination Vertex is valid.
                    _currentVertex = destinationVertex;
                    _currentLexeme += _text[_currentTextPosition];
                    _currentTextPosition++;
                }
            }
            FinishOneLexeme();
        }

        private void FinishOneLexeme()
        {
            if (_stateMachine.IsFinalVertex(_currentVertex))
            {
                int finishTypeIndex = _stateMachine.GetIndexOfTypeOfFinalVertex(_currentVertex);
                string finishType = _stateMachine.GetFinalTypeByIndex(finishTypeIndex);

                _result.Add(new Tuple<string, string>(_currentLexeme, finishType));
            }
            else
            {
                // We are stopping at a NON-final Vertex.
                _currentTextPosition++;
            }
            SetMachineToStart();
        }

        private int NextState(char inputCommand)
        {
            int indexOfCommand = _stateMachine.GetIndexByCommand(inputCommand);

            if (indexOfCommand == _defaultNoIndex)
            {
                return _defaultNoIndex;
            }

            return _stateMachine.GetDestinationVertex(_currentVertex, inputCommand);
        }

        private void SetMachineToStart()
        {
            MoveTo(_stateMachine.StartingVertex);
            _currentLexeme = "";
        }

        private void MoveTo(int inputVertex)
        {
            _currentVertex = inputVertex;
        }

        public void PrintLexemes()
        {
            foreach (Tuple<string, string> tuple in _result)
            {
                PrintTuple(tuple);
            }
        }

        private void PrintTuple(Tuple<string, string> inputTuple)
        {
            Console.WriteLine(inputTuple.Item1 + "   ---   " + inputTuple.Item2);
        }
    }
}