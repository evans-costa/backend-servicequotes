using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using Moq;
using ServiceQuotes.Application.DTO.Customer;
using ServiceQuotes.Application.Exceptions;
using ServiceQuotes.Application.Exceptions.Resources;
using ServiceQuotes.Application.Services;
using ServiceQuotes.Application.Tests.Helpers;
using ServiceQuotes.Domain.Entities;
using ServiceQuotes.Domain.Interfaces;
using ServiceQuotes.Domain.Pagination;
using X.PagedList.Extensions;

namespace ServiceQuotes.Application.Tests.Services;

public class CustomerServicesTests
{
    [Theory]
    [AutoDomainData]
    public async Task CreateCustomer_ShouldReturnCreatedCustomer(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        Customer customerEntity,
        Customer createdCustomer,
        CustomerRequestDTO customerDto,
        CustomerResponseDTO expectedResponse,
        CustomerService sut
        )
    {
        // Arrange
        mockMapper.Setup(m => m.Map<Customer>(customerDto)).Returns(customerEntity);
        mockUnitOfWork.Setup(u => u.CustomerRepository.CreateAsync(customerEntity)).ReturnsAsync(createdCustomer);
        mockMapper.Setup(m => m.Map<CustomerResponseDTO>(createdCustomer)).Returns(expectedResponse);

        // Act
        var result = await sut.CreateCustomer(customerDto);

        // Assert
        result.Should().Be(expectedResponse);
        mockUnitOfWork.Verify(u => u.CustomerRepository.CreateAsync(customerEntity), Times.Once());
        mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetAllCustomer_ShouldReturnCustomersPaginated(
    [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
    [Frozen] Mock<IMapper> mockMapper,
    QueryParameters customerParams,
    CustomerService sut,
    List<Customer> customerEntities,
    List<CustomerResponseDTO> expectedResponse
    )
    {
        //Arrange
        mockUnitOfWork.Setup(u => u.CustomerRepository.GetCustomersAsync()).ReturnsAsync(customerEntities);
        var customerPaginated = customerEntities.ToPagedList(customerParams.PageNumber, customerParams.PageSize);
        mockMapper.Setup(m => m.Map<IEnumerable<CustomerResponseDTO>>(customerPaginated)).Returns(expectedResponse);

        //Act
        var (result, metadata) = await sut.GetAllCustomers(customerParams);

        //Assert
        result.Should().BeEquivalentTo(expectedResponse);
        metadata.Should().BeEquivalentTo(new
        {
            customerPaginated.PageNumber,
            customerPaginated.PageSize,
            customerPaginated.PageCount,
            customerPaginated.TotalItemCount,
            customerPaginated.HasNextPage,
            customerPaginated.HasPreviousPage,
        });

        mockUnitOfWork.Verify(u => u.CustomerRepository.GetCustomersAsync(), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetCustomerById_ShouldReturnCustomer_WhenExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        [Frozen] Mock<IMapper> mockMapper,
        Guid customerId,
        CustomerService sut,
        Customer customerEntity,
        CustomerResponseDTO expectedResponse
        )
    {
        //Arrange
        mockUnitOfWork.Setup(u => u.CustomerRepository.GetAsync(c => c.CustomerId == customerId)).ReturnsAsync(customerEntity);
        mockMapper.Setup(m => m.Map<CustomerResponseDTO>(customerEntity)).Returns(expectedResponse);

        //Act
        var result = await sut.GetCustomerById(customerId);

        //Assert
        result.Should().Be(expectedResponse);
        mockUnitOfWork.Verify(u => u.CustomerRepository.GetAsync(c => c.CustomerId == customerId), Times.Once());
    }

    [Theory]
    [AutoDomainData]
    public async Task GetCustomerById_ShouldThrowNotFoundExcepetion_WhenCustomerDoesNotExists(
        [Frozen] Mock<IUnitOfWork> mockUnitOfWork,
        CustomerService sut,
        Guid customerId
        )
    {
        // Arrange
        mockUnitOfWork.Setup(u => u.CustomerRepository.GetAsync(c => c.CustomerId == customerId)).ReturnsAsync((Customer?) null);

        // Act
        Func<Task> act = async () => await sut.GetCustomerById(customerId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage(ExceptionMessages.CUSTOMER_NOT_FOUND);
        mockUnitOfWork.Verify(u => u.CustomerRepository.GetAsync(c => c.CustomerId == customerId), Times.Once());
    }
}