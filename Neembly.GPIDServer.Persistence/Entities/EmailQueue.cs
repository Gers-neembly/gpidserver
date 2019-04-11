using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class EmailQueue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int OperatorId { get; set; }
        public string Subject { get; set; }
        public bool IsHtml { get; set; }
        public string Message { get; set; }
        public string Receipients { get; set; }
        public string CarbonCopies { get; set; }
        public string BlindCarbonCopies { get; set; }
        public string Sender { get; set; }
        public string Status { get; set; }
        public string FailedMessage { get; set; }
    }
}
