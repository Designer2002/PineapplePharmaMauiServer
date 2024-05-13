using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_db
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        public List<MedicineShoppingCartView> View { get; set; }
        
    }
}
