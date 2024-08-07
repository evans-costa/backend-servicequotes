﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServiceQuotes.Application.DTO.Quote;

public class QuoteProductsRequestDTO
{
    [JsonIgnore]
    public int QuoteId { get; set; }
    [Required]
    public Guid ProductId { get; set; }
    [Required]
    [Range(1, 100, ErrorMessage = "Quantidade deve ser entre 0 e 100")]
    public int Quantity { get; set; }
    [Required]
    [Range(0.01, 10000.00, ErrorMessage = "Preço deve ser entre 0 e 10.000")]
    public decimal Price { get; set; }
}
