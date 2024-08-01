using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceQuotes.Domain.Entities;

public sealed class Quote
{
    public Quote()
    {
        Products = new Collection<Product>();
        QuotesProducts = new Collection<QuoteProducts>();
    }
    [Key]
    public int QuoteId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    [StringLength(200)]
    public string? FileUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; set; }
    public ICollection<Product> Products { get; }
    public ICollection<QuoteProducts> QuotesProducts { get; }
}
