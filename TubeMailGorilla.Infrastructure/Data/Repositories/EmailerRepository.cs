using Microsoft.EntityFrameworkCore;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorillaInfrastructure.Data.Repositories
{
    public class EmailerRepository : IEmailerRepository
    {
        private readonly InfrastructureDbContext _context;

        public EmailerRepository(InfrastructureDbContext context)
        {
            _context = context;
        }

        public IQueryable<Emailer> GetAll() => _context.Emailers.AsQueryable();

        public async Task<List<Emailer>> GetAllAsync() => await _context.Emailers.ToListAsync();

        public async Task<Emailer?> GetByIdAsync(int id) => await _context.Emailers.FindAsync(id);

        public async Task<Emailer?> GetByEmailAsync(string email) =>
            await _context.Emailers.FirstOrDefaultAsync(e => e.Email == email);

        public async Task AddAsync(Emailer emailer) => await _context.Emailers.AddAsync(emailer);

        public void Update(Emailer emailer) => _context.Emailers.Update(emailer);

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Emailers.FindAsync(id);
            if (entity != null)
                _context.Emailers.Remove(entity);
        }

        public async Task<bool> ExistsByEmailAsync(string email) =>
            await _context.Emailers.AnyAsync(e => e.Email == email);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class BlockerRepository : IBlockerRepository
    {
        private readonly InfrastructureDbContext _context;

        public BlockerRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Blocker> GetAll() => _context.Blockers.AsQueryable();

        public async Task<List<Blocker>> GetAllAsync() => await _context.Blockers.ToListAsync();

        public async Task<Blocker?> GetByEmailAsync(string email) =>
            await _context.Blockers.FirstOrDefaultAsync(b => b.Email == email);

        public async Task AddAsync(Blocker blocker) => await _context.Blockers.AddAsync(blocker);

        public void Delete(Blocker blocker) => _context.Blockers.Remove(blocker);

        public async Task<bool> ExistsByEmailAsync(string email) =>
            await _context.Blockers.AnyAsync(b => b.Email == email);


        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class CaptionsRepository : ICaptionsRepository
    {
        private readonly InfrastructureDbContext _context;

        public CaptionsRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Captions> GetAll() => _context.Captions.AsQueryable();

        public async Task<List<Captions>> GetAllAsync() => await _context.Captions.ToListAsync();

        public async Task<Captions?> GetByEmailerIdAsync(int emailerId) =>
            await _context.Captions.FirstOrDefaultAsync(c => c.EmailerId == emailerId);

        public async Task AddAsync(Captions captions) => await _context.Captions.AddAsync(captions);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class IcebreakerRepository : IIcebreakerRepository
    {
        private readonly InfrastructureDbContext _context;

        public IcebreakerRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Icebreaker> GetAll() => _context.Icebreakers.AsQueryable();

        public async Task<List<Icebreaker>> GetAllAsync() => await _context.Icebreakers.ToListAsync();

        public async Task<Icebreaker?> GetByEmailerIdAsync(int emailerId) =>
            await _context.Icebreakers.FirstOrDefaultAsync(i => i.EmailerId == emailerId);

        public async Task AddAsync(Icebreaker icebreaker) => await _context.Icebreakers.AddAsync(icebreaker);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class CredientalsRepository : ICredientalsRepository
    {
        private readonly InfrastructureDbContext _context;

        public CredientalsRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Credientals> GetAll() => _context.Credientals.AsQueryable();

        public async Task<List<Credientals>> GetAllAsync() => await _context.Credientals.ToListAsync();

        public async Task<Credientals?> GetByIdAsync(int id) => await _context.Credientals.FindAsync(id);

        public async Task<Credientals?> GetByEmailAsync(string email) =>
            await _context.Credientals.FirstOrDefaultAsync(c => c.Email == email);

        public async Task AddAsync(Credientals credientals) => await _context.Credientals.AddAsync(credientals);

        public void Update(Credientals credientals) => _context.Credientals.Update(credientals);

        public void Delete(Credientals credientals) => _context.Credientals.Remove(credientals);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class InboxersRepository : IInboxersRepository
    {
        private readonly InfrastructureDbContext _context;

        public InboxersRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Inboxers> GetAll() => _context.Inboxers.AsQueryable();

        public async Task<List<Inboxers>> GetAllAsync() => await _context.Inboxers.ToListAsync();

        public async Task AddAsync(Inboxers inboxer) => await _context.Inboxers.AddAsync(inboxer);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class SettingsRepository : ISettingsRepository
    {
        private readonly InfrastructureDbContext _context;

        public SettingsRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Settings> GetAll() => _context.Settings.AsQueryable();

        public async Task<Settings?> GetFirstAsync() => await _context.Settings.FirstOrDefaultAsync();

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class TemplatesRepository : ITemplatesRepository
    {
        private readonly InfrastructureDbContext _context;

        public TemplatesRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Templates> GetAll() => _context.Templates.AsQueryable();

        public async Task<List<Templates>> GetAllAsync() => await _context.Templates.ToListAsync();

        public async Task<Templates?> GetByNameAsync(string name) =>
            await _context.Templates.FirstOrDefaultAsync(t => t.Name == name);

        public async Task AddAsync(Templates template) => await _context.Templates.AddAsync(template);

        public void Update(Templates template) => _context.Templates.Update(template);

        public void Delete(Templates template) => _context.Templates.Remove(template);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class CommentorRepository : ICommentorRepository
    {
        private readonly InfrastructureDbContext _context;

        public CommentorRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Commentor> GetAll() => _context.Commentors.AsQueryable();

        public async Task<List<Commentor>> GetAllAsync() => await _context.Commentors.ToListAsync();

        public async Task AddAsync(Commentor commentor) => await _context.Commentors.AddAsync(commentor);

        public void Delete(Commentor commentor) => _context.Commentors.Remove(commentor);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }

    public class ProxiesRepository : IProxiesRepository
    {
        private readonly InfrastructureDbContext _context;

        public ProxiesRepository(InfrastructureDbContext context) => _context = context;

        public IQueryable<Proxies> GetAll() => _context.Proxies.AsQueryable();

        public async Task<List<Proxies>> GetAllAsync() => await _context.Proxies.ToListAsync();

        public async Task AddAsync(Proxies proxy) => await _context.Proxies.AddAsync(proxy);

        public void Delete(Proxies proxy) => _context.Proxies.Remove(proxy);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

