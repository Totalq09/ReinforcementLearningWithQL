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

        #endregion

        public AreaModel AreaModel { get; set; }

        public QLViewModel()
        {
            WorldSize = 8.ToString();
            Iterations = 100.ToString();
            ExitQuantity = 3.ToString();
            IterationsPerSecond = 3.ToString();
        }

        public void GenerateNewArea()
        {
            AreaModel = new AreaModel(worldSize);
            AreaModel.InitializeRandomly(exitQuantity);
        }

    }
}
