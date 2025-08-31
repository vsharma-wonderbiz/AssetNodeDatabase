using AssetNode.Models.Dtos;
using AssetNode.Models.Entities;

namespace AssetNode.Interface
{
    public interface ISignalInterface
    {
        public Task<List<SignalNodeDto>> GetSignals();
        public Task<Signal> AddSignal(AddSignalDto Dto);

        public Task<Signal> UpdateSignalAsync(int id, UpdateSignalDto dto);

        public Task<string> DeleteSignal(int id);
    }
}
