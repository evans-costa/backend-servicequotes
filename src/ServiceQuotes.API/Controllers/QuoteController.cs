using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceQuotes.Application.DTO.Quote;
using ServiceQuotes.Application.Interfaces;
using ServiceQuotes.Domain.Pagination;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace ServiceQuotes.API.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[ApiController]
public class QuoteController : ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly ILogger<QuoteController> _logger;

    public QuoteController(IQuoteService quoteService, ILogger<QuoteController> logger)
    {
        _quoteService = quoteService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Buscar todas as cotações paginadas")]
    public async Task<ActionResult<IEnumerable<QuoteResponseDTO>>> GetAllQuotes([FromQuery] QueryParameters quoteParams)
    {
        _logger.LogInformation("### Get all quotes: GET api/qutoe/ ###");

        var (quotes, metadata) = await _quoteService.GetAllQuotes(quoteParams);

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(quotes);
    }

    [HttpGet("{id:int}", Name = "GetQuoteDetailsById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Buscar os detalhes da cotação por ID")]
    public async Task<ActionResult<QuoteDetailedResponseDTO>> GetQuoteDetailsById(int id)
    {
        _logger.LogInformation("### Get a detailed quote by ID: GET api/quote/{id} ###", id);

        var quote = await _quoteService.GetQuoteDetailsById(id);

        return Ok(quote);
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Buscar cotações por filtro")]
    public async Task<ActionResult<IEnumerable<QuoteResponseDTO>>> GetQuoteBySearch([FromQuery] QuoteFilterParams quoteParams)
    {
        _logger.LogInformation("### Get a quote by search criteria: GET api/quote/search/{quoteParams} ###", quoteParams);

        var (quotes, metadata) = await _quoteService.GetQuoteBySearch(quoteParams);

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(quotes);
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [SwaggerOperation(Summary = "Criar uma cotação")]
    public async Task<ActionResult<QuoteResponseDTO>> CreateQuote
        ([FromBody] QuoteWithProductRequestDTO quoteWithProductsDto)
    {
        _logger.LogInformation("### Create a quote: POST api/quote");

        var newQuoteDto = await _quoteService.CreateQuote(quoteWithProductsDto);
        await _quoteService.SaveInvoiceOnQuote(newQuoteDto.QuoteId);

        return new CreatedAtRouteResult("GetQuoteDetailsById", new { id = newQuoteDto.QuoteId }, newQuoteDto);
    }
}
