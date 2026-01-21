namespace Domain.Entities
{
    public interface IEntity<TId>
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        TId Id { get; set; }
    }
}
