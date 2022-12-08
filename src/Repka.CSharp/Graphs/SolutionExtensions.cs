using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Graphs
{
    public static class SolutionExtensions
    {
        public static SolutionFile? ToSolution(this FileInfo file)
        {
            try
            {
                return SolutionFile.Parse(file.FullName);
            }
            catch
            {
                return null;
            }
        }
    }
}
