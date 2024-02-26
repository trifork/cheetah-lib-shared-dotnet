using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;

namespace Cheetah.OpenSearch.ExampleAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class IndexController : ControllerBase
{
    readonly IOpenSearchClient _client;

    public IndexController(IOpenSearchClient client)
    {
        _client = client;
    }

    [HttpGet("indices/{indexPattern}")]
    public async Task<IActionResult> GetIndices([FromRoute] string indexPattern)
    {
        var response = await _client.Indices.GetAsync(indexPattern);
        var indexNames = response.Indices.Select(x => x.Key.Name);
        return Ok(indexNames);
    }

    [HttpPost("indices/{indexName}")]
    public async Task<IActionResult> CreateIndex([FromRoute] string indexName)
    {
        await _client.Indices.CreateAsync(indexName);
        return Ok();
    }

    [HttpDelete("indices/{indexName}")]
    public async Task<IActionResult> DeleteIndex([FromRoute] string indexName)
    {
        await _client.Indices.DeleteAsync(indexName);
        return NoContent();
    }
}
