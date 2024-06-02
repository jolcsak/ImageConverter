using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImageConverter.Web.Server
{
    public class FileCleaner : IFileCleaner
    {
        private readonly ImageConverterConfiguration configuration;
        private readonly ILogger<FileCleaner> logger;

        public FileCleaner(IOptions<ImageConverterConfiguration> configurationSettings, ILogger<FileCleaner> logger)
        {
            configuration = configurationSettings.Value;
            this.logger = logger;
        }

        public bool Clean(string? imageDirectory, FileInfo file)
        {

            if (configuration.CleanPattern!.Any(cp => file.Name.EndsWith("." + cp, StringComparison.InvariantCultureIgnoreCase)))
            {
                string relativeFilePath = Path.GetRelativePath(imageDirectory!, file.FullName);
                try
                {
                    file.Delete();
                    logger.LogWarning("{relativeFilePath} deleted.", relativeFilePath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during {relativeFilePath} deletion.", relativeFilePath);
                }
                return true;
            }
            return false;
        }
    }
}
