using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class OperatorSSO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int OperatorId { get; set; }
        [StringLength(50)]
        public string AuthProvider { get; set; }
        [StringLength(4096)]
        public string Parameters { get; set; }
        public bool IsEnabled { get; set; } 
    }
}
