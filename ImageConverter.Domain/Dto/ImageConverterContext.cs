namespace ImageConverter.Domain.Dto
{
    public class ImageConverterContext
    {
        public long SumInputSize { get; set; } = 0;
        public long SumOutputSize { get; set; } = 0;
        public SumStorage Sum { get; private set; }

        private readonly ISumStorageHandler sumStorageHandler;

        public ImageConverterContext(ISumStorageHandler sumStorageHandler)
        {
            this.sumStorageHandler = sumStorageHandler;
            Sum = this.sumStorageHandler.ReadSumStorage();
        }

        public void Save()
        {
            this.sumStorageHandler.WriteSumStorage(Sum);
        }
    }
}
