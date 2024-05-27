using ImageConverter.Domain.Dto;

namespace ImageConverter.Domain
{
    public interface ISumStorageHandler
    {
        Task<SumStorage> ReadSumStorage();
        Task WriteSumStorage(SumStorage sumStorage);
    }
}