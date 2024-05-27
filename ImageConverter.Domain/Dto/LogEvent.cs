using LiteDB;
namespace ImageConverter.Domain.Dto
{
    public class LogEvent
    {
        public DateTime Date { get; set; }
        public string? Message { get; set; }
        public string? Level { get; set; }
    }
}
