using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    public class Move
    {
        public bool isCross { get; set; }
        public int position { get; set; }

        public Move(bool isCross, int x) 
        {
            this.isCross = isCross;
            this.position = x;
        }
    }
}
