using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using Moq;
using ServiceQuotes.Application.DTO.Quote;
using ServiceQuotes.Application.Exceptions;
using ServiceQuotes.Application.Exceptions.Resources;
using ServiceQuotes.Application.Interfaces;
using ServiceQuotes.Application.Services;
using ServiceQuotes.Application.Tests.Helpers;
using ServiceQuotes.Domain.Entities;
using ServiceQuotes.Domain.Interfaces;
using ServiceQuotes.Domain.Pagination;
using System.Linq.Expressions;
using X.PagedList.Extensions;

namespace ServiceQuotes.Application.Tests.Services;
public class QuotesServiceTests
{
    [Theory]
    [AutoDomainData]
    public async Task CreateQuote_ShouldReturnCreatedQuote(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        QuoteWithProductRequestDTO quoteDto,
        Quote quoteEntity,
        Quote createdQuote,
        Product productEntity,
        Customer customerEntity,
        QuoteProducts quoteProductEntity,
        QuoteResponseDTO expectedResponse,
        QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.CustomerRepository.GetAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(customerEntity);
        mockUnitOfWork.Setup(u => u.ProductRepository.GetAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(productEntity);

        mockMapper.Setup(m => m.Map<Quote>(quoteDto.Quote)).Returns(quoteEntity);
        mockMapper.Setup(m => m.Map<QuoteProducts>(quoteDto.Products!.First())).Returns(quoteProductEntity);
        mockMapper.Setup(m => m.Map<QuoteResponseDTO>(quoteEntity)).Returns(expectedResponse);

        mockUnitOfWork.Setup(u => u.QuotesRepository.CreateAsync(quoteEntity)).ReturnsAsync(createdQuote);
        mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await sut.CreateQuote(quoteDto);

        // Assert
        result.Should().Be(expectedResponse);
        mockUnitOfWork.Verify(u => u.CustomerRepository.GetAsync(It.IsAny<Expression<Func<Customer, bool>>>()), Times.Once());
        mockUnitOfWork.Verify(u => u.ProductRepository.GetAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.AtLeastOnce());
        mockUnitOfWork.Verify(u => u.QuotesRepository.CreateAsync(quoteEntity), Times.Once());
        mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task SaveInvoiceOnQuote_ShouldSaveInvoiceURL_WhenQuoteExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        [Frozen] Mock<IInvoiceService> mockInvoiceService,
        int quoteId,
        string invoiceUrl,
        QuoteDetailedResponseDTO detailedQuote,
        Quote quoteDto,
        QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.QuotesRepository.GetDetailedQuoteAsync(quoteId)).ReturnsAsync(quoteDto);
        mockMapper.Setup(m => m.Map<QuoteDetailedResponseDTO>(quoteDto)).Returns(detailedQuote);

        mockInvoiceService.Setup(i => i.GenerateInvoiceUrl(detailedQuote)).ReturnsAsync(invoiceUrl);

        mockMapper.Setup(m => m.Map<Quote>(detailedQuote)).Returns(quoteDto);

        // Act
        await sut.SaveInvoiceOnQuote(quoteId);

        // Assert
        mockInvoiceService.Verify(i => i.GenerateInvoiceUrl(detailedQuote), Times.Once());
        mockUnitOfWork.Verify(u => u.QuotesRepository.Update(quoteDto), Times.Once());
        mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once());

