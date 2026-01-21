using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Модель данных csv файла.
    /// </summary>
    public class TimescaleValue : IEntity<long>
    {
        public long Id { get; set; }

        /// <summary>
        /// Время начала.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Время выполнения в секундах.
        /// </summary>
        public float ExecutionTime { get; set; }

        /// <summary>
        /// Показатель в виде числа с плавающей запятой.
        /// </summary>
        public float Value { get; set; }

        public Guid FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual TimescaleFile? File { get; set; }
    }
}
