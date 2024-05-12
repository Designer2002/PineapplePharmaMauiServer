using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_db
{
    public enum Countries
    {
        USA,
        UK,
        Japan,
        France,
        Germany
    }
    public class Medicine
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string ActiveComponent { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReleaseForm { get; set; }
        public string Distributer { get; set; }
        public int Expiration { get; set; }
        public double Price { get; set; }
        public bool Prescription { get; set; }
    }

    public class MedicineShoppingCartView
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }

    
}
