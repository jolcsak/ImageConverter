using ImageConverter.Domain.Dto;

namespace ImageConverter.Domain
{
    public interface ISumStorageHandler
    {
        SumStorage ReadSumStorage();
        void WriteSumStorage(SumStorage sumStorage);
    }
}