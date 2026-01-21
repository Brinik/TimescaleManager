using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TimescaleResult : IEntity<long>
    {
        public long Id {  get; set; }

        /// <summary>
        ///  дельта времени Date в секундах (максимальное Date – минимальное Date)
        /// </summary>
        public double DateDelta { get; set; }

        /// <summary>
        /// минимальное дата и время, как момент запуска первой операции (Date)
        /// </summary>
        public DateTime MinDate { get; set; }

        /// <summary>
        /// среднее время выполнения (ExecutionTime)
        /// </summary>
        public double AvgExecutionTime { get; set; }

        /// <summary>
        /// среднее значение по показателям (Value)
        /// </summary>
        public double AvgValue { get; set; }

        /// <summary>
        /// медиана по показателям (Value)
        /// </summary>
        public double MedianValue { get; set; }

        /// <summary>
        ///  максимальное значение показателя (Value)
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// минимальное значение показателя (Value)
        /// </summary>
        public double MinValue { get; set; }

        public Guid FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual TimescaleFile? File { get; set; }
    }
}
