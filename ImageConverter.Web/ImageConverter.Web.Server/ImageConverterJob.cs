using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using Quartz;

namespace ImageConverter.Web.Server
{
    [DisallowConcurrentExecution]
    public class ImageConverterJob : IJob
    {
        private readonly IImageConverter imageConverter;
        private readonly IFileCleaner fileCleaner;
        private readonly ILogger<ImageConverterJob> logger;
        private readonly IConfigurationHandler configurationHandler;
        private readonly ImageConverterContext imageConverterContext;

        public ImageConverterJob(
            IImageConverter imageConverter, 
            IFileCleaner fileCleaner,
            ILogger<ImageConverterJob> logger,
            IConfigurationHandler configurationHandler, 
            ImageConverterContext imageConverterContext)
        {
            this.imageConverter = imageConverter;
            this.fileCleaner = fileCleaner;
            this.logger = logger;
            this.configurationHandler = configurationHandler;
            this.imageConverterContext = imageConverterContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var configuration = configurationHandler.GetConfiguration();

            logger.LogInformation("Conversion started, using {threadCount} threads.", configuration.ThreadNumber!.Value);

            imageConverterContext.OnJobStarted();

            try
            {
                foreach (string? imageDirectory in configuration.ImageDirectories!)
                {
                    logger.LogInformation("Working directory: {dir}", imageDirectory);
                    Parallel.ForEach(
                    Directory.GetFiles(imageDirectory!, "*", SearchOption.AllDirectories),
                        new ParallelOptions { MaxDegreeOfParallelism = configuration.ThreadNumber!.Value, CancellationToken = context.CancellationToken },
                        async file =>
                        {
                            try
                            {
                                FileInfo fileInfo = new FileInfo(file);
                                long fileLength = fileInfo.Length;

                                if (fileCleaner!.Clean(imageDirectory, fileInfo))
                                {
                                    if (!fileInfo.Exists)
                                    {
                                        imageConverterContext.OnFileDeleted(fileLength);
                                    }
                                }
                                else if (configuration.SearchPattern!.Any(f => file.EndsWith("." + f, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    if (!IsSkippable(configuration.SkipPostfix!, fileInfo))
                                    {
                                        long? outputFileSize = 
                                            await imageConverter!.ConvertImage(imageDirectory, file, configuration.Transformers, configuration.OutputFormat!.Value);
                                        imageConverterContext.OnImageConverted(fileInfo.Length, outputFileSize.Value);
                                        File.Delete(file);
                                    }
                                    else
                                    {
                                        imageConverterContext.OnImageIgnored();
                                        logger.LogWarning($"{file}: skipped by skip postfix settings('{configuration.SkipPostfix!}').");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError("There is an error during the image conversion: " + ex.Message);
                                imageConverterContext.OnImageConvertFailed();
                            }
                        }
                    );
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning($"{nameof(ImageConverterJob)} job cancelled!");
            }

            imageConverterContext.OnJobFinished(context);
        }

        private static bool IsSkippable(string? skipPostfix, FileInfo fileInfo)
        {
            return skipPostfix != null && (Path.GetFileNameWithoutExtension(fileInfo.Name).EndsWith(skipPostfix) || (fileInfo.DirectoryName ?? string.Empty).EndsWith(skipPostfix));
        }
    }
}
