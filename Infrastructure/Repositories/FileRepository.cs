using Domain.Entities;
using Domain.RepositoryAbstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FileRepository : Repository<TimescaleFile, Guid>, IFileRepository
    {
        public FileRepository(DatabaseContext context) : base(context)
        {
        }
        public override async Task<TimescaleFile> AddAsync(TimescaleFile timescaleFile)
        {
            return await base.AddAsync(timescaleFile);
        }
    }
}
