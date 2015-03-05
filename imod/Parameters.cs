using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imod
{
    class Parameters
    {
        // DEFAULT PARAMETERS
        public double ratio = 0;
        public int numCycles = 24; // max number of cycles
        public int cycleLength = 60;
        public int optCycles = 100;
        public int iteration = 0;
        public int iterations = 10;
        public double stepSize = 0.01;
        public int serviceTime = 0;
        public bool sortByDistance = false;
        public bool addExtra = false;
        public bool reject = true;
        public int rejectPenalty = 0;
        public int flip1 = 0;
        public int flip2 = 0;
        public bool deterministic = false;
//        public bool deterministic = true; // for debugging
        public double delta = 0.02;
        public double buffer = 1; // min amount of time between arriving at station and train departing

        public int K = 10; // number of vehicles
        public int C = 9;  // capacity

        public int MAXCYCLES = 100;

    }
}
