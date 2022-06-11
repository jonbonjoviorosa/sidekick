using Sidekick.Api.DataAccessLayer.Interfaces;
using System;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly APIDBContext context;

        public UnitOfWork(APIDBContext context)
        {
            this.context = context;
        }

        public void BeginTransaction()
        {
            context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            context.Database.CurrentTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            context.Database.CurrentTransaction.Rollback();
        }

        public void DisposeTransaction()
        {
            context.Database.CurrentTransaction.Dispose();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
