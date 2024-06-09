using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageMagick;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoSmart.PrettySize;

namespace ImageConverter
{
    public class ImageConverter : IImageConverter
    {
        private readonly Dictionary<MagickFormat, IConversionInRule> conversionInRules;
        private readonly Dictionary<MagickFormat, IConversionOutRule> conversionOutRules;

        private readonly Dictionary<string, IImageTransformer> transformers;
        private readonly ImageConverterConfiguration configuration;
        private readonly ILogger<ImageConverter> logger;
        private readonly ImageConverterContext imageConverterContext;

        public ImageConverter(
            IEnumerable<IConversionInRule> conversionInRules, 
            IEnumerable<IConversionOutRule> conversionOutRules,
            IEnumerable<IImageTransformer> transformers,
            IOptions<ImageConverterConfiguration> configurationSettings,
            ILogger<ImageConverter> logger,
            ImageConverterContext imageConverterContext)
        {
            this.conversionInRules = conversionInRules.ToDictionary(cir => cir.ImageFormat, cir => cir);
            this.conversionOutRules = conversionOutRules.ToDictionary(cir => cir.ImageFormat, cir => cir);
            this.transformers = transformers.ToDictionary(tr => tr.Key, tr => tr);
            this.logger = logger;
            configuration = configurationSettings.Value;
            this.imageConverterContext = imageConverterContext; 
        }

        public async Task<long?> ConvertImage(string? imageDirectory, string imagePath, string[]? transformerKeys, MagickFormat outputFormat)
        {
            try
            {
                string outputExtension = outputFormat.ToString();
                var imageInfo = new MagickImageInfo(imagePath);

                var inputFileInfo = new FileInfo(imagePath);
                long inputFileSize = inputFileInfo.Length;
                var prettyInputFileSize = PrettySize.Bytes(inputFileSize);

                string relativeInputFilePath = Path.GetRelativePath(imageDirectory!, inputFileInfo.FullName);

                string outputFileName = GetOutputFileNameWithPath(inputFileInfo, outputExtension);

                using (var image = new MagickImage(imagePath))
                {
                    if (conversionInRules.TryGetValue(image.Format, out var inRule))
                    {
                        if (!inRule.SetImage(image))
                        {
                            logger.LogWarning("Skipping image because of in rule '{imageFormat}'.", inRule.ImageFormat);
                            return null;
                        }
                    }                        

                    image.Format = outputFormat;

                    foreach (string transformerKey in transformerKeys ?? Array.Empty<string>())
                    {
                        if (transformers.TryGetValue(transformerKey, out var transformer))
                        {
                            transformer.TransformImage(image);
                        }
                    }

                    if (conversionOutRules.TryGetValue(image.Format, out var outRule))
                    {
                        if (!outRule.SetImage(image))
                        {
                            logger.LogWarning("Skipping image because of out rule '{imageFormat}'.", outRule.ImageFormat);
                            return null;
                        }
                    }

                    bool imageProcessed = false;
                    do
                    {
                        using (var imageBytes = new MemoryStream())
                        {
                            await image.WriteAsync(imageBytes);
                            imageProcessed = imageBytes.Length < inputFileInfo.Length * configuration.NewSizeRatio;
                            if (imageProcessed)
                            {
                                await File.WriteAllBytesAsync(outputFileName, imageBytes.ToArray());
                            }
                            else
                            {
                                image.Quality -= 5;
                                logger.LogInformation(
                                    "{outputFileName}: output file size {outputFileSize} is too high. Recompressing. Q: {imageQuality}",
                                    outputFileName, PrettySize.Bytes(imageBytes.Length).Format(UnitBase.Base10), image.Quality);
                            }
                        }
                    } while (!imageProcessed);

                    long outputFileSize = new FileInfo(outputFileName).Length;

                    var prettyOutputFileSize = PrettySize.Bytes(outputFileSize);

                    logger.LogInformation(
                        "{relativeInputFilePath}: {compression},{width}x{height},Q:{quality},S:{inputSize} -> {outputFormat},{imageWidth}x{imageHeight}, Q: {imageQuality}, S:{outputFileSize}",
                        relativeInputFilePath, 
                        imageInfo.Compression, imageInfo.Width, imageInfo.Height, imageInfo.Quality, 
                        prettyInputFileSize.Format(UnitBase.Base10), 
                        outputFormat, image.Width, image.Height, image.Quality, prettyOutputFileSize.Format(UnitBase.Base10)
                        );

                    return outputFileSize;
                }
            }
            catch (MagickCorruptImageErrorException mciex)
            {
                logger.LogError(mciex.Message, "{imagePath}", imagePath);
            }
            finally
            {
                GC.Collect();
            }

            return null;
        }

        private static string GetOutputFileNameWithPath(FileInfo fileInfo, string extension)
        {
            string outputDirectory = fileInfo.Directory!.FullName;
            string outputFileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + "." + extension;
            return Path.Combine(outputDirectory, outputFileName);
        }
    }
}
