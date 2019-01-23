using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Models.DTO
{
    public class ResultsInfo
    {
        public bool Success { get; set; }
        public object DataInfo { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
