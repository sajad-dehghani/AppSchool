using Microsoft.EntityFrameworkCore;
using NovinApp.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static MudBlazor.CategoryTypes;

namespace NovinApp.Server.MyContext
{
    public class MyAppContext : DbContext
    {
        public MyAppContext()
        {
        }

        public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
        public DbSet<Report_students> Report_students { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {




            #region Seed Data Admin
            //modelBuilder.Entity<Company>().HasData(new Company()
            //{
            //    Id = 12,
            //    companyname = "شرکت",
            //    model_name = "برنامه نویسی",
            //    RefineryId = 0
            //});


            #endregion



            base.OnModelCreating(modelBuilder);
        }


    }
}
