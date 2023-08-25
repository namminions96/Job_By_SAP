using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePosVoucher.Models
{
    public class Config
    {
        public int Id { get; set; }
        public string? Type { get ; set; } 
        public string? IpSftp { get ; set; }
        public string? username { get; set; }
        public string? password { get; set; }    
        public string? pathRemoteDirectory { get; set; }
        public string? pathLocalDirectory { get; set; }
        public string? MoveFolderPath { get; set; }
        public bool? Status { get; set; }
        public DateTime? LastTimeRun { get; set; }
    }
}
