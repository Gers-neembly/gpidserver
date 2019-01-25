using System;
using System.ComponentModel.DataAnnotations;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class EmailQueue
    {
        [Key]
        public Guid Id { get; set; }
        public string OperatorId { get; set; }
        [MaxLength(100)]
        public string Subject { get; set; }
        public bool IsHtml { get; set; }
        public string Message { get; set; }
        [MaxLength(2000)]
        public string Receipients { get; set; }
        [MaxLength(2000)]
        public string CarbonCopies { get; set; }
        [MaxLength(2000)]
        public string BlindCarbonCopies { get; set; }
        [MaxLength(256)]
        public string Sender { get; set; }
        [MaxLength(256)]
        public string Status { get; set; }
        [MaxLength(1000)]
        public string FailedMessage { get; set; }
    }
}
