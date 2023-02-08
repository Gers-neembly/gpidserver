using System;
using System.ComponentModel;
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
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
        [DefaultValue(true)]
        public bool AutoRegister { get; set; } = true;
    }
}
