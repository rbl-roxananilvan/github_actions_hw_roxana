using Common.DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Roxana_tema1.Middleware.Auth;

namespace Roxana_tema1.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private IBookServiceAsync _bookServiceAsync;

        public BookController(IBookServiceAsync bookServiceAsync)
        {
            _bookServiceAsync = bookServiceAsync;
        }

        [HttpGet]
        [Route("{id}")]
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
        public async Task<IActionResult> Add([FromBody] BookDto bookDto)
        {
            if (bookDto == null) return BadRequest("Book data cannot be null");

            var result = await _bookServiceAsync.AddAsyncV1(bookDto);

            return Ok(result);
        }

        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Id cannot be empty");

            var result = await _bookServiceAsync.RemoveAsync(id);

            if (result)
                return Ok();

            return NotFound();
        }
    }
}

