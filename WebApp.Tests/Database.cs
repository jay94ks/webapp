using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Tests
{
    [Table("test_models")]
    public class TestModel
    {
        [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int No { get; set; }

        [MaxLength(32)]
        public string Login { get; set; }

        [MaxLength(256)]
        public string Password { get; set; }
    }

    /// <summary>
    /// Database Service.
    /// </summary>
    public class Database : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string Server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            optionsBuilder.UseMySQL(string.Format("Server={0};Database=tesst;Uid=root", Server));
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<TestModel> Tests { get; set; }

    }

}
