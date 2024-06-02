namespace ImageConverter.Domain.Dto
{
    public class LogMessage
    {
        public string? Type { get; set; }
        public string? TimeStamp { get; set; }
        public string? Message { get; set; }

        public LogMessage(string? timeStamp, string? message, string? logLevel)
        {
            TimeStamp = timeStamp;
            Message = message;
            Type = GetType(logLevel);
        }

        public static string? GetType(string? logLevel) => 
            (logLevel??string.Empty).ToUpper(System.Globalization.CultureInfo.InvariantCulture) switch
            {
                "INFORMATION" => "INF",
                "ERROR" => "ERR",
                "WARNING" => "WARN",
                "DEBUG" => "DBG",
                _ => "LOG",
            };
    }
}