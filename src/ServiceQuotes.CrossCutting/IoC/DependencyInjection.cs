using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceQuotes.Application.Interfaces;
using ServiceQuotes.Application.Mappings;
using ServiceQuotes.Application.Services;
using ServiceQuotes.Domain.Interfaces;
using ServiceQuotes.Infrastructure.Context;
using ServiceQuotes.Infrastructure.Repositories;
using ServiceQuotes.Infrastructure.Services;


namespace ServiceQuotes.CrossCutting.IoC;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var sqlServerConnection = string.Empty;

        if (environment.IsDevelopment())
        {
            sqlServerConnection = configuration["ConnectionStrings:DefaultConnection"];
        }
        else
        {
            sqlServerConnection = configuration["ConnectionStrings:AzureConnection"];
        }

        services.AddDbContext<ServiceQuoteApiContext>(options =>
        {
            options.UseSqlServer(sqlServerConnection, providerOptions =>
            {
                providerOptions.EnableRetryOnFailure(
                maxRetryDelay: TimeSpan.FromSeconds(60),
                maxRetryCount: 6,
                errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IQuotesRepository, QuotesRepository>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IS3BucketService, S3BucketService>();

        services.AddAutoMapper(typeof(CustomerDTOMappingProfile));
        services.AddAutoMapper(typeof(ProductDTOMappingProfile));
        services.AddAutoMapper(typeof(QuoteDTOMappingProfile));

        return services;
    }
}
