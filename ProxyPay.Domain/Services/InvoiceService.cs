using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Infra.Interfaces.AppServices;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Invoice;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository<InvoiceModel> _invoiceRepository;
        private readonly IInvoiceItemRepository<InvoiceItemModel> _invoiceItemRepository;
        private readonly IAbacatePayAppService _abacatePayAppService;
        private readonly IMapper _mapper;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            IInvoiceRepository<InvoiceModel> invoiceRepository,
            IInvoiceItemRepository<InvoiceItemModel> invoiceItemRepository,
            IAbacatePayAppService abacatePayAppService,
            IMapper mapper,
            ILogger<InvoiceService> logger
        )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _abacatePayAppService = abacatePayAppService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<InvoiceModel> GetByIdAsync(long invoiceId)
        {
            var model = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (model == null)
                return null;

            var items = await _invoiceItemRepository.ListByInvoiceAsync(invoiceId);
            model.Items = items.ToList();

            return model;
        }

        public async Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model)
        {
            var info = _mapper.Map<InvoiceInfo>(model);
            info.Items = model.Items.Select(i => _mapper.Map<InvoiceItemInfo>(i)).ToList();
            return info;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId, long customerId)
        {
            var model = await InsertAsync(invoice, storeId);
            model.SetCustomer(customerId);
            await _invoiceRepository.UpdateAsync(model);
            return model;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId)
        {
            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(storeId);

            var model = _mapper.Map<InvoiceModel>(invoice);
            model.SetStore(storeId);
            model.SetInvoiceNumber(invoiceNumber);
            model.MarkAsDraft();
            model.MarkCreated();

            var savedInvoice = await _invoiceRepository.InsertAsync(model);

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = savedInvoice.InvoiceId;
                itemModel.MarkCreated();
                var savedItem = await _invoiceItemRepository.InsertAsync(itemModel);
                savedInvoice.Items.Add(savedItem);
            }

            return savedInvoice;
        }

        public async Task<QRCodeResponse> CreateQRCodeAsync(InvoiceInsertInfo invoice, long storeId, long customerId)
        {
            _logger.LogInformation("CreateQRCode: validating input for store {StoreId}", storeId);

            if (invoice.Customer == null)
                throw new Exception("Customer is required");

            if (string.IsNullOrWhiteSpace(invoice.Customer.Email))
                throw new Exception("Customer email is required");

            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            var totalAmount = invoice.Items.Sum(i => (i.Quantity * i.UnitPrice) - i.Discount);
            if (totalAmount <= 0)
                throw new Exception("Total amount must be greater than zero");

            _logger.LogInformation("CreateQRCode: calling AbacatePay API for amount {Amount}", (int)(totalAmount * 100));

            var qrCodeRequest = new PixQrCodeCreateRequest
            {
                Amount = (int)(totalAmount * 100),
                Description = invoice.Notes ?? "Payment",
                Customer = new AbacatePayCustomerRequest
                {
                    Name = invoice.Customer.Name,
                    Email = invoice.Customer.Email,
                    Cellphone = invoice.Customer.Cellphone,
                    TaxId = invoice.Customer.DocumentId
                }
            };

            var abacatePayResponse = await _abacatePayAppService.CreatePixQrCodeAsync(qrCodeRequest);

            if (abacatePayResponse?.Data == null)
                throw new Exception("Failed to create QR Code: no response from payment provider");

            var qrCodeData = abacatePayResponse.Data;
            _logger.LogInformation("CreateQRCode: QR Code created with external ID {ExternalId}", qrCodeData.Id);

            _logger.LogInformation("CreateQRCode: creating invoice for store {StoreId}", storeId);

            var savedInvoice = await InsertAsync(invoice, storeId, customerId);
            savedInvoice.PaymentMethod = PaymentMethodEnum.Pix;
            savedInvoice.ExternalCode = qrCodeData.Id;

            if (DateTime.TryParse(qrCodeData.ExpiresAt, out var expiresAt))
                savedInvoice.ExpiresAt = expiresAt;

            await _invoiceRepository.UpdateAsync(savedInvoice);

            _logger.LogInformation("CreateQRCode: invoice {InvoiceId} created with number {InvoiceNumber}",
                savedInvoice.InvoiceId, savedInvoice.InvoiceNumber);

            return new QRCodeResponse
            {
                InvoiceId = savedInvoice.InvoiceId,
                InvoiceNumber = savedInvoice.InvoiceNumber,
                ExternalId = qrCodeData.Id,
                BrCode = qrCodeData.BrCode,
                BrCodeBase64 = qrCodeData.BrCodeBase64,
                ExpiredAt = savedInvoice.ExpiresAt
            };
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice)
        {
            var existing = await GetByIdAsync(invoice.InvoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            existing.Notes = invoice.Notes;
            existing.Discount = invoice.Discount;
            existing.DueDate = invoice.DueDate;
            existing.SetStatus(invoice.Status);

            var updated = await _invoiceRepository.UpdateAsync(existing);

            await _invoiceItemRepository.DeleteByInvoiceAsync(existing.InvoiceId);
            updated.ClearItems();

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = existing.InvoiceId;
                itemModel.MarkCreated();
                var savedItem = await _invoiceItemRepository.InsertAsync(itemModel);
                updated.Items.Add(savedItem);
            }

            return updated;
        }

        public async Task DeleteAsync(long invoiceId)
        {
            var existing = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            await _invoiceItemRepository.DeleteByInvoiceAsync(invoiceId);
            await _invoiceRepository.DeleteAsync(invoiceId);
        }
    }
}
