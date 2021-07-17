using System;

namespace Dapps.CqrsCore.Persistence.Read
{
    /// <summary>
    /// Base class for data entity
    /// </summary>
    public class BaseEntity
    {
        public int Version { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
