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
        private static Database instance;
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
            instance = this;
            this.Database.EnsureCreated();
        }
        public DbSet<Medicine> Catalog { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public string DbPath { get; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
        
    }
}
