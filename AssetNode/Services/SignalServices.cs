using AssetNode.Interface;
using AssetNode.Models.Dtos;
using AssetNode.Models.Entities;
using AssetNode.Services.Sql;
using Microsoft.EntityFrameworkCore;

namespace AssetNode.Services
{
    public class SignalServices : ISignalInterface
    {
        private readonly AssetDbContext _db;

        public SignalServices(AssetDbContext db)
        { 
            _db = db;
        }

        public async Task<List<SignalNodeDto>> GetSignals()
        {
            var allSignals =await _db.Signals.ToListAsync();

            var DisplaySignals = allSignals.Select(x => new SignalNodeDto
            {
                SignalId = x.SignalId,
                SignalName = x.SignalName,
                ValueType = x.ValueType,
                Description = x.Description,
                AssetID = x.AssetID
            }).ToList();


            return DisplaySignals;
        }

        public async Task<Signal> AddSignal(AddSignalDto dto)
        {
            var Newsignal = new Signal
            {
                SignalName = dto.SignalName,
                ValueType = dto.ValueType,
                Description = dto.Description,
                AssetID = dto.AssetId
            };
             _db.Signals.Add(Newsignal);
             await _db.SaveChangesAsync();

            return Newsignal;
        }


        public async Task<Signal> UpdateSignalAsync(int id, AddSignalDto dto)
        {
            var signal = await _db.Signals.FindAsync(id);
            if (signal == null)
                return null;

            // Directly update
            signal.SignalName = dto.SignalName;
            signal.ValueType = dto.ValueType;
            signal.Description = dto.Description;
            signal.AssetID = dto.AssetId;

            await _db.SaveChangesAsync();

            return signal; // Return the updated entity directly
        }


    }
}
