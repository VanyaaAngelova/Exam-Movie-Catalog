using ExamFinal;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.ComponentModel.Design;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;


public class Tests
{
    private RestClient client;
    private string movieId;

    [OneTimeSetUp]
    public void Setup()
    {
        string jwtToken = GetJWTToken("vanya1@test.com", "123456");
        RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
        {
            Authenticator = new JwtAuthenticator(jwtToken)
        };
        this.client = new RestClient(options);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this.client?.Dispose();
    }

    private string GetJWTToken(string email, string password)
    {
        RestSharp.RestClient client = new RestSharp.RestClient("http://144.91.123.158:5000");
        RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
        request.AddJsonBody(new { email, password });
        RestResponse response = client.Execute(request);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
            var token = content.GetProperty("accessToken").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Token not found in the response.");
            }
            return token;
        }
        else
        {
            throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}");
        }
    }

    [Order(1)]
    [Test]
    public void CreateMovieWithRequiredFields_ShouldReturnSuccess()
    {

        var request = new RestRequest("/api/Movie/Create", Method.Post);

        var body = new
        {
            title = "Test Movie",
            description = "Test Description"
        };

        request.AddJsonBody(body);


        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


        var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);


        Assert.That(responseData, Is.Not.Null);
        Assert.That(responseData.movie, Is.Not.Null);
        Assert.That(responseData.movie.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(responseData.Msg, Is.EqualTo("Movie created successfully!"));


        movieId = responseData.movie.Id;
    }
    [Test]
    [Order(2)]
    public void EditMovie_ShouldReturnSuccess()
    {
        var request = new RestRequest($"/api/Movie/Edit?movieId={movieId}", Method.Put);

        var body = new
        {
            title = "Edited Test Movie",
            description = "Edited Test Description"
        };

        request.AddJsonBody(body);

        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ApiResponseDTO responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

        Assert.That(responseData.Msg, Is.EqualTo("Movie edited successfully!"));
    }
    [Order(3)]
    [Test]
    public void GetAllMovies_ShouldReturnNonEmptyArray()
    {
        RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        List<MovieDTO> readyResponse = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);
        Assert.That(readyResponse, Is.Not.Null);
        Assert.That(readyResponse, Is.Not.Empty);
        Assert.That(readyResponse.Count, Is.GreaterThanOrEqualTo(1));

    }
    [Test]
    [Order(4)]
    public void DeleteExistingMovie_ShouldSuccess()
    {
        Assert.That(movieId, Is.Not.Null.And.Not.Empty);

        RestRequest request = new RestRequest($"/api/Movie/Delete?movieId={movieId}", Method.Delete);

        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

        Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
    }
    [Test]
    [Order(5)]
    public void CreateMovie_WithInvalidFields_ShouldReturnBadRequest()
    {
        var request = new RestRequest("/api/Movie/Create", Method.Post);

        var body = new
        {
            title = "",
            description = ""
        };

        request.AddJsonBody(body);
        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }
    [Test]
    [Order(6)]
    public void EditNonExistingMovie_ShouldReturnBadRequest()
    {
        // Arrange
        string invalidMovieId = "non-existing-id";

        var request = new RestRequest($"/api/Movie/Edit?movieId={invalidMovieId}", Method.Put);

        var body = new
        {
            title = "Edited Movie",
            description = "Edited Description"
        };

        request.AddJsonBody(body);

        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode,Is.EqualTo(HttpStatusCode.BadRequest));
        ApiResponseDTO responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
        Assert.That(responseData.Msg,Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));

    }
    [Test]
    [Order(7)]
    public void DeleteNonExistingMovie_ShouldReturnBadRequest()
    {
       
        string invalidMovieId = "non-existing-id";

        var request = new RestRequest($"/api/Movie/Delete?movieId={invalidMovieId}", Method.Delete);


        RestResponse response = client.Execute(request);

        Assert.That(response.StatusCode,Is.EqualTo(HttpStatusCode.BadRequest));
        ApiResponseDTO responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
        Assert.That(responseData.Msg,Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
    }
}