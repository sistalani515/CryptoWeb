using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Entities
{
    public class FaucetHost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? HostName { get; set; }
        public string? BaseURL { get; set; }
        public int Type { get; set; }
        public bool IsActive { get;set; }
        public int Delay { get; set; } = 1;
        public int MaxDayClaim { get; set; } = 50;
        public int MaxThread { get; set; } = 1;
        public bool IsHold { get; set; } 
        public DateTime? Changed {  get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
