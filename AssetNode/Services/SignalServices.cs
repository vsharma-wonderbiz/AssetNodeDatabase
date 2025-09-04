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
            try
            {
              
                if (string.IsNullOrWhiteSpace(dto.SignalName))
                    throw new Exception("Signal Name cannot be empty.");

                if (string.IsNullOrWhiteSpace(dto.ValueType))
                    throw new Exception("Value Type cannot be empty.");

                
                var assetExists = await _db.Assets.AnyAsync(a => a.Id == dto.AssetId);
                if (!assetExists)
                    throw new Exception("Asset does not exist.");

                var exists = await _db.Signals
                    .AnyAsync(s => s.AssetID == dto.AssetId && s.SignalName == dto.SignalName);
                if (exists)
                    throw new Exception($"Signal '{dto.SignalName}' already exists for this asset.");

                
                var newSignal = new Signal
                {
                    SignalName = dto.SignalName.Trim(),
                    ValueType = dto.ValueType.Trim(),
                    Description = dto.Description?.Trim(),
                    AssetID = dto.AssetId
                };

                _db.Signals.Add(newSignal);
                await _db.SaveChangesAsync();

                return newSignal;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error while adding signal: {ex.Message}");

                
                throw new Exception($"Failed to add signal: {ex.Message}");
            }
        }



        public async Task<Signal> UpdateSignalAsync(int id, UpdateSignalDto dto)
        {
            try
            {
                if (dto == null)
                    throw new Exception("Request data is required.");

                if (string.IsNullOrWhiteSpace(dto.SignalName))
                    throw new Exception("Signal name cannot be empty.");

                if (string.IsNullOrWhiteSpace(dto.ValueType))
                    throw new Exception("ValueType cannot be empty.");

                var signal = await _db.Signals.FindAsync(id);
                if (signal == null)
                    throw new Exception($"Signal with ID {id} was not found.");

                int targetAssetId = dto.AssetId ?? signal.AssetID;
                var assetExists = await _db.Assets.AnyAsync(a => a.Id== targetAssetId);
                if (!assetExists)
                    throw new Exception($"Asset with ID {targetAssetId} was not found.");

                bool isExist = await _db.Signals
                    .AnyAsync(x => x.SignalName.ToLower() == dto.SignalName.ToLower() &&
                                   x.AssetID == targetAssetId &&
                                   x.SignalId != id);

                if (isExist)
                    throw new Exception($"A signal named '{dto.SignalName}' already exists for asset {targetAssetId}.");

                signal.SignalName = dto.SignalName.Trim();
                signal.ValueType = dto.ValueType.Trim();
                signal.Description = string.IsNullOrWhiteSpace(dto.Description) ? signal.Description : dto.Description.Trim();
                signal.AssetID = targetAssetId;

                await _db.SaveChangesAsync();
                return signal;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating signal: " + ex.Message);
            }
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
