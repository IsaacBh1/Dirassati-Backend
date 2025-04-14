using System;

namespace Dirassati_Backend.Hubs.Services;

public interface IParentNotificationServices
{
    Task<List<Guid>> GetSchoolIdsByParentIdAsync(Guid parentId);

}
