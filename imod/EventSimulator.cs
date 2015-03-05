using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Priority_Queue;

namespace imod
{
    enum EventType { Advance, Immediate, StartRoute, Move };

    class Event : PriorityQueueNode
    {
        public double time;
        public EventType type;
        public int id;
        public int route;

        public Event(double time, EventType type, int id, int route)
        {
            this.time = time;
            this.type = type;
            this.id = id;
            this.route = route;
        }

        public string toString()
        {
            string typeStr = "";
            if (type == EventType.Advance) typeStr = "Advance";
            else if (type == EventType.Immediate) typeStr = "Immediate";
            else if (type == EventType.Move) typeStr = "Move";
            else if (type == EventType.StartRoute) typeStr = "Start Route";
            return time + ": " + typeStr + ": " + id;
        }
    }

    class Record
    {
        public double total = 0.0;
        public int observations = 0;

        public Record() { }
        public double Average()
        {
            if (observations == 0) return 0;
            return total / observations;
        }
    }

    class Simulator
    {
        Instance inst;                                  // the instance to solve
        Solver2 solver;                                 // VRP solver used for routing
        Parameters p;

        Dictionary<int, Solution> solutions = new Dictionary<int, Solution>();

        Random rnd = new Random();                      
        List<Vehicle> vehicles = new List<Vehicle>();
        HeapPriorityQueue<Event> events;

        double totalDist = 0;

        int MAXCYCLES = 1024;
        int K;

        int currentCycle; // index of the cycle we are currently in
        int lastCycle;
        int cycleLength; 
        int advanceTime;

        int immediate = 0;
        int advance = 0;

        Dictionary<double, List<int>> cycles = new Dictionary<double, List<int>>();

        public Simulator(Instance instance)
        {
            inst = instance;
            events = new HeapPriorityQueue<Event>(inst.customers.Count + MAXCYCLES);
            p = new Parameters();
            K = p.K;
            solver = new Solver2(inst,p);
           
            for (int i = 0; i < K; i++)
            {
                vehicles.Add(new Vehicle(i));
            }
            
        }

        void handleAdvance(Event e)
        {
            int cycle = getCycle(e.time+advanceTime);

//            Console.WriteLine("id " + e.id + ": " + inst.customers[e.id].x + "," + inst.customers[e.id].y);
//            Console.WriteLine("depot: " + inst.customers[0].x + "," + inst.customers[0].y);
//            Console.WriteLine("  Dist: " + (inst.dist(e.id,0) * 2)); 

            cycle = assign(e.id, cycle, cycle);

            if (cycle == -1)
            {
//                Console.WriteLine("Rejected " + e.id);
                inst.customers[e.id].rejected = true;
            }
        }

        int getCycle(double time)
        {
            return (((int)time) / cycleLength) * cycleLength;
        }

        void queueNext(int vehicle, double time)
        {
            Vehicle v = vehicles[vehicle];

            if (inst.fromStation() && v.last.id == 0)
            {
                foreach (int i in cycles[currentCycle])
                {
                    v.passengers.Add(i);
                    inst.customers[i].pickupTime = time;
                }
            }


            if (v.nextMoves.Count == 0) return;

            int next = v.nextMoves.First();

            v.nextMoves.Remove(next);
            cycles[currentCycle].Remove(next);
            double t = time + inst.dist(v.last.id, next);
            Event eNext = new Event(t, EventType.Move, next, vehicle);

  //          Console.Write("Route " + e.route + ":");
  //          Console.WriteLine("  Queued " + vehicles[e.route].current.id + " -> " + next + " : " + vehicles[e.route].current.time + " -> " + t); 

            events.Enqueue(eNext, eNext.time);
            v.current = eNext;
            v.moving = true;
//            inst.customers[curr.id].bookedTime = curr.time;
        }

        /*
        int assign(int id, int cycle, double timeLeft)
        {
            cycles[cycle].Add(id);
            Solution s = solver.solveVRP(cycles[cycle], vehicles, 0, false);
            if (s.cost > timeLeft - p.delta)
            {
                cycles[cycle].Remove(id);
                return -1;
            }
            return cycle;
        }
        */

