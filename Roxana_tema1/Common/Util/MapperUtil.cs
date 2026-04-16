using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Common.DTOs;
using Common.Entities;

namespace Common.Util
{
    public static class MapperUtil
    {
        public static Book ToEntity(this BookDto bookDto)
        {
            return new Book
            {
                Id = bookDto.Id ?? Guid.Empty,
                Title = bookDto.Title,
                CreatedDate = bookDto.CreatedDate,
                AuthorName = bookDto.AuthorName,
                NumberOfPages = bookDto.NumberOfPages,
                Price = bookDto.Price,
                AddedBy = bookDto.AddedBy ?? Guid.Empty
            };
        }

        public static BookDto ToDto(this Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                CreatedDate = book.CreatedDate,
                AuthorName = book.AuthorName,
                NumberOfPages = book.NumberOfPages,
                Price = book.Price,
                AddedBy = book.AddedBy
            };
        }
    }
}
