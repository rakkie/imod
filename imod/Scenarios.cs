using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace imod
{

    class Scenarios
    {
        List<Stats> scenario1 = new List<Stats>();
        List<Stats> scenario2 = new List<Stats>();
        List<Stats> scenario3a = new List<Stats>();
        List<Stats> scenario3b = new List<Stats>();
        List<Stats> scenario3c = new List<Stats>();
        List<Stats> scenario3d = new List<Stats>();

        List<Stats> scenario4 = new List<Stats>();

        public void runScenario1(string filename)
        {
            Parameters p = new Parameters();
            Instance inst = new Instance(filename);
            Simulator sim = new Simulator(inst);

            // note: using 1.01 here stops precision errors... should really use an epsilon instead
            for (double ratio = 0.0; ratio < 1.01; ratio += p.stepSize)
//            for (double ratio = 0.4; ratio < 0.41; ratio += p.stepSize)
            {
                p.ratio = ratio;
                scenario1.Add(sim.simulate(p));
            }

            var lines = new List<string>();

            Console.WriteLine("instance,ratio,cycleLength,iterations,distance,waitingTime,excessTime,rejects,bumps,trips,success");

            foreach (Stats i in scenario1)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario1.csv", lines);

            /*
            foreach (Customer i in inst.customers.Values)
            {
                Console.Write("Customer:" + i.id + "\tBooking:" + i.bookedTime + "\tRequest:" + i.requestTime);
                Console.Write("\tPickup:" + i.pickupTime + "\tDropoff:" + i.dropoffTime);
                Console.Write("\tType: " + (i.immediate == true ? "immediate" : "advance"));
                Console.WriteLine();
            }
            */

            scenario1.Clear();
        }


        public void runScenario2(string filename)
        {
            Parameters p = new Parameters();
            Instance inst = new Instance(filename);
            Simulator sim = new Simulator(inst);

            p.sortByDistance = true;

            for (double ratio = 0.0; ratio < 1.01; ratio += p.stepSize)
//            for (double ratio = 0.9; ratio < 1.01; ratio += 0.01)
//            for (double ratio = 0.08; ratio < 0.11; ratio += 0.01)

            {
                p.ratio = ratio;
                scenario2.Add(sim.simulate(p));
            }

            var lines = new List<string>();

            Console.WriteLine("instance,ratio,cycleLength,iterations,distance,waitingTime,excessTime,rejects,bumps,trips,success");

            foreach (Stats i in scenario2)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario2.csv", lines);

            scenario2.Clear();
        }

        public void runScenario3(string filename)
        {
            Parameters p = new Parameters();
            Instance inst = new Instance(filename);
            Simulator sim = new Simulator(inst);

            p.deterministic = true; // WE NEED COMPARISON RESULTS TO BE SIMILAR?
            p.sortByDistance = true;
            p.iterations = 1;

            int count = 0;

            // 10% of customers closest to depot
            foreach (Customer c in inst.customers.Values.OrderBy(o=>inst.dist(o.id,0))) {

                if (c.id == 0) continue; // skip depot

                count++;
                if (count > inst.customers.Count/10.0) break;


                int id = c.id;

                Console.WriteLine();
                Console.WriteLine("-- Customer: " + c.id + " ; Dist = " + inst.dist(c.id,0));

                for (double ratio = 0.0; ratio <= 0.11; ratio += 0.1)
                {
                    p.ratio = ratio;
                    p.iteration++;

                    p.flip1 = id;
                    scenario3a.Add(sim.simulate(p));
                    sim.display(id);

                    p.flip1 = 0;
                
                    p.flip2 = id;
                    scenario3b.Add(sim.simulate(p));
                    sim.display(id);
                    p.flip2 = 0;

                }
            }

            count = 0;

            // 10% of customers furthest from depot
            foreach (Customer c in inst.customers.Values.OrderBy(o =>-inst.dist(o.id, 0)))
            {

                if (c.id == 0) continue; // skip depot

                count++;
                if (count > inst.customers.Count / 10.0) break;


                int id = c.id;

                Console.WriteLine("-- Customer: " + c.id + " ; Dist = " + inst.dist(c.id, 0));

                for (double ratio = 0.0; ratio <= 1.0; ratio += 0.1)
                {
                    p.ratio = ratio;
                    p.iteration++;

                    p.flip1 = id;
                    scenario3c.Add(sim.simulate(p));
                    p.flip1 = 0;

                    p.flip2 = id;
                    scenario3d.Add(sim.simulate(p));
                    p.flip2 = 0;

                }
            }




            var lines = new List<string>();

            Console.WriteLine("instance,ratio,cycleLength,iterations,distance,waitingTime,excessTime,rejects,bumps,trips,success");

            foreach (Stats i in scenario3a)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario3a.csv", lines);

            lines.Clear();

            foreach (Stats i in scenario3b)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario3b.csv", lines);

            lines.Clear();

            foreach (Stats i in scenario3c)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario3c.csv", lines);

            lines.Clear();

            foreach (Stats i in scenario3d)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario3d.csv", lines);


            scenario3a.Clear();
            scenario3b.Clear();
            scenario3c.Clear();
            scenario3d.Clear();

        }

        public void runScenario4(string filename)
        {
            Parameters p = new Parameters();
            Instance inst = new Instance(filename);
            Simulator sim = new Simulator(inst);

            var cycleLengths = new List<int>();
            cycleLengths.Add(15);
            cycleLengths.Add(30);
            cycleLengths.Add(60);

            // Remove any requests that are too long to fit in the minimum cycle length
//            inst.filter(cycleLengths.Min());

            foreach (int length in cycleLengths)
            {
                p.cycleLength = length;
                for (double ratio = 0.0; ratio <= 1.0; ratio += p.stepSize)
//            for (double ratio = 0.3; ratio < 0.31; ratio += p.stepSize)

                {
                    p.ratio = ratio;
                    scenario4.Add(sim.simulate(p));
                }
            }

            var lines = new List<string>();

            Console.WriteLine("instance,ratio,cycleLength,iterations,distance,waitingTime,excessTime,rejects,bumps,trips,success");

            foreach (Stats i in scenario4)
            {
                Console.WriteLine(i.toString());
                lines.Add(i.toString());
            }

            File.AppendAllLines("c:\\results\\scenario4.csv", lines);

            /*
            foreach (Customer i in inst.customers.Values)
            {
                Console.Write("Customer:" + i.id + "\tBooking:" + i.bookedTime + "\tRequest:" + i.requestTime);
                Console.Write("\tPickup:" + i.pickupTime + "\tDropoff:" + i.dropoffTime);
                Console.Write("\tType: " + (i.immediate == true ? "immediate" : "advance"));
                Console.WriteLine();
            }
            */
             
            scenario4.Clear();
        }
    }
}
