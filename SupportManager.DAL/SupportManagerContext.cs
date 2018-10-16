using System;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;

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
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SupportManagerContext, Migrations.Configuration>(true));
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
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        public void BeginTransaction()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();

                _currentTransaction?.Commit();
            }
            catch
            {
                RollbackTransaction();
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

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
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
