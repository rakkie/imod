using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imod
{
    class Vehicle
    {
        public List<int> nextMoves = new List<int>();          // list of customers to visit
        public Event last;
        int id;

        public bool moving = false;
        public Event current; // the current location vehicle is going towards or has arrived at
        public List<int> passengers = new List<int>(); // list of passengers currently on the vehicle

        public double time = 0;

        public Vehicle(int id)
        {
            this.id = id;
            last = new Event(0, EventType.Move, 0, id);
            current = new Event(0, EventType.Move, 0, id);
        }
            

        public void reset()
        {
            nextMoves = new List<int>();
            last = new Event(0, EventType.Move, 0, id);
            moving = false;
            current = new Event(0, EventType.Move, 0, id);
            passengers = new List<int>();
        }
    }
}
