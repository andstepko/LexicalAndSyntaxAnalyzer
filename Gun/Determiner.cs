using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gun
{
    class Determiner
    {
        public static StateMachine Determined(StateMachine machine)
        {
            if(machine.IsDetermined)
            {
                return machine;
            }

            StateMachine result = new StateMachine(machine.TerminalAlphabet);
            result.FinalTypesDirty = machine.FinalTypesDirty;
            List<List<int>> oldVertexSets = new List<List<int>>();
            int currentNewVertex = 0;

            oldVertexSets.Add(new List<int>() { 0 });

            while (result.VertexExists(currentNewVertex))
            {
                for(int command = 0; command < machine.AlphabetLength; command++)
                {
                    // command --- index of a folowing command
                    List<int> destinationsByCommand = GetMultipleDestinationVertexes(machine, oldVertexSets[currentNewVertex], command);
                    destinationsByCommand.Sort();

                    if(!Match(destinationsByCommand, new List<int>() { -1 }))
                    {
                        int indexAmoungOldVertexes = IndexOf(oldVertexSets, destinationsByCommand);

                        if (indexAmoungOldVertexes == -1)
                        {
                            oldVertexSets.Add(destinationsByCommand);
                            indexAmoungOldVertexes = oldVertexSets.Count - 1;

                            int finalType = WhichFinalType(machine, oldVertexSets[indexAmoungOldVertexes]);
                            if (WhichFinalType(machine, oldVertexSets[indexAmoungOldVertexes]) != -1)
                            {
                                //FIXME
                                result.AddFinal(indexAmoungOldVertexes, finalType);
                            }
                        }

                        result.AddTransition(currentNewVertex, command, indexAmoungOldVertexes);
                    }
                    else{
                        // No transition from currentNewVertex to anywhere by command.
                    }
                } // Command ended.
                currentNewVertex++;
            } // while

            return result;
        }

        private static bool Match(List<int> list1, List<int> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            for(int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static List<int> Intersect(List<int> list1, List<int> list2)
        {
            List<int> result = new List<int>();

            foreach(int i in list1)
            {
                if(list2.Contains(i))
                {
                    result.Add(i);
                }
            }
            return result;
        }

        private static bool AddNew(ref List<int> list, int newInt)
        {
            if (list.Contains(newInt))
            {
                return false;
            }
            if (Match(list, new List<int>() { -1 }))
            {
                list = new List<int>();
                list.Add(newInt);
            }
            else if (newInt != -1)
            {
                list.Add(newInt);
            }
            return true;
        }

        private static bool AddNewRange(ref List<int> list, List<int> addingList)
        {
            bool result = false;

            foreach (int newItem in addingList)
            {
                result |= AddNew(ref list, newItem);
            }
            return result;
        }

        private static int IndexOf(List<List<int>> dataList, List<int> soughtForList)
        {
            for(int i = 0; i < dataList.Count; i++)
            {
                if(Match(dataList[i], soughtForList))
                {
                    return i;
                }
            }
            return -1;
        }

        private static int WhichFinalType(StateMachine machine, List<int> vertexes)
        {
            foreach (int vertex in vertexes)
            {
                if(machine.IsFinalVertex(vertex))
                {
                    return machine.GetIndexOfTypeOfFinalVertex(vertex);
                }
            }
            // No final was found in vertexes.
            return -1;
        }

        private static List<int> GetMultipleDestinationVertexes(StateMachine machine, List<int> sourceVertexes, int command)
        {
            List<int> destinations = new List<int>() { -1 };

            if ((sourceVertexes.Count < 1) || (!machine.CommandExists(command)) || (!machine.VertexesExist(sourceVertexes)))
            {
                return destinations;
            }

            foreach (int sourceVertex in sourceVertexes)
            {
                AddNewRange(ref destinations, machine.GetDestinationVetexes(sourceVertex, command));
            }
            if (destinations.Count > 0)
            {
                return destinations;
            }
            return new List<int>() { -1 };
        }
    }
}