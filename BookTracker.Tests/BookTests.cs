using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
using System.Net.Http.Headers;

namespace BookTracker.Tests;

public class BookTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BookTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = "Password123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    #region Unit Tests
    [Fact] 
    public void ValidateRequiredFields_ShouldFailIfMissing() 
    {
        var book = new Book { Title = "", Author = "" };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(book);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(book, context, results, true).Should().BeFalse();
    }
    #endregion

    #region API Tests
    [Fact] 
    public async Task CreateBook_Valid_Returns201() 
    {
        await AuthenticateAsync();
        var book = new Book { Title = "Test Book", Author = "Test Author" };
        var response = await _client.PostAsJsonAsync("/api/v1/books", book);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact] 
    public async Task GetAllBooks_Returns200() 
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/books");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task UpdateBook_Valid_Returns200() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Old", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        
        book!.Title = "New";
        var response = await _client.PutAsJsonAsync($"/api/v1/books/{book.Id}", book);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task DeleteBook_Valid_Returns204() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "To Delete", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();
        
        var response = await _client.DeleteAsync($"/api/v1/books/{book!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact] 
    public async Task CreateBook_InvalidISBN_Returns400() 
    {
        await AuthenticateAsync();
        var book = new Book { Title = "Title", Author = "Author", ISBN = "invalid" };
        var response = await _client.PostAsJsonAsync("/api/v1/books", book);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact] 
    public async Task CreateBook_InvalidYear_Returns400() 
    {
        await AuthenticateAsync();
        var book = new Book { Title = "Title", Author = "Author", PublicationYear = 2500 };
        var response = await _client.PostAsJsonAsync("/api/v1/books", book);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact] 
    public async Task UploadCover_ValidImage_Succeeds() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new Book { Title = "Title", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>();

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x01 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        var response = await _client.PostAsync($"/api/v1/books/{book!.Id}/cover", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    #endregion
}
