using ImageMagick;

namespace ImageConverter.Domain.Dto
{
    public class ImageConverterConfiguration
    {
        public string? Starts { get; set; }
        public string[]? ImageDirectories { get; set; }
        public string[]? SearchPattern { get; set; }
        public string[]? CleanPattern { get; set; }
        public string? SkipPostfix { get; set; }
        public MagickFormat? OutputFormat { get; set; }
        public double? NewSizeRatio { get; set; }
        public string[]? Transformers { get; set; }
        public bool? DeleteOriginal { get; set; }
        public int? ThreadNumber { get; set; }

        public string? StoragePath { get; set; }
        public bool? RunAtStart { get ; set; }
    }
}
