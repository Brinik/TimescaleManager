namespace TimescaleManager.DTO
{
    public class TimescaleResultDTO
    {
        public double DateDelta { get; set; }
        public DateTime MinDate { get; set; }
        public float AvgExecutionTime { get; set; }
        public float AvgValue { get; set; }
        public float MedianValue { get; set; }
        public float MaxValue { get; set; }
        public float MinValue { get; set; }
    }
}
