using ImageConverter.Domain.Dto;

namespace ImageConverter.Domain
{
    public interface IConfigurationHandler
    {
        ImageConverterConfiguration GetConfiguration();
    }
}