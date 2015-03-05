using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imod
{
    class Stats
    {
        string instance;
        double ratio;
        int cycleLength;
        int iterations;

        double distance;
        double waitingTime;
        double excessTime;
        int rejects;
        int bumps;
        double trips;
        double success;

        public Stats(string instance, Parameters parameters, double distance, double waitingTime, double excessTravelTime, int rejects, int bumps, double trips, double success)
        {
            this.instance = instance;
            this.ratio = parameters.ratio;
            this.cycleLength = parameters.cycleLength;
            this.iterations = parameters.iterations;
            this.distance = distance;
            this.waitingTime = waitingTime;
            this.excessTime = excessTravelTime;
            this.rejects = rejects;
            this.bumps = bumps;
            this.trips = trips;
            this.success = success;
        }

        public string toString()
        {
            return instance + "," + ratio.ToString("F2") + "," + cycleLength + "," + iterations + "," +
                distance.ToString("F2") + "," + waitingTime.ToString("F2") + "," + excessTime.ToString("F2")
                + "," + rejects + "," + bumps + "," + trips.ToString("F2") + "," + success.ToString("F3");
        }
    }
}
