using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.UI.Models
{
    public class Transformer
    {
        public int Id { get; set; }
        public double PowerKVA { get; set; } 
        public double PowerFactor { get; set; }
        public decimal Price { get; set; }
    }
}
