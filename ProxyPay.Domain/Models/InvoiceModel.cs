using ProxyPay.DTO.Invoice;
using System;

namespace ProxyPay.Domain.Models
{
    public class InvoiceModel
    {
        public long InvoiceId { get; set; }
        public long UserId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Notes { get; set; }
        public InvoiceStatusEnum Status { get; set; }
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
