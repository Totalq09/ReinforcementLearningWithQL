using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Models
{
    public class AreaModel
    {
        public int WorldSize { get; set;
        }
        public CellModel[,] Cells { get; set; }

        public Tuple<int,int> PlayerInitialPosition { get; set; }

        public AreaModel(int worldSize)
        {
            WorldSize = worldSize;
            Cells = new CellModel[worldSize,worldSize];
        }

        public void InitializeRandomly(int exitQuantity)
        {
            for(int i = 0; i < WorldSize; i++)
            {
                for(int j = 0; j < WorldSize; j++)
                {
                    Cells[i,j] = new CellModel(CellContentEnum.Free);
                }
            }

            this.InitializeExits(exitQuantity);
            this.InitializeWalls();
            this.InitializeAgentPosition();
        }

        private void InitializeExits(int exitQuantity)
        {
            for (int i = 0; i < exitQuantity;)
            {
                Random randomizer = new Random();
                var x = (int)(randomizer.NextDouble() * WorldSize);
                var y = 0;
                if (x == WorldSize)
                    x--;

                if (x != 0 && x != WorldSize - 1)
                {
                    var chooseFromTwo = Math.Round(randomizer.NextDouble());

                    if (chooseFromTwo == 1)
                        y = WorldSize - 1;
                }
                else
                {
                    y = (int)(randomizer.NextDouble() * WorldSize);
                }

                if (Cells[x, y].CellContent == CellContentEnum.Free)
                {
                    Cells[x, y].CellContent = CellContentEnum.Exit;
                    i++;
                }
            }
        }

        private void InitializeWalls()
        {
            var randomizer = new Random();
            var occupancePercent = 0.0;
            var cellsMarkedAsWalls = 0;
            var loopWithoutSuccess = 0;

            var offset = randomizer.NextDouble();
            offset = Math.Min(0.3, offset);
            offset = Math.Max(0.1, offset);

            while(occupancePercent < 0.2 + offset && loopWithoutSuccess < 10)
            {
                var x = (int)(randomizer.NextDouble() * WorldSize);
                var y = (int)(randomizer.NextDouble() * WorldSize);

                var lengthMax = randomizer.Next(WorldSize * 2);
                if (lengthMax < 4)
                    lengthMax = 4;

                var currentLength = 0;
                var currentEnd1 = Tuple.Create(x, y);
                var currentEnd2 = Tuple.Create(x, y);
                var possibleMoves = this.InitializePossibleMovements();
                var currentlyBuildingPath = new List<Tuple<int, int>>();

                if(IsPossibleToStartWall(x,y,out var linkedToEdge))
                {
                    Cells[currentEnd1.Item1, currentEnd1.Item2].CellContent = CellContentEnum.Wall;

                    currentlyBuildingPath.Add(Tuple.Create(currentEnd1.Item1, currentEnd1.Item2));
                    cellsMarkedAsWalls++;
                    currentLength++;

                    loopWithoutSuccess = 0;
                    while (possibleMoves.Count != 0 && currentLength < lengthMax)
                    {
                        var movement = possibleMoves[(int)(randomizer.Next(possibleMoves.Count))];
                        possibleMoves.Remove(movement);
                        bool linkedToEdgeFromNow = false;
                        if (this.PossibleToMove(currentlyBuildingPath, currentEnd1.Item1, currentEnd1.Item2, currentEnd1.Item1+movement.Item1, currentEnd1.Item2+movement.Item2, linkedToEdge, out linkedToEdgeFromNow))
                        {
                            if(!linkedToEdge)
                            {
                                linkedToEdge = linkedToEdgeFromNow;
                            }

                            currentEnd1 = Tuple.Create(currentEnd1.Item1 + movement.Item1, currentEnd1.Item2 + movement.Item2);
                            Cells[currentEnd1.Item1, currentEnd1.Item2].CellContent = CellContentEnum.Wall;

                            currentlyBuildingPath.Add(Tuple.Create(currentEnd1.Item1, currentEnd1.Item2));
                            cellsMarkedAsWalls++;
                            currentLength++;
                            possibleMoves = this.InitializePossibleMovements();
                        }
                        else if (this.PossibleToMove(currentlyBuildingPath, currentEnd2.Item1, currentEnd2.Item2, currentEnd2.Item1 + movement.Item1, currentEnd2.Item2 + movement.Item2, linkedToEdge, out linkedToEdgeFromNow))
                        {
                            if (!linkedToEdge)
                            {
                                linkedToEdge = linkedToEdgeFromNow;
                            }

                            currentEnd2 = Tuple.Create(currentEnd2.Item1 + movement.Item1, currentEnd2.Item2 + movement.Item2);
                            Cells[currentEnd2.Item1, currentEnd2.Item2].CellContent = CellContentEnum.Wall;

                            currentlyBuildingPath.Add(Tuple.Create(currentEnd2.Item1, currentEnd2.Item2));
                            cellsMarkedAsWalls++;
                            currentLength++;
                            possibleMoves = this.InitializePossibleMovements();
                        }
                    }
                    
                    occupancePercent = (double)(cellsMarkedAsWalls) / (double)(WorldSize*WorldSize);
                }
                else
                {
                    loopWithoutSuccess++;
                }
            }
        }

        private bool IsPossibleToStartWall(int x, int y, out bool startFromEdge)
        {
            startFromEdge = false;

            if (x >= WorldSize || 
                y >= WorldSize ||
                x < 0 ||
                y < 0 ||
                Cells[x, y].CellContent == CellContentEnum.Exit || 
                Cells[x, y].CellContent == CellContentEnum.Wall)
            {
                return false;
            }

            for(int i = -1; i < 2; i++)
            {
                for(int j = -1; j < 2; j++)
                {
                    try
                    {
                        if(Cells[x+i, y+j].CellContent == CellContentEnum.Wall)
                        {
                            return false;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        startFromEdge = true;
                    }
                }
            }

            return true;
        }

        private bool PossibleToMove(List<Tuple<int,int>> currentlyBuildingPath, int fromX, int fromY, int x, int y, bool linkedToEdge, out bool linkedToEdgeFromNow)
        {
            linkedToEdgeFromNow = false;
            var fragmentTouched = 0;

            if (x >= WorldSize ||
                y >= WorldSize ||
                x < 0 ||
                y < 0)
            {
                return false;
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (currentlyBuildingPath.Any(item => item.Item1 == x + i && item.Item2 == y + j))
                    {
                        fragmentTouched++;
                        if (fragmentTouched > 2)
                            return false;
                        continue;
                    }

                    try
                    {
                        
                        if (Cells[x + i, y + j].CellContent != CellContentEnum.Free)
                        {
                            return false;
                        }

                        try
                        {
                            var offset = Tuple.Create((x - fromX) * 2, (y - fromY) * 2);

                            if (Cells[fromX + offset.Item1, fromY + offset.Item2].CellContent != CellContentEnum.Free)
                            {
                                return false;
                            }

                            if (offset.Item1 != 0)
                            {
                                if (Cells[fromX + offset.Item1, fromY + offset.Item2 + 1].CellContent != CellContentEnum.Free)
                                {
                                    return false;
                                }
                                if (Cells[fromX + offset.Item1, fromY + offset.Item2 -1].CellContent != CellContentEnum.Free)
                                {
                                    return false;
                                }
                            }
                            else if (offset.Item2 != 0)
                            {
                                if (Cells[fromX + offset.Item1 - 1, fromY + offset.Item2].CellContent != CellContentEnum.Free)
                                {
                                    return false;
                                }
                                if (Cells[fromX + offset.Item1 + 1, fromY + offset.Item2].CellContent != CellContentEnum.Free)
                                {
                                    return false;
                                }
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (!linkedToEdge)
                            {
                                linkedToEdgeFromNow = true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                        //    for (int a = -1; a < 2; a++)
                        //    {
                        //        for (int b = -1; b < 2; b++)
                        //        {
                        //            try
                        //            {
                        //                if (Cells[x + i+a, y + j+b].CellContent != CellContentEnum.Free)
                        //                {

                        //                }
                        //            }
                        //            catch (IndexOutOfRangeException)
                        //            {
                        //                if (!linkedToEdge)
                        //                {
                        //                    linkedToEdgeFromNow = true;
                        //                }
                        //                else
                        //                {
                        //                    return false;
                        //                }
                        //            }
                        //        }
                        //    }
                    }
                        catch (IndexOutOfRangeException)
                    {

                    }
                }
            }

            return true;
        }

        private List<Tuple<int,int>> InitializePossibleMovements()
        {
            return new List<Tuple<int, int>>
            {
                Tuple.Create(-1,0),
                Tuple.Create(1,0),
                Tuple.Create(0,-1),
                Tuple.Create(0,1),
            };
        }

        private void InitializeAgentPosition()
        {
            var randomizer = new Random();
            var x = 0;
            var y = 0;

            while(true)
            {
                x = randomizer.Next(WorldSize);
                y = randomizer.Next(WorldSize);

                if(Cells[x, y].CellContent == CellContentEnum.Free)
                {
                    Cells[x, y].CellContent = CellContentEnum.Agent;
                    break;
                }
            }
        }
    }
}
