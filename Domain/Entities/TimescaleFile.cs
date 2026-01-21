namespace Domain.Entities
{
    public class TimescaleFile : IEntity<Guid>
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public virtual List<TimescaleValue>? Values { get; set; }
        public virtual List<TimescaleResult>? Results { get; set; }
    }
}
