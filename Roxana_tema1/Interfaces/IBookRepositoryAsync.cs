using System;
using System.Collections.Generic;
using System.Text;
using Common.DTOs;
using Common.Entities;

namespace Interfaces
{
    public interface IBookRepositoryAsync
    {
        public Task<Book> GetBookByIdAsync(Guid id);

        public Task<IEnumerable<Book>> GetBooksAsync(int pageNumber, int pageSize);

        public Task<Book> AddAsync(Book book);

        public Task<bool> UpdateAsync(Book book);

        public Task<bool> RemoveAsync(Guid id);
    }
}
