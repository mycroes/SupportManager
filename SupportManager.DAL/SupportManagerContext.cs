using System;
using System.Data;
using System.Data.Entity;

namespace SupportManager.DAL
{
    public class SupportManagerContext : DbContext
    {
        private DbContextTransaction _currentTransaction;

        public SupportManagerContext() : this(nameof(SupportManagerContext))
        {
        }

        public SupportManagerContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SupportManagerContext, Migrations.Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(GetType().Assembly);
            modelBuilder.Entity<TeamMember>().HasRequired(m => m.User).WithMany();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ForwardingState> ForwardingStates { get; set; }
        public DbSet<ScheduledForward> ScheduledForwards { get; set; }
        public DbSet<SupportTeam> Teams { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserEmailAddress> UserEmailAddresses { get; set; }
        public DbSet<UserPhoneNumber> UserPhoneNumbers { get; set; }

        public void BeginTransaction()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    return;
                }

                _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch (Exception)
            {
                // todo: log transaction exception
                throw;
            }
        }

        public void CloseTransaction()
        {
            CloseTransaction(exception: null);
        }

        public void CloseTransaction(Exception exception)
        {
            try
            {
                if (_currentTransaction != null && exception != null)
                {
                    // todo: log exception
                    _currentTransaction.Rollback();
                    return;
                }

                SaveChanges();

                if (_currentTransaction != null)
                {
                    _currentTransaction.Commit();
                }
            }
            catch (Exception)
            {
                // todo: log exception
                if (_currentTransaction != null && _currentTransaction.UnderlyingTransaction.Connection != null)
                {
                    _currentTransaction.Rollback();
                }

                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
