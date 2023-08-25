using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePosVoucher.Models
{
    public class Inb_Voucher
    {
        public string? Voucher_Type { get; set; }
        public string? SerialNo { get; set; }
        public double? Voucher_Value { get; set; }
        public string? Voucher_Currency { get; set; }
        public string? Validity_From_Date { get; set; }
        public string? Expiry_Date { get; set; }
        public string? Processing_Type { get; set; }
        public string? Status { get; set; }
        public string? Site { get; set; }
        public string? Article_No { get; set; }
        public string? Bonus_Buy { get; set; }
        public string? POSNo { get; set; }
        public string? ReceiptNo { get; set; }
        public string? TranDate { get; set; }
        public string? TranTime { get; set; }

    }
    public class INB_VoucherToSAP
    {
        public string? Voucher_Type { get; set; }
        public string? SerialNo { get; set; }
        public double? Voucher_Value { get; set; }
        public string? Voucher_Currency { get; set; }
        public string? Validity_From_Date { get; set; }
        public string? Expiry_Date { get; set; }
        public string? Processing_Type { get; set; }
        public string? Status { get; set; }
        public string? Site { get; set; }
        public string? Article_No { get; set; }
        public string? Bonus_Buy { get; set; }
        public string? POSNo { get; set; }
        public string? ReceiptNo { get; set; }
        public string? TranDate { get; set; }
        public string? TranTime { get; set; }
        public string? FileName { get; set; }
    }
    public class ApiResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
