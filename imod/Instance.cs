using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace imod
{
    class Customer
    {
        public int requestTime { get; set; }
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }

        public Customer(int id, int time, int x, int y)
        {
            this.requestTime = time;
            this.id = id;
            this.x = x;
            this.y = y;
        }

        // STATS
        public double bookedTime { get; set; }
        public double pickupTime { get; set; }
        public double dropoffTime { get; set; }

        public bool immediate = false;
        public bool rejected = false;

        public double weight = 1;

        public double waitTime()
        {
            if (!immediate) return 0;
            return Math.Abs(pickupTime - bookedTime);
        }

        public double travelTime()
        {
            return (dropoffTime - pickupTime);
        }

    }

    class Instance
    {
        public Dictionary<int,Customer> customers;
        public string name;
        public double[,] distMatrix;

        public int start { get; set; }
        public int end { get; set; }

        bool station = true;

        public Instance(string filename) {

            name = Path.GetFileNameWithoutExtension(filename).ToLower();

            if (name.StartsWith("station"))
                station = true;
            else
                station = false;

            int max = 0;

            customers = new Dictionary<int,Customer>();

            // Add Depot
            customers[0] = new Customer(0, 0, 0, 0);

            var lines = File.ReadAllLines(filename);

            start = int.MaxValue;
            end = int.MinValue;

            foreach (var line in lines.Skip(1))
            {
                var val = line.Split(',');
                int id = int.Parse(val[0]);
                int time = int.Parse(val[1]);
                int x = int.Parse(val[2]);
                int y = int.Parse(val[3]);

                customers[id] = new Customer(id, time, x, y);

                start = Math.Min(time, start);
                end = Math.Max(time, end);
                max = Math.Max(id, max);
            }

//            Console.WriteLine("start = " + start);
//            Console.WriteLine("end = " + end);
            createDistanceMatrix(max);
        }
        
        void createDistanceMatrix(int max)
        {
            distMatrix = new double[max+1,max+1];

            foreach (int i in customers.Keys) {
                foreach (int j in customers.Keys) {
                    distMatrix[i,j] = getDist(i,j);
                }
            }
        }

        double getDist(int i, int j)
        {
            return (Math.Abs(customers[i].x - customers[j].x) +
                   Math.Abs(customers[i].y - customers[j].y)) / 1000.00;
        }

        public double dist(int i, int j)
        {
//            return distMatrix[customers[i].id, customers[j].id];
            return distMatrix[i, j];
        }

        public bool fromStation()
        {
            return station;
        }

        public void filter(int maxLength)
        {
            int count = customers.Count;
            customers = customers.Where(id => 2*dist(0, id.Key) < maxLength).ToDictionary(id => id.Key, id => id.Value);
            createDistanceMatrix(customers.Keys.Max());

            Console.WriteLine("Filtered " + (count - customers.Count) + " requests.");
        }

    }
}
