using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace traffic_light
{
    class light
    {
        public int id;
        public int time;
        public int traffic;
        public string state;//unknown,  red  ,green , blink
        public bool allowIncrease;
        public double ANT;//average neighbour traffic
        public light(int id,int time, int traffic , string state)
        {
            this.id = id;
            this.time = time;
            this.traffic = traffic;
            this.state = state;
            allowIncrease = true;
        }

    }
}
