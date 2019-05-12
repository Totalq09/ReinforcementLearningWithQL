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
        }
    }
}
