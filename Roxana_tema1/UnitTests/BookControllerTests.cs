using System;
using System.Collections.Generic;
using System.Text;
using Common.DTOs;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Roxana_tema1.Controllers;
using Xunit;

namespace UnitTests
{
    public class BookControllerTests
    {
        private readonly Mock<IBookServiceAsync> _bookServiceMock;
        private readonly BookController _bookController;

        public BookControllerTests()
        {
            _bookServiceMock = new Mock<IBookServiceAsync>();
            _bookController = new BookController(_bookServiceMock.Object);
        }


        // TEST 1 — Happy path for Get by ID
        // The service finds the bok and returns it. We check we get a 200
        // and that the body matches what the service returned.
        [Fact]
        public async Task Get_WithValidId_ReturnsOkWithBook()
        {
            //Arrange 
            var bookId = Guid.NewGuid();
            var fakeBook = new BookDto
            {
                Id = bookId,
                Title = "The Art of Clean Code",
                CreatedDate = DateTime.Now.AddDays(-10),
                AuthorName = "Robert C. Martin",
                NumberOfPages = 450,
                Price = 59.99f,
                AddedBy = Guid.NewGuid()
            };
            _bookServiceMock.Setup(service => service.GetByIdAsync(bookId)).ReturnsAsync(fakeBook);

            //Act
            var result = await _bookController.Get(bookId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var returnedBook = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal(fakeBook.Title, returnedBook.Title);
        }

        // TEST 2 — Invalid ID (empty GUID)
        // We call Get with an empty GUID. We expect a 400 Bad Request with an error message.
        [Fact]
        public async Task Get_WithEmptyId_ReturnsBadRequestWithErrorMessage()
        {
            //Arrange
            var bookId = Guid.Empty;

            //Act
            var result = await _bookController.Get(bookId);

            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Id cannot be empty", badRequestResult.Value);
            _bookServiceMock.Verify(service => service.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        // TEST 3 — Book not found
        // The service returns null, simulating a missing record.
        // We expect a clean 404 with no extra body.
        [Fact]
        public async Task Get_WithNonexistentId_ReturnsNotFound()
        {
            //Arrange
            var bookId = Guid.NewGuid();
            _bookServiceMock
                .Setup(service => service.GetByIdAsync(bookId))
                .ReturnsAsync((BookDto?)null);

            //Act
            var result = await _bookController.Get(bookId);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 4 — Paged list with default values
        // When no page number or size is passed, the controller should
        // silently default to page 1 with 10 results. We verify the service
        // is called with exactly those values
        [Fact]
        public async Task Get_WithoutPagingParameters_ReturnsOkAndCallsServiceWithDefaults()
        {
            //Arrange
            var fakeBooks = new List<BookDto>
            {
                new BookDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 1",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    AuthorName = "Author 1",
                    NumberOfPages = 300,
                    Price = 29.99f,
                    AddedBy = Guid.NewGuid()
                },
                new BookDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 2",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    AuthorName = "Author 2",
                    NumberOfPages = 400,
                    Price = 39.99f,
                    AddedBy = Guid.NewGuid()
                }
            };

            _bookServiceMock.Setup(service => service.GetAsync(1, 10)).ReturnsAsync(fakeBooks);

            //Act
            var result = await _bookController.Get();

            //Assert
            Assert.IsType<OkObjectResult>(result);
            _bookServiceMock.Verify(service => service.GetAsync(1, 10), Times.Once);
        }

        // TEST 5 — Empty list returns 204
        // A valid request that simply has no data to return.
        // 204 is not an error — it just means "nothing here right now."
        [Fact]
        public async Task Get_WhenNoBooksExist_ReturnsNoContent()
        {
            //Arrange
            _bookServiceMock.Setup(service => service.GetAsync(1, 10)).ReturnsAsync(new List<BookDto>());
            //Act
            var result = await _bookController.Get();
            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        // TEST 6 — Successful Add
        // A valid model comes in, the service saves it and returns it with an assigned ID.
        // We check we get that saved version back in the response body.
        [Fact]
        public async Task Add_WithValidModel_ReturnsOkWithSavedBook()
        {
            // Arrange
            var newBook = new BookDto
            {
                Title = "The Pragmatic Programmer",
                CreatedDate = DateTime.Now,
                AuthorName = "Andrew Hunt and David Thomas",
                NumberOfPages = 320,
                Price = 49.99f,
                AddedBy = Guid.NewGuid()
            };

            var savedBook = new BookDto
            {
                Id = Guid.NewGuid(),
                Title = newBook.Title,
            };

            _bookServiceMock
                .Setup(service => service.AddAsyncV1(It.IsAny<BookDto>()))
                .ReturnsAsync(savedBook);

            // Act
            var result = await _bookController.Add(newBook);

            // Assert
            // Folosim IsType pentru a verifica dacă e un 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Extragem valoarea și verificăm dacă este tipul corect
            var returnData = Assert.IsType<BookDto>(okResult.Value);

            // Verificăm ID-ul ca să fim siguri că am primit obiectul "salvat" de mock
            Assert.Equal(savedBook.Id, returnData.Id);

            _bookServiceMock.Verify(x => x.AddAsyncV1(It.IsAny<BookDto>()), Times.Once);
        }


        // TEST 7 — ID mismatch on Update
        // If the route ID and the body ID don't match, reject immediately.
        // The service should never be called in this case.
        [Fact]
        public async Task Update_WithIdMismatch_ReturnsBadRequestWithErrorMessage()
        {
            //Arrange
            var routeId = Guid.NewGuid();
            var model = new BookDto
            {
                Id = Guid.NewGuid(),
                Title = "The Pragmatic Programmer",
                CreatedDate = DateTime.Now,
                AuthorName = "Andrew Hunt and David Thomas",
                NumberOfPages = 320,
                Price = 49.99f,
                AddedBy = Guid.NewGuid()
            };

            //Act
            var result = await _bookController.Update(routeId, model);

            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Ids do not match", badRequestResult.Value);
            _bookServiceMock.Verify(service => service.UpdateAsync(It.IsAny<BookDto>()), Times.Never);
        }

        // TEST 8 — Delete success and failure using Theory
        // The same delete logic runs twice with different service responses.
        // True means the delete worked (200), false means something went wrong (500).
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Delete_WithServiceResult_ReturnsExpectedResult(bool serviceResult)
        {
            var bookId = Guid.NewGuid();

            _bookServiceMock
                .Setup(x => x.RemoveAsync(bookId))
                .ReturnsAsync(serviceResult);

            var result = await _bookController.Delete(bookId);

            if (serviceResult)
            {
                Assert.IsType<OkResult>(result);
            }
            else
            {
                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
