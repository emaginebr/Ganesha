using ProxyPay.Domain.Models;
using ProxyPay.DTO.Invoice;

namespace ProxyPay.Domain.Mappers
{
    public static class InvoiceItemMapper
    {
        public static InvoiceItemInfo ToInfo(InvoiceItemModel md)
        {
            return new InvoiceItemInfo
            {
                InvoiceItemId = md.InvoiceItemId,
                InvoiceId = md.InvoiceId,
                Description = md.Description,
                Quantity = md.Quantity,
                UnitPrice = md.UnitPrice,
                Discount = md.Discount,
                Total = md.Total,
                CreatedAt = md.CreatedAt
            };
        }
    }
}
