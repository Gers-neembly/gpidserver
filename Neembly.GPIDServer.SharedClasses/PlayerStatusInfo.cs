using System;
using System.Collections.Generic;
using System.Text;

namespace Neembly.GPIDServer.SharedClasses
{
    public class PlayerStatusInfo
    {
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public string Status { get; set; }
        public string ModifiedBy { get; set; }
    }
}
