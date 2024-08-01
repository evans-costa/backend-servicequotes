using FluentAssertions;
using ServiceQuotes.Application.DTO.Product;
using System.ComponentModel.DataAnnotations;

namespace ServiceQuotes.Application.Tests.DTO;
public class ProductRequestDTOTests
{
    [Fact]
    public void ShoudlThrowError_WhenNameIsNotProvided()
    {
        var dto = new ProductRequestDTO { Name = null };

        var validationResult = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);
        var isValid = Validator.TryValidateObject(dto, validationContext, validationResult);

        isValid.Should().BeFalse();
        validationResult.Should().HaveCount(1);
        validationResult[0].ErrorMessage.Should().Be("O nome é obrigatório");
    }
}
