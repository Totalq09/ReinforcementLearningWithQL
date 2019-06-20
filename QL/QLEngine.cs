using Prism.Commands;
using QL.Helpers;
using QL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace QL
{
    public class QLEngine
    {
        public AreaModel AreaModel { get; set; }

        public double[,,] RewardTable { get; set; }

        public double[,,] QTable { get; set; }

        public double Gamma { get; set; }

        public double Alpha { get; set; }

        public int IterationMaxLength { get; set; }

        public int IterationMax { get; set; }

        public event EventHandler ProgressUpdate;
        public event EventHandler IterationUpdate;

        public double IterationsPerSecond { get; set; }

        public bool GoFurtherWithIteration { get; set; }

        public bool ManualIteration { get; set; }

        public bool Reset { get; set; }

        public bool UnlimitedIterationPerSeconds { get; set; }

        public QLEngine(AreaModel areaModel, int iterationMax, int iterationMaxLength, double iterationsPerSecond, bool manualIteration, bool unlimitedIterationsPerSecond)
        {
            AreaModel = areaModel;
            RewardTable = new double[AreaModel.WorldSize, AreaModel.WorldSize,4];
            QTable = new double[AreaModel.WorldSize, AreaModel.WorldSize, 4];
            Gamma = 0.9;
            Alpha = 0.2;
            IterationMax = iterationMax;
            IterationMaxLength = iterationMaxLength * AreaModel.WorldSize;
            this.IterationsPerSecond = iterationsPerSecond;
            this.GoFurtherWithIteration = false;
            this.ManualIteration = manualIteration;
            this.Reset = false;
            this.UnlimitedIterationPerSeconds = unlimitedIterationsPerSecond;
        }

        public void Learn()
        {
            this.CallReset();

            int i = 0;
            int j = 0;
            var initialAgentPosition = Tuple.Create(AreaModel.AgentPosition.Item1, AreaModel.AgentPosition.Item2);
            var currentAgentPosition = Tuple.Create(AreaModel.AgentPosition.Item1, AreaModel.AgentPosition.Item2);

            var watch = new Stopwatch();

            while (i < IterationMax)
            {
                AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Free;
                AreaModel.Cells[initialAgentPosition.Item1, initialAgentPosition.Item2].CellContent = CellContentEnum.Agent;
                currentAgentPosition = Tuple.Create(initialAgentPosition.Item1, initialAgentPosition.Item2);
                j = 0;
                watch.Restart();
                while (j < IterationMaxLength)
                {
                    if (Reset)
                    {
                        this.CallReset(currentAgentPosition, initialAgentPosition);
                        return;
                    }

                    var currentAction = GetNextActionEpsilon(currentAgentPosition, (double)((double)i / (double)IterationMax));
                    var newAgentPosition = this.UpdateQTable(currentAgentPosition, currentAction);

                    if (newAgentPosition != null &&
                        AreaModel.Cells[newAgentPosition.Item1, newAgentPosition.Item2].CellContent == CellContentEnum.Exit)
                    {
                        break;
                    }
                    
                    if(newAgentPosition != null)
                    {
                        AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Free;
                        AreaModel.Cells[newAgentPosition.Item1, newAgentPosition.Item2].CellContent = CellContentEnum.Agent;
                        currentAgentPosition = Tuple.Create(newAgentPosition.Item1, newAgentPosition.Item2);
                    }

                    j++; 
                }

                this.CallEvents(i, !UnlimitedIterationPerSeconds);

                if (ManualIteration)
                {
                    while (!GoFurtherWithIteration && ManualIteration && !Reset)
                        Thread.Sleep(50);

                    GoFurtherWithIteration = false;
                }

                watch.Stop();

                if (!UnlimitedIterationPerSeconds)
                {
                    var sleep = (1000.0 - watch.ElapsedMilliseconds * IterationsPerSecond) / IterationsPerSecond;
                    if (sleep > 0)
                        Thread.Sleep((int)((1000.0 - watch.ElapsedMilliseconds * IterationsPerSecond) / IterationsPerSecond));
                }
                i++;
            }

            AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Free;
            AreaModel.Cells[initialAgentPosition.Item1, initialAgentPosition.Item2].CellContent = CellContentEnum.Agent;
        }

        public void Play(CancellationToken token)
        {
            var initialAgentPosition = Tuple.Create(AreaModel.AgentPosition.Item1, AreaModel.AgentPosition.Item2);
            var currentAgentPosition = Tuple.Create(AreaModel.AgentPosition.Item1, AreaModel.AgentPosition.Item2);

            while (!token.IsCancellationRequested)
            {
                var currentState = CellContentEnum.Free;
                initialAgentPosition = Tuple.Create(AreaModel.AgentPosition.Item1, AreaModel.AgentPosition.Item2);
                currentAgentPosition = Tuple.Create(AreaModel.AgentPosition.Item1, AreaModel.AgentPosition.Item2);

                while (currentState != CellContentEnum.Exit && !token.IsCancellationRequested)
                {
                    var action = this.GetNextBestAction(currentAgentPosition);
                    var newAgentPosition = this.MoveToNewState(currentAgentPosition, action, out var movedToInvalidState);

                    if (movedToInvalidState)
                    {
                        Thread.Sleep(500);
                        break;
                    }

                    currentState = AreaModel.Cells[newAgentPosition.Item1, newAgentPosition.Item2].CellContent;
                    AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Free;
                    AreaModel.Cells[newAgentPosition.Item1, newAgentPosition.Item2].CellContent = CellContentEnum.Agent;
                    currentAgentPosition = Tuple.Create(newAgentPosition.Item1, newAgentPosition.Item2);

                    this.CallEvents();
                    Thread.Sleep(500);
                }

                AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Exit;
                AreaModel.Cells[initialAgentPosition.Item1, initialAgentPosition.Item2].CellContent = CellContentEnum.Agent;
            }

            if(AreaModel.Exits.Any(item => item.Item1 == currentAgentPosition.Item1 && item.Item2 == currentAgentPosition.Item2))
                AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Exit;
            else
                AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Free;
               
            AreaModel.Cells[initialAgentPosition.Item1, initialAgentPosition.Item2].CellContent = CellContentEnum.Agent;
        }

        private Tuple<int,int> UpdateQTable(Tuple<int,int> position, int action)
        {
            var newAgentPosition = this.MoveToNewState(position, action, out var movedToInvalidState);

            QTable[position.Item1, position.Item2, action] =
                QTable[position.Item1, position.Item2, action] +
                Alpha * (RewardTable[position.Item1, position.Item2, action]
                + Gamma * (movedToInvalidState ? -1 : GetEstimatedFutureValue(position, action)) - QTable[position.Item1, position.Item2, action]);

            AreaModel.Cells[position.Item1, position.Item2].CurrentReward0 = QTable[position.Item1, position.Item2, 0];
            AreaModel.Cells[position.Item1, position.Item2].CurrentReward1 = QTable[position.Item1, position.Item2, 1];
            AreaModel.Cells[position.Item1, position.Item2].CurrentReward2 = QTable[position.Item1, position.Item2, 2];
            AreaModel.Cells[position.Item1, position.Item2].CurrentReward3 = QTable[position.Item1, position.Item2, 3];

            if (movedToInvalidState)
                return null;

            return newAgentPosition;
        } 

        private Tuple<int,int> MoveToNewState(Tuple<int, int> position, int action, out bool movedToInvalidState)
        {
            Tuple<int, int> newAgentPosition = null;
            movedToInvalidState = false;
            try
            {
                switch (action)
                {
                    case 0:
                        if (AreaModel.Cells[position.Item1 - 1, position.Item2].CellContent == CellContentEnum.Wall)
                            movedToInvalidState = true;
                        else
                            newAgentPosition = Tuple.Create(position.Item1 - 1, position.Item2);
                        break;
                    case 1:
                        if (AreaModel.Cells[position.Item1, position.Item2 + 1].CellContent == CellContentEnum.Wall)
                            movedToInvalidState = true;
                        else
                            newAgentPosition = Tuple.Create(position.Item1, position.Item2 + 1);
                        break;
                    case 2:
                        if (AreaModel.Cells[position.Item1 + 1, position.Item2].CellContent == CellContentEnum.Wall)
                            movedToInvalidState = true;
                        else
                            newAgentPosition = Tuple.Create(position.Item1 + 1, position.Item2);
                        break;
                    case 3:
                        if (AreaModel.Cells[position.Item1, position.Item2 - 1].CellContent == CellContentEnum.Wall)
                            movedToInvalidState = true;
                        else
                            newAgentPosition = Tuple.Create(position.Item1, position.Item2 - 1);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                movedToInvalidState = true;
            }

            return newAgentPosition;
        }

        private double GetEstimatedFutureValue(Tuple<int, int> position, int action)
        {
            var rewards = new double[4];

            try
            {
                switch (action)
                {
                    case 0:
                        rewards[0] = QTable[position.Item1 - 1, position.Item2, 0];
                        rewards[1] = QTable[position.Item1 - 1, position.Item2, 1];
                        rewards[2] = QTable[position.Item1 - 1, position.Item2, 2];
                        rewards[3] = QTable[position.Item1 - 1, position.Item2, 3];
                        break;
                    case 1:
                        rewards[0] = QTable[position.Item1, position.Item2 + 1, 0];
                        rewards[1] = QTable[position.Item1, position.Item2 + 1, 1];
                        rewards[2] = QTable[position.Item1, position.Item2 + 1, 2];
                        rewards[3] = QTable[position.Item1, position.Item2 + 1, 3];
                        break;
                    case 2:
                        rewards[0] = QTable[position.Item1 + 1, position.Item2, 0];
                        rewards[1] = QTable[position.Item1 + 1, position.Item2, 1];
                        rewards[2] = QTable[position.Item1 + 1, position.Item2, 2];
                        rewards[3] = QTable[position.Item1 + 1, position.Item2, 3];
                        break;
                    case 3:
                        rewards[0] = QTable[position.Item1, position.Item2 - 1, 0];
                        rewards[1] = QTable[position.Item1, position.Item2 - 1, 1];
                        rewards[2] = QTable[position.Item1, position.Item2 - 1, 2];
                        rewards[3] = QTable[position.Item1, position.Item2 - 1, 3];
                        break;
                }

                return rewards.Max();
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
        }

        private int GetNextActionRandomly()
        {
            return Randomizer.Next(4);
        }

        private int GetNextActionEpsilon(Tuple<int,int> position, double iterationPassedPercent)
        {
            var epsilon = 1.0 * (1.0 - iterationPassedPercent);

            if(Randomizer.NextDouble() < epsilon)
            {
                return GetNextActionRandomly();
            }

            var rewards = new double[4];

            rewards[0] = QTable[position.Item1, position.Item2, 0];
            rewards[1] = QTable[position.Item1, position.Item2, 1];
            rewards[2] = QTable[position.Item1, position.Item2, 2];
            rewards[3] = QTable[position.Item1, position.Item2, 3];

            return rewards.ToList().IndexOf(rewards.Max());
        }

        private int GetNextBestAction(Tuple<int, int> position)
        {
            var rewards = new double[4];
            rewards[0] = QTable[position.Item1, position.Item2, 0];
            rewards[1] = QTable[position.Item1, position.Item2, 1];
            rewards[2] = QTable[position.Item1, position.Item2, 2];
            rewards[3] = QTable[position.Item1, position.Item2, 3];

            return rewards.ToList().IndexOf(rewards.Max());
        }

        public void Initialize()
        {
            this.InitializeRewardTable();
        }

        private void InitializeRewardTable()
        {
            var worldSize = AreaModel.WorldSize;
            var calculationPartSize = worldSize / 4;
            var watch = new Stopwatch();
            watch.Start();
            Parallel.Invoke(
                () =>
                {
                    InitializePartOfRewardTable(calculationPartSize, 0);
                },
                () =>
                {
                    InitializePartOfRewardTable(calculationPartSize, 1);
                },
                () =>
                {
                    InitializePartOfRewardTable(calculationPartSize, 2);
                },
                () =>
                {
                    InitializePartOfRewardTable(calculationPartSize, 3);
                });
            watch.Stop();
            Trace.WriteLine($"InitializeRewardTable took {watch.ElapsedMilliseconds}ms");
        }

        private void InitializePartOfRewardTable(int partSize, int partNumber)
        {
            var offset = partSize;
            if (partNumber == 3)
            {
                offset = AreaModel.WorldSize - partSize * partNumber;
            }

            for(int x = partNumber*partSize; x < partSize*partNumber+ offset; x++)
            {
                for(int y = 0; y < AreaModel.WorldSize; y++)
                {
                    RewardTable[x, y, 0] = GetInitialReward(x - 1 < 0 ? (CellContentEnum?)null : AreaModel.Cells[x - 1, y].CellContent);
                    RewardTable[x, y, 1] = GetInitialReward(y + 1 == AreaModel.WorldSize ? (CellContentEnum?)null : AreaModel.Cells[x, y + 1].CellContent);
                    RewardTable[x, y, 2] = GetInitialReward(x + 1 == AreaModel.WorldSize ?(CellContentEnum?)null : AreaModel.Cells[x + 1, y].CellContent);
                    RewardTable[x, y, 3] = GetInitialReward(y - 1 < 0 ? (CellContentEnum?)null : AreaModel.Cells[x, y - 1].CellContent);
                }
            }
        }

        private double GetInitialReward(CellContentEnum? cellContent)
        {
            if (cellContent == null)
                return -4.0;

            else if (cellContent == CellContentEnum.Exit)
                return 500.0;

            else if (cellContent == CellContentEnum.Wall)
                return -2.0;

            else
                return -0.15;
        }

        private void CallReset(Tuple<int,int> currentAgentPosition, Tuple<int, int> initialAgentPosition)
        {
            AreaModel.Cells[currentAgentPosition.Item1, currentAgentPosition.Item2].CellContent = CellContentEnum.Free;
            AreaModel.Cells[initialAgentPosition.Item1, initialAgentPosition.Item2].CellContent = CellContentEnum.Agent;

            this.CallReset();
        }

        private void CallReset()
        {
            for (int o = 0; o < AreaModel.WorldSize; o++)
            {
                for (int p = 0; p < AreaModel.WorldSize; p++)
                {
                    AreaModel.Cells[o, p].CurrentReward0 = 0.0;
                    AreaModel.Cells[o, p].CurrentReward1 = 0.0;
                    AreaModel.Cells[o, p].CurrentReward2 = 0.0;
                    AreaModel.Cells[o, p].CurrentReward3 = 0.0;
                }
            }
        }

        private void CallEvents(int iteration = -1, bool sendUpdateEvent = true)
        {
            if (ProgressUpdate != null && sendUpdateEvent)
                ProgressUpdate(this, null);
        
            if (IterationUpdate != null && iteration != -1)
                IterationUpdate(this, new IterationUpdateEventArgs() { Iteration = iteration });
        }
    }
}
