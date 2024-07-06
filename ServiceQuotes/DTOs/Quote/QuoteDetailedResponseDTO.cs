﻿using ServiceQuotes.DTOs.Customer;
using ServiceQuotes.DTOs.Product;

namespace ServiceQuotes.DTOs.Quote;

public class QuoteDetailedResponseDTO
{
    public int QuoteId { get; set; }
    public CustomerResponseDTO? CustomerInfo { get; set; }
    public decimal TotalPrice { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductDetailResponseDTO>? Products { get; set; }

}