using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imod
{
    struct Position
    {
        public int request;
        public int pos;
        public double cost;

        public Position(int request, int pos, double cost)
        {
            this.request = request;
            this.pos = pos;
            this.cost = cost;
        }
    }

    class Solution
    {
        int[] next;
        int[] prev;
        int[] location;

        public double cachedCost = -1;
        Instance inst;

        int v; // first vehicle
//        int k; // number of vehicles

        double[] finishTime; // finish time for each route

        public List<int> open = new List<int>();
        public List<int> closed = new List<int>();

        /*
        public Solution()
        {
//            cost = 0;
        }
        */

        public Solution(Solution other)
        {
            this.inst = other.inst;
//            this.cost = other.cost;
            this.v = other.v;

            this.open = new List<int>(other.open);
            this.closed = new List<int>(other.closed);

            this.next = new int[other.next.Length];
            this.prev = new int[other.prev.Length];
            this.location = new int[other.location.Length];
            this.finishTime = new double[other.finishTime.Length];

            Array.Copy(other.next, this.next, other.next.Length);
            Array.Copy(other.prev, this.prev, other.prev.Length);
            Array.Copy(other.location, this.location, other.location.Length);

            Array.Copy(other.finishTime, this.finishTime, other.finishTime.Length);
        }

        public Solution(Instance inst, int numVehicles, double startTime, List<int> cust)
        {
            this.inst = inst;
            // cost = 0;

            v = cust.Count;

            int n = cust.Count + 2 * numVehicles;

            next = new int[n];
            prev = new int[n];
            location = new int[n];

            finishTime = new double[numVehicles];
            for (int i = 0; i < numVehicles; i++)
                finishTime[i] = startTime;

            for (int i = 0; i < n; i++)
            {
                next[i] = -1;
                prev[i] = -1;
                if (i < cust.Count)
                {
                    location[i] = cust[i];
                    open.Add(i);
                }
            }

            for (int i = cust.Count; i < cust.Count + numVehicles; i++)
            {
                location[i] = 0;
                next[i] = i + numVehicles;
                prev[i] = -1;

                location[i + numVehicles] = 0;
                next[i + numVehicles] = -1;
                prev[i + numVehicles] = i;
            }

        }

        public Solution(Instance inst, List<Vehicle> vehicles, List<int> cust)
        {
            this.inst = inst;

//            cost = inst.dist(vehicles[0].current.id,0);

            v = cust.Count;

            int n = cust.Count + 2*vehicles.Count;

            next = new int[n];
            prev = new int[n];
            location = new int[n];

            finishTime = new double[vehicles.Count];

            for (int k = 0; k < vehicles.Count; k++) 
            {
                finishTime[k] = vehicles[k].time;   
            }


            for (int i = 0; i < n; i++)
            {
                next[i] = -1;
                prev[i] = -1;
                if (i < cust.Count)
                {
                    location[i] = cust[i];
                    open.Add(i);
                }
            }

            for (int i = cust.Count; i < cust.Count + vehicles.Count; i++)
            {
                location[i]     = vehicles[i-cust.Count].current.id;
                next[i]         = i + vehicles.Count;
                prev[i]         = -1;

                location[i + vehicles.Count]    = 0;
                next[i + vehicles.Count]        = -1;
                prev[i + vehicles.Count]        = i;

//                cost += inst.dist(location[i], 0);
            }

        }

        public double routeCost(int route)
        {
//            double cost = 0;
            double cost = finishTime[route];
            if (closed.Count == 0) return cost;
            for (int i = v + route; next[i] != -1; i = next[i])
                cost += dist(i, next[i]);
            return cost;
        }

        public double cost()
        {
            if (cachedCost > 0)
                return cachedCost;

            double cost = 0;
            for (int route = 0; route < finishTime.Length; route++) {
                double a = routeCost(route);
                if (a > cost)
                    cost = a;
            }
            cachedCost = cost;
            return cost;
        }

        double dist(int i, int j)
        {
            return inst.distMatrix[location[i],location[j]];
        }


        private double insertionCost(int request, int pos)
        {
            double oldCost = dist(pos, next[pos]);
            double newCost = dist(pos, request) +
                          dist(request, next[pos]);

            return newCost - oldCost;
        }

        public void insert(Position p) {
//            cost += insertionCost(p.request, p.pos);
            cachedCost = -1;

            // Insert Pickup
            next[p.request] = next[p.pos];
            prev[p.request] = p.pos;
            prev[next[p.request]] = p.request;
            next[p.pos] = p.request;

            open.Remove(p.request);
            closed.Add(p.request);
        }

        public double removalCost(int request)
        {
            double oldCost = dist(prev[request], request) +
                          dist(request, next[request]);

            double newCost = dist(prev[request], next[request]);

            return newCost - oldCost;
        }

        public void remove(int request)
        {
//            cost += removalCost(request);
            cachedCost = -1;

            // remove pickup
            next[prev[request]] = next[request];
            prev[next[request]] = prev[request];

            next[request] = -1;
            prev[request] = -1;

            closed.Remove(request);
            open.Add(request);
        }

        public Position greedyCost(int request, int route)
        {
            
            Position result = new Position(request, -1, Double.PositiveInfinity);

            for (int i = v+route; next[i] != -1; i = next[i])
            {   
 
                double pickupCost = dist(i, request)
                                  + dist(request, next[i])
                                  - dist(i, next[i]);

                if (pickupCost < result.cost)
                {
                    result.cost = pickupCost;
                    result.pos = i;
                }

            }

            return result;
            
        }


        public List<int> toList(int route)
        {
            if (cost() == 0) return new List<int>();

            var list = new List<int>();
            for (int i = next[v+route]; i != -1; i = next[i])
            {
                list.Add(location[i]);
            }
            return list;
        }

        public void print()
        {
            Console.Write("  Best Solution: ");
            Console.WriteLine(this.cost());
            
        }



        public void greedyInsertAll()
        {
            while (open.Count > 0)
            {
                Position best = new Position();
                best.cost = double.PositiveInfinity;

                for (int route = 0; route < finishTime.Length; route++) 
                {
                    double startCost = routeCost(route);
                    foreach (int req in open)
                    {
                        Position p = greedyCost(req, route);
                        p.cost += startCost;
                        if (p.cost < best.cost)
                            best = p;
                    }
                }

                insert(best);
            }
        }
    }

    class Solver2
    {
        Instance inst;
        Solution sol;
        Random random = new Random();
        int K = 10;

        public Solver2(Instance instance, Parameters p)
        {
            inst = instance;
            if (p.deterministic)
                random = new Random(p.iteration);
            K = p.K;
        }

        void removeRandom(Solution s, int count)
        {
            for (int i = 0; i < Math.Min(count, s.closed.Count); i++)
            {
                int request = s.closed[random.Next(s.closed.Count)];
                s.remove(request);
            }
        }


        void move(Solution s)
        {
            int count = 4;
            removeRandom(s, count);
            s.greedyInsertAll();
        }

        void getGreedySolution(Solution s) 
        {
            s.greedyInsertAll();
        }

        public Solution solveVRP(List<int> customers, List<Vehicle> vehicles, double startTime, int iterations, bool save)
        {
            int count = K;

            if (customers.Count == 0)
                return new Solution(inst, count, startTime, customers);


            if (vehicles != null)
            {
                    sol = new Solution(inst, vehicles, customers);
                    /*   Console.WriteLine("  Vehicles:");
                       foreach (Vehicle v in vehicles)
                       {
                           Console.WriteLine("  " + v.time + " : " + v.current.id);
                       }
                    */
                    count = vehicles.Count;    
            }
            else
            {
                    sol = new Solution(inst, count, startTime, customers);
                    //              Console.WriteLine("  Vehicles: " + count);
            }
           
            getGreedySolution(sol);

            var best = new Solution(sol);
                
            for (int i = 0; i < iterations; i++)
            {
                move(sol);
                // Greedy improvement
                if (sol.cost() < best.cost())
                    best = new Solution(sol);
            }

   //         best.print();


            /*
            for (int i = 0; i < count; i++)
            {
                Console.Write("  Route " + i + ": ");
                foreach (int j in best.toList(i))
                {
                    Console.Write("  " + j);
                }
                Console.WriteLine();
            }
            */

            if (save) 
                for (int i = 0; i < vehicles.Count; i++)
                    vehicles[i].nextMoves = best.toList(i);

            return best;
        }
        
    }
}
