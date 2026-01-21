namespace Domain.RepositoryAbstractions
{
    public interface IUnitOfWork
    {
        IFileRepository UploadedFiles { get; }
        IValueRepository UploadedValues { get; }
        IResultRepository UploadedResults { get; }
        void DetachAllEntities();
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
