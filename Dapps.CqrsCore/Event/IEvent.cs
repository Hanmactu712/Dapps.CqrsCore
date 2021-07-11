using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Event
{
    public interface IEvent
    {
        Guid Id { get; set; }
        int Version { get; set; }
        Guid UserId { get; set; }
        //Guid TenantID { get; set; }
        DateTimeOffset Time { get; set; }
    }
}
