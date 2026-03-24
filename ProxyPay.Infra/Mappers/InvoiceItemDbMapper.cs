using ProxyPay.Domain.Models;
using ProxyPay.Infra.Context;

namespace ProxyPay.Infra.Mappers
{
    public static class InvoiceItemDbMapper
    {
        public static InvoiceItemModel ToModel(InvoiceItem row)
        {
            return new InvoiceItemModel
            {
                InvoiceItemId = row.InvoiceItemId,
                InvoiceId = row.InvoiceId,
                Description = row.Description,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                Discount = row.Discount,
                Total = row.Total,
                CreatedAt = row.CreatedAt
            };
        }

        public static void ToEntity(InvoiceItemModel md, InvoiceItem row)
        {
            row.InvoiceItemId = md.InvoiceItemId;
            row.InvoiceId = md.InvoiceId;
            row.Description = md.Description;
            row.Quantity = md.Quantity;
            row.UnitPrice = md.UnitPrice;
            row.Discount = md.Discount;
            row.Total = md.Total;
            row.CreatedAt = md.CreatedAt;
        }
    }
}