        int assign(int id, int cycle, double startTime)
        {
            cycles[cycle].Add(id);

            Solution s;

            if (startTime % cycleLength == 0)
                s = solver.solveVRP(cycles[cycle], null, startTime, p.optCycles, false);
            else
                s = solver.solveVRP(cycles[cycle], vehicles, startTime, 0, false);


//            Console.WriteLine("  cost: " + s.cost() + " ; startTime: " + startTime + " ; cycle:" + cycle);
            if (s.cost() > cycle + cycleLength - p.buffer)
            {
 //               Console.WriteLine("  reject");
                cycles[cycle].Remove(id);
                cycle += cycleLength;
                cycles[cycle].Add(id);
                s = solver.solveVRP(cycles[cycle], null, cycle, p.optCycles, false);

 //               Console.WriteLine("  cost: " + s.cost + " ; timeLeft: " + cycleLength + "; cycle:" + cycle);
                if (s.cost() > cycle + cycleLength - p.buffer)
                {
                    cycles[cycle].Remove(id);
 //                   Console.WriteLine("  Rejected " + id);
                    return -1;
                }
            }
 //           else
 //               Console.WriteLine("  accept");
            solutions[cycle] = s;
            return cycle;
        }


        void handleMove(Event e)
        {
//            statMove += 1;

            Vehicle v = vehicles[e.route];

            totalDist += inst.dist(v.last.id, e.id);
            v.last = e;

            if (v.nextMoves.Count > 0)
            {
                queueNext(e.route,e.time);
            }
            else
            {
                v.moving = false;
            }
 
            // HANDLE PICKUPS & DROPOFFS
            if (e.id == 0) // arrived at station
            {
                if (inst.fromStation())
                {
                    // nothing... finished route
                    // Console.WriteLine("Finished route");
                }
                else { // drop off all the passengers
                    foreach (int i in v.passengers)
                    {
                        inst.customers[i].dropoffTime = e.time;
                        double excessTime = (inst.customers[i].dropoffTime - inst.customers[i].pickupTime) - inst.dist(i, 0);
                        if (excessTime < -0.001)
                        {
                            double d = inst.dist(i, 0);
                            Console.WriteLine("Error! - Impossible Travel Time - " + i + ": " + excessTime.ToString("F2"));
                            Console.WriteLine("Dropoff: " + inst.customers[i].dropoffTime + " ; Pickup: " + inst.customers[i].pickupTime + " ; Dist " + d);
                        }
                    }
                    v.passengers.Clear();
                }
            }
            else // arrived at a customer 
            {
                if (inst.fromStation()) {
                    v.passengers.Remove(e.id);
                    inst.customers[e.id].dropoffTime = e.time;
                }
                else {
                    v.passengers.Add(e.id);
                    inst.customers[e.id].pickupTime = e.time;
                }
            }
        }

        void handleImmediate(Event e)
        {
            // Update potential starting times for each vehicle
            updateTime(e.time);

            // See if request can be served by current or next cycle
            int cycle = assign(e.id, currentCycle, e.time);
            

            if (cycle == currentCycle)
            {
                // Console.WriteLine("Adding to current cycle");

                Solution s = solver.solveVRP(cycles[cycle], vehicles, e.time, 0, true);

                // Start vehicles moving if necessary
                for (int i = 0; i < vehicles.Count(); i++)
                    if (!vehicles[i].moving)
                        queueNext(i,e.time);
            }
            else if (cycle == -1)
            {
//                Console.WriteLine("  Rejected request: " + e.id + " ; Time Left : " + timeLeft);
                // add to rejected passengers list
                inst.customers[e.id].rejected = true;
            }

        }

        void updateTime(double time)
        {
            foreach (Vehicle v in vehicles)
            {
                v.time = Math.Max(time, v.current.time);
            }
        }

        void handleStartRoute(Event e)
        {

            Vehicle v = vehicles[e.route];

            currentCycle = (int)e.time;

            if (v.current.time > currentCycle)
            {
                Console.WriteLine("Error: Vehicle arrived too late!");
                Console.WriteLine("  Arrived at: " + v.current.time + ", should have finished by " + currentCycle);
            }

            /*
            Console.Write("Current Cycle:" + currentCycle + ": ");
            foreach (int i in cycles[currentCycle])
                Console.Write(i + " ");
            Console.WriteLine(": end");
            */

//            Console.WriteLine("Cycle " + e.time + ": " + cycles[currentCycle].Count + " planned passengers");

/*            if (v.nextMoves.Count > 0)
            {
                foreach (int i in v.nextMoves)
                {
                    if (i != 0)
                        cycles[currentCycle].Add(i);
                    Console.Write(i + " ");
                }
//                statBumped += nextMoves.Count;
                Console.WriteLine(" : Bumped " + (v.nextMoves.Count-1) + " passengers");
            }
*/
            if (cycles[currentCycle].Count == 0)
                return;

            Solution s = solutions[currentCycle];

            for (int i = 0; i < vehicles.Count; i++)
                vehicles[i].nextMoves = s.toList(i);

            /*
            // Update start time availability for vehicles
            updateTime(e.time);
            
            if (e.route == 0) { // don't solve it again for each vehicle!
                s = solver.solveVRP(cycles[currentCycle], vehicles, e.time, p.optCycles*100, true);
             
//                Console.WriteLine("  finish at: " + s.cost());
             */

                if (s.cost() > currentCycle + cycleLength - p.buffer)
                {
                    Console.WriteLine("  ERROR: Cycle too long");
                }
                
            
            queueNext(e.route, e.time);
        }

