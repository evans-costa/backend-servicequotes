using Microsoft.EntityFrameworkCore;
using ServiceQuotes.Domain.Entities;
using ServiceQuotes.Domain.Interfaces;
using ServiceQuotes.Domain.Pagination;
using ServiceQuotes.Infrastructure.Context;

namespace ServiceQuotes.Infrastructure.Repositories;

public class QuotesRepository : Repository<Quote>, IQuotesRepository
{
    public QuotesRepository(ServiceQuoteApiContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Quote>> GetQuotesAsync()
    {
        var quotes = await GetAllAsync();

        var orderedQuotes = quotes.OrderBy(q => q.QuoteId).AsQueryable();

        return orderedQuotes;
    }

    public async Task<Quote?> GetDetailedQuoteAsync(int id)
    {
        var detailedQuote = await _context.Quotes
            .Include(q => q.Customer)
                .Where(c => c.QuoteId == id)
            .Include(q => q.Products)
            .ThenInclude(p => p.QuoteProducts
                .Where(qp => qp.QuoteId == id)
            )
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuoteId == id);

        return detailedQuote;
    }

    public async Task<IEnumerable<Quote>> SearchQuotesAsync(QuoteFilterParams quoteParams)
    {
        IEnumerable<Quote> quotes = await _context.Quotes
            .Include(q => q.Customer)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();

        FilterCriteria? filterType = quoteParams.FilterCriteria;

        if (filterType.HasValue)
        {
            quotes = filterType.Value switch
            {
                FilterCriteria.Date => GetQuoteByDateAsync(quoteParams, quotes),
                FilterCriteria.Customer => GetQuoteByCustomerName(quoteParams, quotes),
                _ => quotes
            };
        }

        return quotes.Any() ? quotes.AsQueryable() : [];
    }

    private static IEnumerable<Quote> GetQuoteByDateAsync(QuoteFilterParams quoteParams, IEnumerable<Quote> quotes)
    {
        if (!string.IsNullOrEmpty(quoteParams.CreatedDate))
        {
            if (DateTime.TryParse(quoteParams.CreatedDate, out DateTime createdDate))
            {
                var filteredQuotes = quotes.Where(q => q.CreatedAt.Date == createdDate.Date).OrderBy(q => q.CreatedAt);

                return filteredQuotes;
            }
        }

        return [];
    }

    private static IEnumerable<Quote> GetQuoteByCustomerName(QuoteFilterParams quoteParams, IEnumerable<Quote> quotes)
    {
        if (!string.IsNullOrEmpty(quoteParams.CustomerName))
        {
            var filteredQuotes = quotes.Where(q => q.Customer!.Name!.Contains(quoteParams.CustomerName, StringComparison.CurrentCultureIgnoreCase)).OrderBy(q => q.CreatedAt);

            return filteredQuotes;
        };

        return [];
    }
}
