using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using NeoSmart.PrettySize;
using Quartz;
using System.Diagnostics;

namespace ImageConverter.Web.Server
{
    [DisallowConcurrentExecution]
    public class ImageConverterJob : IJob
    {
        private static object countLock = new object();

        private readonly IImageConverter imageConverter;
        private readonly IFileCleaner fileCleaner;
        private readonly ILogger<ImageConverterJob> logger;
        private readonly IConfigurationHandler configurationHandler;
        private readonly ImageConverterContext imageConverterContext;
        private readonly ImageConverterJobRegistry imageConverterJobRegistry;

        public ImageConverterJob(
            IImageConverter imageConverter, 
            IFileCleaner fileCleaner,
            ILogger<ImageConverterJob> logger,
            IConfigurationHandler configurationHandler, 
            ImageConverterContext imageConverterContext,
            ImageConverterJobRegistry imageConverterJobRegistry)
        {
            this.imageConverter = imageConverter;
            this.fileCleaner = fileCleaner;
            this.logger = logger;
            this.configurationHandler = configurationHandler;
            this.imageConverterContext = imageConverterContext;
            this.imageConverterJobRegistry = imageConverterJobRegistry;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var configuration = configurationHandler.GetConfiguration();

            logger.LogInformation("Conversion started, using {threadCount} threads.", configuration.ThreadNumber!.Value);

            long convertedCount = 0;
            long deletedCount = 0;
            long sumDeletedFileSize = 0;

            imageConverterContext.Sum.State = ImageConverterStates.Running.ToString();
            imageConverterContext.Sum.LastStarted = DateTime.Now.Ticks;
            imageConverterContext.Save();

            Stopwatch sw = new Stopwatch();
            sw.Start();

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
                                    lock (countLock)
                                    {
                                        if (!fileInfo.Exists)
                                        {
                                            deletedCount++;
                                            sumDeletedFileSize += fileLength;
                                            imageConverterContext.Sum.DeletedFileCount++;
                                            imageConverterContext.Save();
                                        }
                                    }
                                }
                                else if (configuration.SearchPattern!.Any(f => file.EndsWith("." + f, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    if (!IsSkippable(configuration.SkipPostfix!, fileInfo))
                                    {
                                        await imageConverter!.ConvertImage(imageDirectory, file, configuration.Transformers, configuration.OutputFormat!.Value);
                                        lock (countLock)
                                        {
                                            imageConverterContext.Sum.ConvertedImageCount++;
                                            convertedCount++;
                                            if (configuration.DeleteOriginal!.Value)
                                            {
                                                File.Delete(file);
                                            }
                                            imageConverterContext.Save();
                                        }
                                    }
                                    else
                                    {
                                        logger.LogWarning($"{file}: skipped by skip postfix settings('{configuration.SkipPostfix!}').");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError("There is an error during the image conversion: " + ex.Message);
                                lock (countLock)
                                {
                                    imageConverterContext.Sum.ErrorCount++;
                                    imageConverterContext.Save();
                                }
                            }
                        }
                    );
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning($"{nameof(ImageConverterJob)} job cancelled!");
            }

            sw.Stop();

            var prettyInputSumSize = PrettySize.Bytes(imageConverterContext.SumInputSize);
            var prettyOutputSumSize = PrettySize.Bytes(imageConverterContext.SumOutputSize);
            var prettySavedSumSize = PrettySize.Bytes(imageConverterContext.SumInputSize - imageConverterContext.SumOutputSize);
            var prettySumDeletedFileSize = PrettySize.Bytes(sumDeletedFileSize);

            logger.LogInformation("Conversion done: {convertedCount} files converted, {inputSumSize} -> {outputSumSize}, saved: {savedSumSize}, cleaned #: {deletedCount}, cleaned size: {sumDeletedFileSize}, took {totalSeconds} seconds.",
                convertedCount, prettyInputSumSize.Format(UnitBase.Base10), prettyOutputSumSize.Format(UnitBase.Base10), prettySavedSumSize.Format(UnitBase.Base10), deletedCount, prettySumDeletedFileSize.Format(UnitBase.Base10), sw.Elapsed.TotalSeconds);

            LogStatistics(imageConverterContext);

            DateTimeOffset? nextFireTimeUtc = imageConverterJobRegistry.Trigger!.GetNextFireTimeUtc();

            if (nextFireTimeUtc != null)
            {
                DateTimeOffset nextFireTime = nextFireTimeUtc.Value.ToLocalTime();
                logger.LogInformation("Next fire time {nextFireTime}", nextFireTime);
                imageConverterContext.Sum.NextFire = nextFireTime.Ticks;
            }
            else
            {
                imageConverterContext.Sum.NextFire = 0;
            }

            imageConverterContext.Sum.State = context.CancellationToken.IsCancellationRequested ?
                ImageConverterStates.Cancelled.ToString() :
                ImageConverterStates.Finished.ToString();

            imageConverterContext.Sum.LastFinished = DateTime.Now.Ticks;
            imageConverterContext.Save();
        }

        private void LogStatistics(ImageConverterContext context)
        {
            SumStorage sumStorage = context.Sum;

            var prettyProcessedBytes = PrettySize.Bytes(sumStorage.ProcessedBytes);
            var prettySumSavedBytes = PrettySize.Bytes(sumStorage.SumSavedBytes);
            var prettySumDeletedFileSize = PrettySize.Bytes(sumStorage.SumDeleteFileSize);

            logger.LogInformation("******************************************************************************************************");
            logger.LogInformation("Totally converted images: {convertedImageCount}, processed: {processedBytes}, saved: {sumSavedBytes}, cleaned #: {deletedFileCount}, cleaned: {sumDeletedFileSize}",
                sumStorage.ConvertedImageCount, prettyProcessedBytes.Format(UnitBase.Base10), prettySumSavedBytes.Format(UnitBase.Base10),
                sumStorage.DeletedFileCount, prettySumDeletedFileSize.Format(UnitBase.Base10));
            logger.LogInformation("******************************************************************************************************");
        }

        private static bool IsSkippable(string? skipPostfix, FileInfo fileInfo)
        {
            return skipPostfix != null && (Path.GetFileNameWithoutExtension(fileInfo.Name).EndsWith(skipPostfix) || (fileInfo.DirectoryName ?? string.Empty).EndsWith(skipPostfix));
        }
    }
}
