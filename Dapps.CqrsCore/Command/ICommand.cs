using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Command
{
    public interface ICommand
    {
        Guid Id { get; set; }
        int? Version { get; set; }
        Guid CommandId { get; set; }
        Guid UserId { get; set; }

        //Guid TenantID { get; set; }
    }
}
