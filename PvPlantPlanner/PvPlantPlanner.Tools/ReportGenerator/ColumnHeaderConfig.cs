using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.Tools.ReportGenerator
{
    public class ColumnHeadersConfig
    {
        public double Height { get; set; }
        public List<ColumnHeaderParam> ColumnHeaders { get; set; }
    }

    public class ColumnHeaderParam
    {
        public string Name { get; set; }
        public double Width { get; set; }
    }
}
