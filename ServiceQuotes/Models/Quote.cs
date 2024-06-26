﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ServiceQuotes.Models;

public class Quote
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
    public Guid CustomerId { get; set; }

    [JsonIgnore]
    public Customer? Customer { get; set; }
    public ICollection<Product> Products { get; }

    [JsonIgnore]
    public ICollection<QuoteProducts> QuotesProducts { get; }
}
