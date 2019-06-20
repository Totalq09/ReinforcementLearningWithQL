using Prism.Commands;
using Prism.Mvvm;
using QL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QL
{
    public class QLViewModel : BindableBase
    {
        #region Properties

        private int worldSize { get; set; }
        public string WorldSize
        {
            get { return worldSize.ToString(); }
            set
            {
                worldSize = int.Parse(value);
                RaisePropertyChanged("WorldSize");
            }
        }

        private int exitQuantity { get; set; }
        public string ExitQuantity
        {
            get { return exitQuantity.ToString(); }
            set
            {
                exitQuantity = int.Parse(value);
                RaisePropertyChanged("ExitQuantity");
            }
        }

        private int iterations { get; set; }
        public string Iterations
        {
            get { return iterations.ToString(); }
            set
            {
                iterations = int.Parse(value);
                RaisePropertyChanged("Iterations");
            }
        }

        private double iterationsPerSecond { get; set; }
        public string IterationsPerSecond
        {
            get { return iterationsPerSecond.ToString("0.##"); }
            set
            {
                iterationsPerSecond = double.Parse(value);
                RaisePropertyChanged("IterationsPerSecond");
            }
        }

        private double iterationLength { get; set; }
        public string IterationLength
        {
            get { return iterationLength.ToString("0.##"); }
            set
            {
                iterationLength = double.Parse(value);
                RaisePropertyChanged("IterationLength");
            }
        }
        private bool manualIteration { get; set; }
        public bool ManualIteration
        {
            get { return manualIteration; }
            set
            {
                manualIteration = value;
                RaisePropertyChanged("ManualIteration");
            }
        }

        private int currentIteration { get; set; }
        public string CurrentIteration
        {
            get { return $"Iteration: {currentIteration.ToString()}"; }
            set
            {
                currentIteration = int.Parse(value);
                RaisePropertyChanged("CurrentIteration");
            }
        }

        private bool agentReadyToPlay { get; set; }
        public bool AgentReadyToPlay 
        {
            get { return agentReadyToPlay; }

            set
            {
                agentReadyToPlay = value;
                RaisePropertyChanged("AgentReadyToPlay");
            }
        }

        private bool environmentReadyForLearning { get; set; }
        public bool EnvironmentReadyForLearning
        {
            get { return environmentReadyForLearning; }

            set
            {
                environmentReadyForLearning = value;
                RaisePropertyChanged("EnvironmentReadyForLearning");
            }
        }

        private bool unlimitedIterations { get; set; }
        public bool UnlimitedIterations
        {
            get { return unlimitedIterations; }

            set
            {
                unlimitedIterations = value;
                RaisePropertyChanged("UnlimitedIterations");
            }
        }

        #endregion

        public AreaModel AreaModel { get; set; }

        public QLViewModel()
        {
            WorldSize = 12.ToString();
            Iterations = 100.ToString();
            ExitQuantity = 3.ToString();
            IterationsPerSecond = 7.ToString();
            IterationLength = 15.ToString();
            ManualIteration = false;
            CurrentIteration = 0.ToString();
            AgentReadyToPlay = false;
        }

        public void GenerateNewArea()
        {
            AreaModel = new AreaModel(worldSize);
            AreaModel.InitializeRandomly(exitQuantity);
        }
    }
}