        bool processEvent()
        {
            if (events.Count == 0)
                return false;

            Event e = events.Dequeue();

            // DEBUG
//            if (e.id == 27)
//                    Console.WriteLine(e.toString());
//            Console.WriteLine(e.toString());


//            if (e.time == 0)
//                return true;
//                    Console.WriteLine("here");

            if (e.type == EventType.Advance) handleAdvance(e);
            else if (e.type == EventType.Immediate) handleImmediate(e);
            else if (e.type == EventType.Move) handleMove(e);
            else if (e.type == EventType.StartRoute) { handleStartRoute(e); }
//            if (e.type == EventType.StartRoute) { handleStartRoute(e); Environment.Exit(0); }

            return true;
        }

        void createEvents(double ratio) {

            immediate = 0;
            advance = 0;

            if (p.deterministic)
                rnd = new Random(p.iteration);  

            if (p.sortByDistance == true)
            {
                int count = 0;

                foreach (Customer i in inst.customers.Values.OrderBy(o=>inst.dist(o.id,0)))
                {

                    if (i.id == 0) continue; // skip depot

                    i.dropoffTime = 0;
                    i.pickupTime = 0;
                    i.rejected = false;
                    i.immediate = false;

                    double pos = (double)count / (inst.customers.Count-1);

                    if (i.id == p.flip1)
                    {
                        // set p.flip1 to advance
                        events.Enqueue(new Event(i.requestTime - advanceTime, EventType.Advance, i.id, 0), i.requestTime - advanceTime);
                        i.bookedTime = i.requestTime - advanceTime;
                    }
                    else if (i.id == p.flip2)
                    {
                        // set p.flip2 to immediate
                        events.Enqueue(new Event(i.requestTime, EventType.Immediate, i.id, 0), i.requestTime);
                        i.bookedTime = i.requestTime;
                        i.immediate = true;
                    }
                    else if ((pos != 0) && (rnd.NextDouble() < Math.Pow(ratio,pos)))

//                  else if (((double)count / inst.customers.Count) <= ratio)
                    {
                        events.Enqueue(new Event(i.requestTime, EventType.Immediate, i.id, 0), i.requestTime);
                        i.bookedTime = i.requestTime;
                        i.immediate = true;
                        immediate++;
                    }
                    else
                    {
                        events.Enqueue(new Event(i.requestTime - advanceTime, EventType.Advance, i.id, 0), i.requestTime - advanceTime);
                        i.bookedTime = i.requestTime - advanceTime;
                        advance++;
                    }

                    count++;
                }
           
            }
            else
            {
                foreach (Customer i in inst.customers.Values.OrderBy(o =>inst.dist(o.id, 0)))
//                foreach (Customer i in inst.customers.Values)

                {
                    if (i.id == 0) continue; // skip depot

                    i.dropoffTime = 0;
                    i.pickupTime = 0;
                    i.rejected = false;
                    i.immediate = false;

                    
                 if (rnd.NextDouble() < ratio)
                    {
                        events.Enqueue(new Event(i.requestTime, EventType.Immediate, i.id,0), i.requestTime);
                        i.bookedTime = i.requestTime;
                        i.immediate = true;
                        immediate++;
                    }
                    else
                    {
                        events.Enqueue(new Event(i.requestTime - advanceTime, EventType.Advance, i.id,0), i.requestTime - advanceTime);
                        i.bookedTime = i.requestTime - advanceTime;
                        advance++;
                    }
                }
            }

            Console.WriteLine("Immediate : " + immediate/250.0);
            Console.WriteLine("Advance   : " + advance/250.0);

//            foreach (var i in 

            lastCycle = inst.end + cycleLength * 3;

            for (int i = getCycle(inst.start); i <= lastCycle; i += cycleLength)
            {
                for (int j = 0; j < K; j++)
                    events.Enqueue(new Event(i, EventType.StartRoute,0,j), i);

                cycles[i] = new List<int>();
            }

        }

