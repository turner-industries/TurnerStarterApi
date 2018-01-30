using System;

namespace TurnerStarterApi.Core.Data
{
    public interface IEntity
    {
        int Id { get; set; }

        int? CreatedByUserId { get; set; }

        int? ModifiedByUserId { get; set; }

        int? DeletedByUserId { get; set; }

        bool IsDeleted { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime ModifiedDate { get; set; }

        DateTime? DeletedDate { get; set; }
    }
}
