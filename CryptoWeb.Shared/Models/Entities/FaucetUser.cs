using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Entities
{
    public class FaucetUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? Email { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? Host { get; set; }
        public string? Password { get; set; }
        public string? Session {  get; set; }
        public int TodayClaim {  get; set; }
        public int TotalClaim {  get; set; }
        public DateTime? LastClaim {  get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; } 
        public bool IsDeleted { get; set; }
        public bool IsSL { get; set; }
        public string? IpAddress { get; set; }
        public string? UA { get; set; }
        public DateTime? SLCompleted {  get; set; }
        public DateTime Created {  get; set; } = DateTime.UtcNow;
    }
}