        void updateWait(Record rec)
        {
            foreach (Customer i in inst.customers.Values)
            {
                if (i.id == 0) continue;

                if (i.immediate && !i.rejected)
                {
                    if (i.pickupTime - i.bookedTime < 0)
                    {
                        Console.WriteLine("ERROR - Invalid Travel Time for " + i.id);
                        Console.WriteLine("  Picked up at: " + i.pickupTime + " ; Dropped off at " + i.dropoffTime);
                        i.rejected = true;
                    }
                    else
                    {
                        rec.observations++;
                        rec.total += (i.pickupTime - i.bookedTime);
                    }
                }
            }
        }

        void updateExcess(Record rec)
        {
            foreach (Customer i in inst.customers.Values)
            {
                if (i.id == 0) continue;

                if (!i.rejected)
                {
                    rec.observations++;
                    rec.total += (i.dropoffTime - i.pickupTime) - inst.dist(i.id,0);
                }
            }
        }

        void updateRide(Record rec)
        {
            foreach (Customer i in inst.customers.Values)
            {
                if (i.id == 0) continue;

                if (!i.rejected)
                {
                    rec.observations++;
                    rec.total += (i.dropoffTime - i.pickupTime);
                }
            }
        }

        void updateSuccess(Record rec)
        {
            foreach (Customer i in inst.customers.Values)
            {
                if (i.id == 0) continue;

                if (!i.rejected) rec.total += 1;
                rec.observations++;

            }
        }

        void updateRejectedAdvance(Record rec)
        {
            foreach (Customer i in inst.customers.Values)
            {
                if (i.id == 0) continue;

                if (i.rejected && !i.immediate) rec.total += 1;
                rec.observations++;
            }
        }

        public void display(int id)
        {
            double excess = (inst.customers[id].dropoffTime - inst.customers[id].pickupTime) - inst.dist(id, 0);
            double wait = (inst.customers[id].pickupTime - inst.customers[id].bookedTime);

            Console.WriteLine("Customer " + id + ":");
            Console.Write("  Type:    ");
            if (inst.customers[id].immediate)
                Console.WriteLine("Immediate");
            else
                Console.WriteLine("Advance");
            Console.WriteLine("  Request: " + (inst.customers[id].requestTime - p.cycleLength));
            Console.WriteLine("  Pickup:  " + inst.customers[id].pickupTime);
            Console.WriteLine("  Dropoff: " + inst.customers[id].dropoffTime);
            Console.WriteLine("  Wait:    " + wait.ToString("F2"));
            Console.WriteLine("  Excess:  " + excess.ToString("F2"));
            Console.WriteLine("  Travel:  " + (inst.customers[id].dropoffTime - inst.customers[id].pickupTime).ToString("F2")); 
        }

        public Stats simulate(Parameters param)
        {
            p = param;

            Console.WriteLine("ratio = " + param.ratio.ToString("F2"));

            // sort customers if necessary by distance
            // set ratio
            totalDist = 0;

            cycleLength = param.cycleLength;
 //           advanceTime = 2 * cycleLength;
            advanceTime = cycleLength;
            int totalCount = 0;

            Record success = new Record();
            Record waitTime = new Record();
            Record excessTime = new Record();

            // for scenario3
            Record waitTime2 = new Record();
            Record excessTime2 = new Record();

            Record rejectAdv = new Record();


            for (int i = 0; i < param.iterations; i++) 
            {
                int last = (int) (success.observations - success.total);

                cycles.Clear();
                solutions.Clear();

                foreach (Vehicle v in vehicles)
                    v.reset();

                currentCycle = getCycle(inst.start);

                createEvents(param.ratio);

                while (processEvent()) { }

                updateWait(waitTime);
                updateExcess(excessTime);
                updateSuccess(success);
                updateRejectedAdvance(rejectAdv);

                totalCount += (inst.customers.Count-1);

                Console.WriteLine("Rejects: " + ((success.observations - success.total) - last));
 
            }

            Console.WriteLine("Total:   " + success.observations);
            Console.WriteLine("Rejects: " + (success.observations - success.total));
            Console.WriteLine("Rejects Adv: " + (rejectAdv.total));
            Console.WriteLine("Rate:    " + success.Average());

            return new Stats(inst.name,param,totalDist,waitTime.Average(),excessTime.Average(), 0, 0, 0, success.Average());

        }
    }
}
