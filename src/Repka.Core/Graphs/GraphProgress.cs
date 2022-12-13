using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Graphs
{
    public class GraphProgress
    {
        public virtual void Start(string message)
        {
        }

        public virtual void Report(string progress)
        {
        }

        public virtual void Finish(string message)
        {
        }

        public class StdIO : GraphProgress
        {
            public override void Start(string message)
            {
                Console.WriteLine(message);
            }

            public override void Report(string progress)
            {
                Console.WriteLine(progress);
            }

            public override void Finish(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
}
