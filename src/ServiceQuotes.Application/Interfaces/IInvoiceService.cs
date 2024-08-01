using ServiceQuotes.Application.DTO.Quote;

namespace ServiceQuotes.Application.Interfaces;

public interface IInvoiceService
{
    byte[] GenerateInvoiceDocument(QuoteDetailedResponseDTO quote);
    Task<string> GenerateInvoiceUrl(QuoteDetailedResponseDTO quote);
}
