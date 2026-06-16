using Microsoft.EntityFrameworkCore;
using TubeMailGorillaDomain.Interfaces;
using TubeMailGorillaInfrastructure.Data.Repositories;

namespace TubeMailGorillaInfrastructure.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly InfrastructureDbContext _context;
        private bool _disposed;

        public IEmailerRepository Emailers { get; }
        public IBlockerRepository Blockers { get; }
        public ICaptionsRepository Captions { get; }
        public IIcebreakerRepository Icebreakers { get; }
        public ICredientalsRepository Credientals { get; }
        public IInboxersRepository Inboxers { get; }
        public ISettingsRepository Settings { get; }
        public ITemplatesRepository Templates { get; }
        public ICommentorRepository Commentors { get; }
        public IProxiesRepository Proxies { get; }

        public UnitOfWork(InfrastructureDbContext context)
        {
            _context = context;
            Emailers = new EmailerRepository(_context);
            Blockers = new BlockerRepository(_context);
            Captions = new CaptionsRepository(_context);
            Icebreakers = new IcebreakerRepository(_context);
            Credientals = new CredientalsRepository(_context);
            Inboxers = new InboxersRepository(_context);
            Settings = new SettingsRepository(_context);
            Templates = new TemplatesRepository(_context);
            Commentors = new CommentorRepository(_context);
            Proxies = new ProxiesRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}

