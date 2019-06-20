using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Models
{
    public class CellModel
    {
        public CellContentEnum CellContent { get; set; }

        public double CurrentReward0 { get; set; }
        public double CurrentReward1 { get; set; }
        public double CurrentReward2 { get; set; }
        public double CurrentReward3 { get; set; }

        public CellModel(CellContentEnum cellContent)
        {
            this.CellContent = cellContent;
            this.CurrentReward0 = 0.0;
            this.CurrentReward1 = 0.0;
            this.CurrentReward2 = 0.0;
            this.CurrentReward3 = 0.0;
        }
    }
}
