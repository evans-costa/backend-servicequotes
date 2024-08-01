using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using Moq;
using ServiceQuotes.Application.DTO.Product;
using ServiceQuotes.Application.Exceptions;
using ServiceQuotes.Application.Exceptions.Resources;
using ServiceQuotes.Application.Services;
using ServiceQuotes.Application.Tests.Helpers;
using ServiceQuotes.Domain.Entities;
using ServiceQuotes.Domain.Interfaces;
using ServiceQuotes.Domain.Pagination;
using X.PagedList.Extensions;

namespace ServiceQuotes.Application.Tests.Services;
public class ProductServiceTests
{
    [Theory]
    [AutoDomainData]
    public async Task CreateProduct_ShouldReturnCreatedProduct(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        Product productEntity,
        Product createdProduct,
        ProductRequestDTO productDto,
        ProductResponseDTO expectedResponse,
        ProductService sut
        )
    {
        // Arrange
        mockMapper.Setup(m => m.Map<Product>(productDto)).Returns(productEntity);
        mockUnitOfWork.Setup(u => u.ProductRepository.CreateAsync(productEntity)).ReturnsAsync(createdProduct);
        mockMapper.Setup(m => m.Map<ProductResponseDTO>(createdProduct)).Returns(expectedResponse);

        //Act
        var result = await sut.CreateProduct(productDto);

        //Assert
        result.Should().Be(expectedResponse);
        mockUnitOfWork.Verify(u => u.ProductRepository.CreateAsync(productEntity), Times.Once());
        mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetAllProduct_ShouldReturnPaginatedProducts_WhenExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        QueryParameters productParams,
        List<Product> productEntities,
        List<ProductResponseDTO> expectedResponse,
        ProductService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.ProductRepository.GetProductsAsync()).ReturnsAsync(productEntities);
        var productsPaginated = productEntities.ToPagedList(productParams.PageNumber, productParams.PageSize);
        mockMapper.Setup(m => m.Map<IEnumerable<ProductResponseDTO>>(productsPaginated)).Returns(expectedResponse);

        // Act
        var (result, metadata) = await sut.GetAllProducts(productParams);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        metadata.Should().BeEquivalentTo(new
        {
            productsPaginated.PageNumber,
            productsPaginated.PageSize,
            productsPaginated.PageCount,
            productsPaginated.TotalItemCount,
            productsPaginated.HasNextPage,
            productsPaginated.HasPreviousPage,
        });

        mockUnitOfWork.Verify(u => u.ProductRepository.GetProductsAsync(), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetProductById_ShouldReturnAProduct_WhenExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        Guid productId,
        Product productEntity,
        ProductResponseDTO expectedResponse,
        ProductService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.ProductRepository.GetAsync(p => p.ProductId == productId)).ReturnsAsync(productEntity);
        mockMapper.Setup(m => m.Map<ProductResponseDTO>(productEntity)).Returns(expectedResponse);

        // Act
        var result = await sut.GetProductById(productId);

        // Assert
        result.Should().Be(expectedResponse);
        mockUnitOfWork.Verify(u => u.ProductRepository.GetAsync(p => p.ProductId == productId), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetProductById_ShouldThrowANotFoundExcepetion_WhenIdNotExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        Guid productId,
        ProductService sut)
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.ProductRepository.GetAsync(p => p.ProductId == productId)).ReturnsAsync((Product?) null);

        // Act
        Func<Task> act = async () => await sut.GetProductById(productId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage(ExceptionMessages.PRODUCT_NOT_FOUND);
        mockUnitOfWork.Verify(u => u.ProductRepository.GetAsync(p => p.ProductId == productId), Times.Once());
    }
}