using Common.DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roxana_tema1.Middleware.Auth;

namespace Roxana_tema1.Controllers
{
    [Route("api/v2/books")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class BookControllerV2: ControllerBase
    {
        private IBookServiceAsync _bookServiceAsync;

        public BookControllerV2(IBookServiceAsync bookServiceAsync)
        {
            _bookServiceAsync = bookServiceAsync;
        }


        [HttpGet]
        [Route("{id}")]
        [Authorize(Policy = Policies.All)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Id cannot be empty");

            var result = await _bookServiceAsync.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Policies.All)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Get([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            if (!pageNumber.HasValue) pageNumber = 1;
            if (!pageSize.HasValue) pageSize = 10;

            var result = await _bookServiceAsync.GetAsync(pageNumber.Value, pageSize.Value);

            if (!result.Any()) return NoContent();

            return Ok(result);
        }

        [HttpPost]
        [Produces("application/json")]
        [Authorize(Policy = Policies.Admin)]
        public async Task<IActionResult> Add([FromBody] BookDto bookDto)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized();
            }

            var accessToken = authHeader.Substring("Bearer ".Length).Trim();

            if(bookDto == null) return BadRequest("Book data cannot be null");

            var result = await _bookServiceAsync.AddAsync(bookDto, accessToken);

            return Ok(result);
        }

        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [Authorize(Policy = Policies.Admin)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] BookDto bookDto)
        {
            if (id == Guid.Empty) return BadRequest("Id cannot be empty");

            if (id != bookDto.Id) return BadRequest("Ids do not match");

            var result = _bookServiceAsync.UpdateAsync(bookDto);

            if (await result) return Ok(result);

            return BadRequest();
        }

        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [Authorize(Policy = Policies.Admin)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Id cannot be empty");

            var result = _bookServiceAsync.RemoveAsync(id);
            if (await result) return Ok(result);

            return NotFound();

        }
    }
}
