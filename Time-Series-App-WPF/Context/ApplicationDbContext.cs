using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Context
{
    public class ApplicationDbContext : DbContext
    {
        private string DbPath { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            DbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\sqlitedatabase.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}
