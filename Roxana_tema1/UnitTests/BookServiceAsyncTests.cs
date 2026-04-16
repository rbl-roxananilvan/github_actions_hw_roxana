using System;
using System.Collections.Generic;
using System.Text;
using Common.DTOs;
using Common.Entities;
using Interfaces;
using Moq;
using Services;
using Xunit;

namespace UnitTests
{
    public class BookServiceAsyncTests
    {
        private readonly Mock<IBookRepositoryAsync> _bookRepository;
        private readonly IBookServiceAsync _bookServiceAsync;

        public BookServiceAsyncTests()
        {
            _bookRepository = new Mock<IBookRepositoryAsync>();
            _bookServiceAsync = new BookServiceAsync(_bookRepository.Object);
        }

        [Fact]
        public async Task AddAsyncV1_WithValidBook_ReturnsMappedBookDto()
        {
            // Arrange
            var inputDto = new BookDto
            {
                Title = "Clean Code",
                AuthorName = "Robert C. Martin",
                NumberOfPages = 450,
                Price = 50,
                CreatedDate = DateTime.Now,
                AddedBy = Guid.NewGuid()
            };

            var savedEntity = new Book 
            {
                Id = Guid.NewGuid(),
                Title = inputDto.Title,
                AuthorName = inputDto.AuthorName,
                NumberOfPages = inputDto.NumberOfPages,
                Price = inputDto.Price,
                CreatedDate = inputDto.CreatedDate,
                AddedBy = (Guid)inputDto.AddedBy
            };

            _bookRepository
                .Setup(x => x.AddAsync(It.IsAny<Book>()))
                .ReturnsAsync(savedEntity);

            // Act
            var result = await _bookServiceAsync.AddAsyncV1(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedEntity.Id, result.Id);
            Assert.Equal(savedEntity.Title, result.Title);
            _bookRepository.Verify(x => x.AddAsync(It.IsAny<Book>()), Times.Once);
        }


    }
}
