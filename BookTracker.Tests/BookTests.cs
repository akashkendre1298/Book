using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Models;
using BookTracker.Api.Models.Dtos;
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
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Options);
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

    [Fact] 
    public void ISBN10_Validation_Works() 
    {
        var book = new Book { Title = "T", Author = "A", ISBN = "0306406152" };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(book);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(book, context, results, true).Should().BeTrue();
    }

    [Fact] 
    public void ISBN_WithHyphens_Works() 
    {
        var book = new Book { Title = "T", Author = "A", ISBN = "978-3-16-148410-0" };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(book);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(book, context, results, true).Should().BeTrue();
    }

    [Fact] 
    public void PubYear_Boundary_1450_Passes() 
    {
        var book = new Book { Title = "T", Author = "A", PublicationYear = 1450 };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(book);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(book, context, results, true).Should().BeTrue();
    }

    [Fact] 
    public void PubYear_Future_Fails() 
    {
        var book = new Book { Title = "T", Author = "A", PublicationYear = 2101 }; // Max 2100 in Range
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
        var response = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Test Book", Author = "Test Author" });
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
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Old", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        
        var updateDto = new BookUpdateDto { Title = "New" };
        var response = await _client.PutAsJsonAsync($"/api/v1/books/{book!.Id}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact] 
    public async Task DeleteBook_Valid_Returns204() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "To Delete", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        var response = await _client.DeleteAsync($"/api/v1/books/{book!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact] 
    public async Task CreateBook_DuplicateISBN_Returns409() 
    {
        await AuthenticateAsync();
        var book = new BookCreateDto { Title = "B1", Author = "A1", ISBN = "1234567890" };
        await _client.PostAsJsonAsync("/api/v1/books", book);
        var response = await _client.PostAsJsonAsync("/api/v1/books", book);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact] 
    public async Task GetById_NonExisting_Returns404() 
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact] 
    public async Task UploadCover_ValidImage_Succeeds() 
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/books", new BookCreateDto { Title = "Title", Author = "Author" });
        var book = await createResponse.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Options);
        var content = new MultipartFormDataContent();
        
        // Mocking a valid JPEG magic bytes (FF D8 FF)
        var fileBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");
        
        var response = await _client.PostAsync($"/api/v1/books/{book!.Id}/cover", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    #endregion
}

