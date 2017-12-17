using System;
using System.Data.Entity;

namespace Z1
{

    public class TodoDbContext : DbContext
    {

        public DbSet<TodoItem> Items { get; set; }
        public DbSet<TodoLabel> Labels { get; set; }

        //Comment out ConnectionString constructor for tests.
        public TodoDbContext(string connectionString) : base($"{connectionString}") { }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //TodoItem model.
            builder.Entity<TodoItem>().HasKey(item => item.Id);
            builder.Entity<TodoItem>().HasMany(item => item.TodoLabels).WithMany(label => label.TodoItems);

            builder.Entity<TodoItem>().Property(item => item.Id).IsRequired();
            builder.Entity<TodoItem>().Property(item => item.UserId).IsRequired();
            builder.Entity<TodoItem>().Property(item => item.Text).IsRequired();
            builder.Entity<TodoItem>().Property(item => item.DateCreated).IsRequired();

            //TodoLabel model.
            builder.Entity<TodoLabel>().HasKey(label => label.Id);
            builder.Entity<TodoLabel>().HasMany(label => label.TodoItems).WithMany(item => item.TodoLabels);

            builder.Entity<TodoLabel>().Property(label => label.Id).IsRequired();
            builder.Entity<TodoLabel>().Property(label => label.Value).IsRequired();
        }

    }

}
