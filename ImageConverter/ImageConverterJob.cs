using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Logging;
using NeoSmart.PrettySize;
using Quartz;
using System.Diagnostics;

namespace ImageConverter
{
    [DisallowConcurrentExecution]
    public class ImageConverterJob : IJob
    {
        private static object _countLock = new object();

        private readonly IImageConverter imageConverter;
        private readonly IFileCleaner fileCleaner;
        private readonly ISumStorageHandler sumStorageHandler;
        private readonly ILogger<ImageConverterJob> logger;
        private readonly IConfigurationHandler configurationHandler;

        public ImageConverterJob(IImageConverter imageConverter, IFileCleaner fileCleaner, 
            ISumStorageHandler sumStorageHandler, ILogger<ImageConverterJob> logger, IConfigurationHandler configurationHandler)
        {
            this.imageConverter = imageConverter;
            this.fileCleaner = fileCleaner;
            this.sumStorageHandler = sumStorageHandler;
            this.logger = logger;
            this.configurationHandler = configurationHandler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var configuration = configurationHandler.GetConfiguration();

            logger.LogInformation("Conversion started, using {threadCount} threads.", configuration.ThreadNumber!.Value);

            long convertedCount = 0;
            long deletedCount = 0;
            long sumDeletedFileSize = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (string? imageDirectory in configuration.ImageDirectories!)
            {
                logger.LogInformation("Working directory: {dir}", imageDirectory);
                Parallel.ForEach(
                Directory.GetFiles(imageDirectory!, "*", SearchOption.AllDirectories),
                    new ParallelOptions { MaxDegreeOfParallelism = configuration.ThreadNumber!.Value },
                    async file =>
                    {
                        try
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            long fileLength = fileInfo.Length;

                            if (fileCleaner!.Clean(imageDirectory, fileInfo))
                            {
                                lock (_countLock)
                                {
                                    if (!fileInfo.Exists)
                                    {
                                        deletedCount++;
                                        sumDeletedFileSize += fileLength;
                                    }
                                }
                            }
                            else if (configuration.SearchPattern!.Any(f => file.EndsWith("." + f, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                if (!IsSkippable(configuration.SkipPostfix!, fileInfo))
                                {
                                    await imageConverter!.ConvertImage(imageDirectory, file, configuration.Transformers, configuration.OutputFormat!.Value);
                                    lock (_countLock)
                                    {
                                        convertedCount++;
                                    }
                                    if (configuration.DeleteOriginal!.Value)
                                    {
                                        File.Delete(file);
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
                        }
                    });
            }

            sw.Stop();

            var prettyInputSumSize = PrettySize.Bytes(imageConverter!.SumInputSize);
            var prettyOutputSumSize = PrettySize.Bytes(imageConverter!.SumOutputSize);
            var prettySavedSumSize = PrettySize.Bytes(imageConverter!.SumInputSize - imageConverter!.SumOutputSize);
            var prettySumDeletedFileSize = PrettySize.Bytes(sumDeletedFileSize);

            logger.LogInformation("Conversion done: {convertedCount} files converted, {inputSumSize} -> {outputSumSize}, saved: {savedSumSize}, cleaned #: {deletedCount}, cleaned size: {sumDeletedFileSize}, took {totalSeconds} seconds.",
                convertedCount, prettyInputSumSize.Format(UnitBase.Base10), prettyOutputSumSize.Format(UnitBase.Base10), prettySavedSumSize.Format(UnitBase.Base10), deletedCount, prettySumDeletedFileSize.Format(UnitBase.Base10), sw.Elapsed.TotalSeconds);

            await RefreshStatistics(imageConverter, sumStorageHandler, convertedCount, deletedCount, sumDeletedFileSize, logger);

            if (context.NextFireTimeUtc != null)
            {
                logger.LogInformation("Next fire time {nextFireTime}", context.NextFireTimeUtc!.Value.ToLocalTime());
            }
        }

        private async static Task RefreshStatistics(IImageConverter? imageConverter, ISumStorageHandler? sumStorageHandler,
            long convertedCount, long deletedFileCount, long sumDeleteFileSize, ILogger logger)
        {
            SumStorage sumStorage = await sumStorageHandler!.ReadSumStorage()!;
            sumStorage.ProcessedBytes += imageConverter!.SumInputSize;
            sumStorage.SumSavedBytes += imageConverter!.SumInputSize - imageConverter!.SumOutputSize;
            sumStorage.ConvertedImageCount += convertedCount;
            sumStorage.DeletedFileCount += deletedFileCount;
            sumStorage.SumDeleteFileSize += sumDeleteFileSize;

            var prettyProcessedBytes = PrettySize.Bytes(sumStorage.ProcessedBytes);
            var prettySumSavedBytes = PrettySize.Bytes(sumStorage.SumSavedBytes);
            var prettySumDeletedFileSize = PrettySize.Bytes(sumStorage.SumDeleteFileSize);

            logger.LogInformation("******************************************************************************************************");
            logger.LogInformation("Totally converted images: {convertedImageCount}, processed: {processedBytes}, saved: {sumSavedBytes}, cleaned #: {deletedFileCount}, cleaned: {sumDeletedFileSize}",
                sumStorage.ConvertedImageCount, prettyProcessedBytes.Format(UnitBase.Base10), prettySumSavedBytes.Format(UnitBase.Base10),
                sumStorage.DeletedFileCount, prettySumDeletedFileSize.Format(UnitBase.Base10));
            logger.LogInformation("******************************************************************************************************");

            if (convertedCount > 0)
            {
                await sumStorageHandler!.WriteSumStorage(sumStorage);
            }
        }

        private static bool IsSkippable(string? skipPostfix, FileInfo fileInfo)
        {
            return skipPostfix != null && (Path.GetFileNameWithoutExtension(fileInfo.Name).EndsWith(skipPostfix) || (fileInfo.DirectoryName ?? string.Empty).EndsWith(skipPostfix));
        }
    }
}
