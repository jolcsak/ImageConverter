using Quartz;

namespace ImageConverter.Domain.Dto
{
    public class ImageConverterJobRegistry
    {
        public JobKey? JobKey { get; set; }
        public ITrigger? Trigger { get; set; }
    }
}
