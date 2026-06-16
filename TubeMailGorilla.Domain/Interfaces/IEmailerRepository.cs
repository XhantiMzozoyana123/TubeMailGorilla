using TubeMailGorillaDomain.Entities;

namespace TubeMailGorillaDomain.Interfaces
{
    public interface IEmailerRepository
    {
        //
        IQueryable<Emailer> GetAll();
        Task<List<Emailer>> GetAllAsync();
        Task<Emailer?> GetByIdAsync(int id);
        Task<Emailer?> GetByEmailAsync(string email);
        Task AddAsync(Emailer emailer);
        void Update(Emailer emailer);
        Task DeleteAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> SaveChangesAsync();
    }

    public interface IBlockerRepository
    {
        IQueryable<Blocker> GetAll();
        Task<List<Blocker>> GetAllAsync();
        Task<Blocker?> GetByEmailAsync(string email);
        Task AddAsync(Blocker blocker);
        void Delete(Blocker blocker);
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> SaveChangesAsync();
    }


    public interface ICaptionsRepository
    {
        IQueryable<Captions> GetAll();
        Task<List<Captions>> GetAllAsync();
        Task<Captions?> GetByEmailerIdAsync(int emailerId);
        Task AddAsync(Captions captions);
        Task<int> SaveChangesAsync();
    }

    public interface IIcebreakerRepository
    {
        IQueryable<Icebreaker> GetAll();
        Task<List<Icebreaker>> GetAllAsync();
        Task<Icebreaker?> GetByEmailerIdAsync(int emailerId);
        Task AddAsync(Icebreaker icebreaker);
        Task<int> SaveChangesAsync();
    }

    public interface ICredientalsRepository
    {
        IQueryable<Credientals> GetAll();
        Task<List<Credientals>> GetAllAsync();
        Task<Credientals?> GetByIdAsync(int id);
        Task<Credientals?> GetByEmailAsync(string email);
        Task AddAsync(Credientals credientals);
        void Update(Credientals credientals);
        void Delete(Credientals credientals);
        Task<int> SaveChangesAsync();
    }

    public interface IInboxersRepository
    {
        IQueryable<Inboxers> GetAll();
        Task<List<Inboxers>> GetAllAsync();
        Task AddAsync(Inboxers inboxer);
        Task<int> SaveChangesAsync();
    }

    public interface ISettingsRepository
    {
        IQueryable<Settings> GetAll();
        Task<Settings?> GetFirstAsync();
        Task<int> SaveChangesAsync();
    }

    public interface ITemplatesRepository
    {
        IQueryable<Templates> GetAll();
        Task<List<Templates>> GetAllAsync();
        Task<Templates?> GetByNameAsync(string name);
        Task AddAsync(Templates template);
        void Update(Templates template);
        void Delete(Templates template);
        Task<int> SaveChangesAsync();
    }

    public interface ICommentorRepository
    {
        IQueryable<Commentor> GetAll();
        Task<List<Commentor>> GetAllAsync();
        Task AddAsync(Commentor commentor);
        void Delete(Commentor commentor);
        Task<int> SaveChangesAsync();
    }

    public interface IProxiesRepository
    {
        IQueryable<Proxies> GetAll();
        Task<List<Proxies>> GetAllAsync();
        Task AddAsync(Proxies proxy);
        void Delete(Proxies proxy);
        Task<int> SaveChangesAsync();
    }

    public interface IUnitOfWork
    {
        IEmailerRepository Emailers { get; }
        IBlockerRepository Blockers { get; }
        ICaptionsRepository Captions { get; }
        IIcebreakerRepository Icebreakers { get; }
        ICredientalsRepository Credientals { get; }
        IInboxersRepository Inboxers { get; }
        ISettingsRepository Settings { get; }
        ITemplatesRepository Templates { get; }
        ICommentorRepository Commentors { get; }
        IProxiesRepository Proxies { get; }
        Task<int> CompleteAsync();
    }
}
