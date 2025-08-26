using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Entities
{
    public class FaucetListCurrency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? HostName { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created {  get; set; }
    }
}
