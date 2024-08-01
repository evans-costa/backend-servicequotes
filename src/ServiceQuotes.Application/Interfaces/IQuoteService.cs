using ServiceQuotes.Application.DTO.Quote;
using ServiceQuotes.Domain.Pagination;

namespace ServiceQuotes.Application.Interfaces;
public interface IQuoteService
{
    Task<QuoteResponseDTO> CreateQuote(QuoteWithProductRequestDTO quoteWithProductDto);
    Task<(IEnumerable<QuoteResponseDTO>, object)> GetAllQuotes(QueryParameters quoteParams);
    Task<QuoteDetailedResponseDTO> GetQuoteDetailsById(int id);
    Task<(IEnumerable<QuoteResponseDTO>, object)> GetQuoteBySearch(QuoteFilterParams quoteFilterParams);
    Task SaveInvoiceOnQuote(int id);
}
