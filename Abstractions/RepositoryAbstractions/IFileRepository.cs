using Domain.Entities;

namespace Domain.RepositoryAbstractions
{
    public interface IFileRepository : IRepository<TimescaleFile, Guid>
    {
    }
}
