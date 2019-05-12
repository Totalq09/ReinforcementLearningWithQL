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

        public CellModel(CellContentEnum cellContent)
        {
            this.CellContent = cellContent;
        }
    }
}
