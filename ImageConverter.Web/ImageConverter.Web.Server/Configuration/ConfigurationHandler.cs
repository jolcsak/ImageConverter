using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Options;

namespace ImageConverter.Configuration
{
    public class ConfigurationHandler : IConfigurationHandler
    {
        private const double DEFAULT_NEW_SIZE_RATIO = 0.8;

        private readonly ILogger<ConfigurationHandler> logger;
        private readonly ImageConverterConfiguration imageConverterConfiguration;

        public ConfigurationHandler(
            ILogger<ConfigurationHandler> logger,
            IOptions<ImageConverterConfiguration> imageConverterConfigurationOptions)
        {
            this.logger = logger;
            imageConverterConfiguration = imageConverterConfigurationOptions.Value;
        }

        public ImageConverterConfiguration GetConfiguration()
        {
            if (string.IsNullOrEmpty(imageConverterConfiguration.Starts))
            {
                throw new Exception("Start expression not set!");
            }

            if (imageConverterConfiguration.ImageDirectories != null && imageConverterConfiguration.ImageDirectories.Any())
            {
                imageConverterConfiguration.ImageDirectories = imageConverterConfiguration.ImageDirectories.Distinct().ToArray();

                foreach (var directory in imageConverterConfiguration.ImageDirectories)
                {
                    if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                    {
                        logger.LogError("'{directory}' not found!", imageConverterConfiguration.ImageDirectories);
                    }
                }
            }
            else
            {
                throw new Exception("ImageDirectories not set!");
            }

            if (string.IsNullOrEmpty(imageConverterConfiguration.StoragePath) || !Directory.Exists(imageConverterConfiguration.StoragePath))
            {
                logger.LogError("Storage path not set!");
            }
            else
            {
                logger.LogInformation("Storage path: {directoryName}", imageConverterConfiguration.StoragePath);
            }

            if (imageConverterConfiguration.SearchPattern == null || !imageConverterConfiguration.SearchPattern.Any())
            {
                imageConverterConfiguration.SearchPattern = ["jpg", "jpeg"];
                logger.LogWarning("Search pattern not found in config file. Using *.jpg, *.jpeg.");
            }

            if (imageConverterConfiguration.CleanPattern == null || !imageConverterConfiguration.CleanPattern.Any())
            {
                imageConverterConfiguration.SearchPattern = [];
            }

            if (!imageConverterConfiguration.DeleteOriginal.HasValue)
            {
                imageConverterConfiguration.DeleteOriginal = false;
            }

            var threadNumber = imageConverterConfiguration.ThreadNumber;

            if (!threadNumber.HasValue)
            {
                imageConverterConfiguration.ThreadNumber = Environment.ProcessorCount;
            }

            if (imageConverterConfiguration.NewSizeRatio == null)
            {
                imageConverterConfiguration.NewSizeRatio = DEFAULT_NEW_SIZE_RATIO;
            }

            return imageConverterConfiguration;
        }
    }
}
