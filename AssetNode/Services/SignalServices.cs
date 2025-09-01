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


        public async Task<Signal> UpdateSignalAsync(int id, UpdateSignalDto dto)    
        {
            
            var signal = await _db.Signals.FindAsync(id);
            if (signal == null)
                return null;

            bool isExist = _db.Signals.Any(x =>
        x.SignalName == dto.SignalName &&
        x.AssetID == (dto.AssetId ?? signal.AssetID) &&  
        x.SignalId != id);

            if (isExist)
            {
                throw new InvalidOperationException("Signal already exists in this asset.");
            }




            if (!string.IsNullOrWhiteSpace(dto.SignalName) &&
                dto.SignalName != "string" &&
                dto.SignalName != signal.SignalName)
            {
                signal.SignalName = dto.SignalName;
            }

            if (!string.IsNullOrWhiteSpace(dto.ValueType) &&
                dto.ValueType != "string" &&
                dto.ValueType != signal.ValueType)
            {
                signal.ValueType = dto.ValueType;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description) &&
                dto.Description != "string" &&
                dto.Description != signal.Description)
            {
                signal.Description = dto.Description;
            }

          
            if (dto.AssetId.HasValue &&
                dto.AssetId.Value > 0 &&
                dto.AssetId.Value != signal.AssetID)
            {
                signal.AssetID = dto.AssetId.Value;
            }


            await _db.SaveChangesAsync();
            return signal;
        }

        public async Task<string> DeleteSignal(int id)
        {
            if (id <= 0)
            {
                return "Invalid signal ID";
            }

            try
            {
                var node = await _db.Signals.FindAsync(id);
                if (node == null)
                {
                    return "Signal not found";
                }

                _db.Signals.Remove(node);
                await _db.SaveChangesAsync();

                return "Signal deleted successfully";
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details (not shown here for brevity)
                return "Failed to delete signal due to database error";
            }
            catch (Exception ex)
            {
                // Log the exception details
                return "An unexpected error occurred while deleting the signal";
            }
        }

    }
}
