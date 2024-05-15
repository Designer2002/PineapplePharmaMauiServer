using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_db
{
    public class Database : DbContext
    {
        private static Database instance { get; set; }  
        public static Database GetInstance()
        {
            if (instance == null)
                instance = new Database();
            return instance;
        }
        public Database()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "medicine.db");
            this.Database.EnsureCreated();
            //this.ChangeTracker.Clear();
            instance = this;
        }
        public DbSet<Medicine> Catalog { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<MedicineShoppingCartView> CartViews { get; set; } = null!;
        public string DbPath { get; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
            options.LogTo(Console.WriteLine);
            //options.EnableSensitiveDataLogging();
        }
        
        
    }
}