        detailedQuote.FileUrl.Should().Be(invoiceUrl);
    }

    [Theory]
    [AutoDomainData]
    public async Task GetAllQuotes_ShouldReturnPaginatedQuotes(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        List<QuoteResponseDTO> expectedResult,
        List<Quote> quotesEntities,
        QueryParameters quoteParams,
        QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.QuotesRepository.GetQuotesAsync()).ReturnsAsync(quotesEntities);
        var quotesPaginated = quotesEntities.ToPagedList(quoteParams.PageNumber, quoteParams.PageSize);
        mockMapper.Setup(m => m.Map<IEnumerable<QuoteResponseDTO>>(quotesPaginated)).Returns(expectedResult);

        // Act
        var (result, metadata) = await sut.GetAllQuotes(quoteParams);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        metadata.Should().BeEquivalentTo(new
        {
            quotesPaginated.PageNumber,
            quotesPaginated.PageSize,
            quotesPaginated.PageCount,
            quotesPaginated.TotalItemCount,
            quotesPaginated.HasNextPage,
            quotesPaginated.HasPreviousPage,
        });

        mockUnitOfWork.Verify(u => u.QuotesRepository.GetQuotesAsync(), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetQuotesBySearch_ShouldReturnPaginatedSearchedQuotes(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        List<Quote> quoteEntities,
        List<QuoteResponseDTO> expectedResult,
        QuoteFilterParams quoteFilterParams,
        QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.QuotesRepository.SearchQuotesAsync(quoteFilterParams)).ReturnsAsync(quoteEntities);
        var quotesPaginated = quoteEntities.ToPagedList(quoteFilterParams.PageNumber, quoteFilterParams.PageSize);
        mockMapper.Setup(m => m.Map<IEnumerable<QuoteResponseDTO>>(quoteEntities)).Returns(expectedResult);

        // Act
        var (result, metadata) = await sut.GetQuoteBySearch(quoteFilterParams);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        metadata.Should().BeEquivalentTo(new
        {
            quotesPaginated.PageNumber,
            quotesPaginated.PageSize,
            quotesPaginated.PageCount,
            quotesPaginated.TotalItemCount,
            quotesPaginated.HasNextPage,
            quotesPaginated.HasPreviousPage,
        });

        mockUnitOfWork.Verify(u => u.QuotesRepository.SearchQuotesAsync(quoteFilterParams), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetQuotesBySearch_ShouldReturnNotFoundException_WhenQuoteNotExists(
    [Frozen] Mock<IUnitOfWork> mockUnitOfWork, QuoteFilterParams quoteParams,
    QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.QuotesRepository.SearchQuotesAsync(quoteParams)).ReturnsAsync(Enumerable.Empty<Quote>());

        // Act
        Func<Task> act = async () => await sut.GetQuoteBySearch(quoteParams);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage(ExceptionMessages.QUOTE_SEARCH_NOT_FOUND);

        mockUnitOfWork.Verify(u => u.QuotesRepository.SearchQuotesAsync(quoteParams), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetQuoteById_ShouldReturnDetailedQuote_WhenQuoteExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        Quote quoteEntity,
        QuoteDetailedResponseDTO expectedResult,
        int quoteId,
        QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.QuotesRepository.GetDetailedQuoteAsync(quoteId)).ReturnsAsync(quoteEntity);
        mockMapper.Setup(m => m.Map<QuoteDetailedResponseDTO>(quoteEntity)).Returns(expectedResult);

        // Act
        var result = await sut.GetQuoteDetailsById(quoteId);

        // Assert
        result.Should().Be(expectedResult);
        mockUnitOfWork.Verify(u => u.QuotesRepository.GetDetailedQuoteAsync(quoteId), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetQuoteById_ShouldThrowANotFoundException_WhenIdNotExists(
    [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
    int quoteId,
    QuoteService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.QuotesRepository.GetDetailedQuoteAsync(quoteId)).ReturnsAsync((Quote?) null);

        // Act
        Func<Task> act = async () => await sut.GetQuoteDetailsById(quoteId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage(ExceptionMessages.QUOTE_NOT_FOUND);
        mockUnitOfWork.Verify(u => u.QuotesRepository.GetDetailedQuoteAsync(quoteId), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public void AddProductToQuote_ShouldAddProduct_WhenProductIsValid(
        [Frozen] Mock<IMapper> mockMapper,
        Quote quote,
        QuoteProductsRequestDTO productDto,
        QuoteProducts quoteProducts,
        Product product,
        QuoteService sut)
    {
        quoteProducts.ProductId = product.ProductId;
        mockMapper.Setup(m => m.Map<QuoteProducts>(productDto)).Returns(quoteProducts);

        // Act
        sut.AddProductToQuote(quote, productDto, product);

        // Assert
        quote.QuotesProducts.Should().Contain(p => p.ProductId == product.ProductId);
    }

    [Theory]
    [AutoDomainData]
    public void AddProductToQuote_ShouldThrowConflictException_WhenProductIsDuplicated(
        Quote quote,
        QuoteProductsRequestDTO productDto,
        Product product,
        QuoteProducts quoteProduct,
        QuoteService sut)
    {
        // Arrange
        quoteProduct.ProductId = product.ProductId;
        quote.QuotesProducts.Add(quoteProduct);
        productDto.ProductId = quoteProduct.ProductId;

        // Act
        Action act = () => sut.AddProductToQuote(quote, productDto, product);

        // Assert
        act.Should().Throw<ConflictException>().WithMessage(ExceptionMessages.PRODUCT_DUPLICATED);
    }
}
