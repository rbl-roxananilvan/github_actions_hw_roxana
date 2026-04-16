using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Common.DTOs;
using Microsoft.AspNetCore.Identity.Data;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Started");

        var client = new HttpClient();
        client.BaseAddress = new Uri("https://localhost:7175");

        var newBook = new BookDto
        {
            Title = "Test",
            CreatedDate = DateTime.Now,
            AuthorName = "Test",
            NumberOfPages = 1,
            Price = 1
        };

        var json = JsonSerializer.Serialize(newBook);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var postResponse = await client.PostAsync("/api/books", content);
        Console.WriteLine($"POST Status: {postResponse.StatusCode}");

        var newBook2 = new BookDto
        {
            Title = "Test2",
            CreatedDate = DateTime.Now,
            AuthorName = "Test2",
            NumberOfPages = 1,
            Price = 1
        };

        var json2 = JsonSerializer.Serialize(newBook2);
        var conten2 = new StringContent(json2, Encoding.UTF8, "application/json");

        var postResponse2 = await client.PostAsync("/api/books", content);

        Console.WriteLine($"POST Status: {postResponse2.StatusCode}");
        var getResponse = await client.GetAsync("/api/books?pageNumber=1&pageSize=10");

        if (getResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            Console.WriteLine("No books found");
            return;
        }

        var responseContent = await getResponse.Content.ReadAsStringAsync();

        Console.WriteLine("RAW JSON:");
        Console.WriteLine(responseContent);

        var books = JsonSerializer.Deserialize<List<BookDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (books == null || !books.Any())
        {
            Console.WriteLine("No books found for update");
            return;
        }

        var bookToUpdate = books.First();

        Console.WriteLine($"Updating book: {bookToUpdate.Title} ({bookToUpdate.Id})");

        bookToUpdate.Title = "UPDATED FROM CONSOLE";

        var updateJson = JsonSerializer.Serialize(bookToUpdate);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        var putResponse = await client.PutAsync($"/api/books/{bookToUpdate.Id}", updateContent);

        Console.WriteLine($"PUT Status: {putResponse.StatusCode}");

        var bookToDelete = books.First();
        Console.WriteLine($"Deleting book: {bookToDelete.Title} ({bookToDelete.Id})");

        var deleteResponse = await client.DeleteAsync($"/api/books/{bookToDelete.Id}");

        Console.WriteLine($"DELETE Status: {deleteResponse.StatusCode}");

        var finalGet = await client.GetAsync("/api/books?pageNumber=1&pageSize=10");
        var finalContent = await finalGet.Content.ReadAsStringAsync();

        Console.WriteLine("After delete:");
        Console.WriteLine(finalContent);

        Console.WriteLine("Login part");

        var authenticateRequest = new AuthenticateRequest()
        {
            UserName = "test-admin",
            Password = "admin"
        };

        var authJson = JsonSerializer.Serialize(authenticateRequest);
        var authContent = new StringContent(authJson, Encoding.UTF8, "application/json");

        var authResponse = await client.PostAsync("api/v2/login", authContent);


        if (authResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Login successful");

            var authResponseContent = await authResponse.Content.ReadFromJsonAsync<AuthenticateResponse>();

            Console.WriteLine("Authentication response:", authResponseContent);
            var accessToken = authResponseContent.AccessToken;

            Console.WriteLine($"Access Token: {accessToken}");

            Console.WriteLine("Token refresh part");

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/v2/login/renew-access");
            refreshRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var refreshResponse = await client.SendAsync(refreshRequest);

            if (refreshResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Token renewed successfully");

                var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
                var newTokens = JsonSerializer.Deserialize<AuthenticateResponse>(refreshContent);
                Console.WriteLine($"New Access Token: {newTokens.AccessToken}");
            }
            else
            {
                Console.WriteLine("Token renewal failed: " + refreshResponse.StatusCode);
            }
        }
        else
        {
            Console.WriteLine("Login failed: " + authResponse.StatusCode);
        }

        Console.WriteLine("Apasă orice tastă...");
        Console.ReadLine();
    }
}