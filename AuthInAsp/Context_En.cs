using AuthInAsp.model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AuthInAsp
{
    public class Context_En : DbContext
    {
        public Context_En(DbContextOptions<Context_En> options)
        : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(Context_En).Assembly);
        }



        public DbSet<User_En> user_Ens { get; set; }

    }
}
