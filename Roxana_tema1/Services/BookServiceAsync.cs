using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Common.DTOs;
using Common.Util;
using Interfaces;

namespace Services
{
    public class BookServiceAsync : IBookServiceAsync
    {
        private readonly IBookRepositoryAsync _bookRepositoryAsync;

        public BookServiceAsync(IBookRepositoryAsync bookRepositoryAsync)
        {
            _bookRepositoryAsync = bookRepositoryAsync;
        }

        async Task<BookDto> IBookServiceAsync.AddAsync(BookDto entity, string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            entity.AddedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
            var result = await _bookRepositoryAsync.AddAsync(entity.ToEntity());
            return result.ToDto();
        }

        async Task<BookDto> IBookServiceAsync.AddAsyncV1(BookDto entity)
        {
            var result = await _bookRepositoryAsync.AddAsync(entity.ToEntity());
            return result.ToDto();
        }

        async Task<IEnumerable<BookDto>> IBookServiceAsync.GetAsync(int page = 1, int pageSize = 100)
        {
            var result = await _bookRepositoryAsync.GetBooksAsync(page, pageSize);
            return result.Any() ? result.Select(x => x.ToDto()) : new List<BookDto>();
        }

        async Task<BookDto> IBookServiceAsync.GetByIdAsync(Guid id)
        {
            var book = await _bookRepositoryAsync.GetBookByIdAsync(id);
            return book?.ToDto();
        }

       async Task<bool> IBookServiceAsync.RemoveAsync(Guid id)
        {
            return await _bookRepositoryAsync.RemoveAsync(id);
        }

       async Task<bool> IBookServiceAsync.UpdateAsync(BookDto updateEntity)
        {
            return await _bookRepositoryAsync.UpdateAsync(updateEntity.ToEntity());  
        }
    }
}
