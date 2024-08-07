﻿using Microsoft.EntityFrameworkCore;
using ServiceQuotes.Domain.Entities;

namespace ServiceQuotes.Infrastructure.Context;

public class ServiceQuoteApiContext : DbContext
{
    public ServiceQuoteApiContext(DbContextOptions<ServiceQuoteApiContext> options) : base(options)
    {

    }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteProducts> QuotesProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Quote>()
            .HasMany(e => e.Products)
            .WithMany(e => e.Quotes)
            .UsingEntity<QuoteProducts>();

        modelBuilder.Entity<Quote>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
