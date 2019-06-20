using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Helpers
{
    public class IterationUpdateEventArgs: EventArgs
    {
        public int Iteration { get; set; }
    }
}
