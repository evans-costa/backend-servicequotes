using QuestPDF.Fluent;
using ServiceQuotes.Application.DTO.Invoice;
using ServiceQuotes.Application.DTO.Quote;
using ServiceQuotes.Application.Exceptions;
using ServiceQuotes.Application.Exceptions.Resources;
using ServiceQuotes.Application.Interfaces;
using ServiceQuotes.Domain.Interfaces;
using ServiceQuotes.Infrastructure.Helpers;
using System.Globalization;

namespace ServiceQuotes.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IS3BucketService _bucketService;

    public InvoiceService(IUnitOfWork unitOfWork, IS3BucketService bucketService)
    {
        _unitOfWork = unitOfWork;
        _bucketService = bucketService;
    }

    public byte[] GenerateInvoiceDocument(QuoteDetailedResponseDTO quote)
    {
        var invoice = new InvoiceDTO
        {
            InvoiceNumber = quote.QuoteId,
            CreatedAt = quote.CreatedAt,

            CustomerInfo = new CustomerInfo
            {
                Name = quote.CustomerInfo?.Name,
                Phone = quote.CustomerInfo?.Phone
            },

            Items = quote.Products?.Select(product => new OrderItem
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
            }).ToList()
        };

        var culture = CultureInfo.CreateSpecificCulture("pt-BR");
        var document = new InvoiceDocumentTemplate(invoice, culture);

        return document.GeneratePdf();
    }

    public async Task<string> GenerateInvoiceUrl(QuoteDetailedResponseDTO quote)
    {
        var invoiceDocument = GenerateInvoiceDocument(quote);

        var fileName = $"invoice_{quote.CreatedAt:yyyyMMddTHHmmss}_{quote.QuoteId:d8}.pdf";

        var fileUrl = await _bucketService.UploadFileToS3(invoiceDocument, fileName);

        return fileUrl;
    }
}
