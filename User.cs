using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_db
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        [Key]
        public string Email { get; set; }
        public string Role { get; set; }
        public List<MedicineShoppingCartView> ShoppingCart { get; set;}
    }
}
