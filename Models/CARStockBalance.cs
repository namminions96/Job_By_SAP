using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePosVoucher.Models
{
    public class CARStockBalance
    {
        public Guid Id { get; set; }
        public string? TimeStamp { get; set; }
        public string? Site { get; set; }
        public string? ArticleNumber { get; set; }
        public string? MCH5 { get; set; }
        public string? BaseUoM { get; set; }
        public string? UnreUseQty { get; set; }
        public string? UnreConsQty { get; set; }
        public string? TransitQty { get; set; }
        public string? UnprSaleQty { get; set; }
        public string? FileName { get; set; }   
        public int Status { get; set; }
    }
}
