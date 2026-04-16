using System;
using System.Collections.Generic;
using System.Text;
using Common.DTOs;

namespace Interfaces
{
    public interface IBookServiceAsync
    {
        public Task<BookDto> GetByIdAsync(Guid id);

        public Task<IEnumerable<BookDto>> GetAsync(int  page = 1, int pageSize = 10);
        public Task<BookDto> AddAsyncV1(BookDto entity);
        public Task<BookDto> AddAsync(BookDto entity, string accessToken);

        public Task<bool> UpdateAsync(BookDto updateEntity);

        public Task<bool> RemoveAsync(Guid id);

    }
}
