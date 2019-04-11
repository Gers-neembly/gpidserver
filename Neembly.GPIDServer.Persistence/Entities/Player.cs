using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string NetUserId { get; set; }
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePrefix { get; set; }
        public string MobileNo { get; set; }
    }
}
