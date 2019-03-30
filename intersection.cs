using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace traffic_light
{

    class intersection
    {
        public int X, Y;
        public light L1;
        public light L2;
        public light L3;
        public light L4;
        public light NL1;
        public light NL2;
        public light NL3;
        public light NL4;
        public intersection(int X, int Y, light L1, light L2, light L3, light L4,
                                        light NL1, light NL2, light NL3, light NL4
            )
        {
            this.X = X;
            this.Y = Y;
            this.L1 = L1;
            this.L2 = L2;
            this.L3 = L3;
            this.L4 = L4;
            this.NL1 = NL1;
            this.NL2 = NL2;
            this.NL3 = NL3;
            this.NL4 = NL4;
        }
    }
}
