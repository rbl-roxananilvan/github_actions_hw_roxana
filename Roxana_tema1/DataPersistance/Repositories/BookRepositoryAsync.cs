using System;
using System.Collections.Generic;
using System.Text;
using Common.DTOs;
using Common.Entities;
using Interfaces;

namespace DataPersistance.Repositories
{
    public class BookRepositoryAsync : IBookRepositoryAsync
    {
        private IList<Book> _books;

        public BookRepositoryAsync()
        {
            _books = new List<Book>();
        }

        async Task<Book> IBookRepositoryAsync.AddAsync(Book book)
        {
            book.Id = Guid.NewGuid();
            _books.Add(book);

            return book;
        }

        async Task<Book> IBookRepositoryAsync.GetBookByIdAsync(Guid id)
        {
            return _books.FirstOrDefault(x => x.Id == id);
        }

       async Task<IEnumerable<Book>> IBookRepositoryAsync.GetBooksAsync(int pageNumber, int pageSize)
        {
            return _books
                .Skip((pageNumber - 1) *pageSize)
                .Take(pageSize)
                .ToList();

        }

        async Task<bool> IBookRepositoryAsync.RemoveAsync(Guid id)
        {
            var existingEntity = _books.FirstOrDefault(x => x.Id == id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Item with given id does not exist");

            return _books.Remove(existingEntity);
        }

        async Task<bool> IBookRepositoryAsync.UpdateAsync(Book book)
        {
            var existingEntity = _books.FirstOrDefault(x => x.Id == book.Id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Item with given id does not exist");

            existingEntity.Title = book.Title;
            existingEntity.Price = book.Price;
            existingEntity.NumberOfPages = book.NumberOfPages;
            existingEntity.AuthorName = book.AuthorName;
            existingEntity.CreatedDate = book.CreatedDate;

            return true;
        }
    }
}
