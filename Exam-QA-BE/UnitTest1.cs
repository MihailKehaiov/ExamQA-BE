using Exam_QA_BE.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Exam_QA_BE
{
    [TestFixture]
    public class Tests
    {
        private RestClient _client;
        private static string createdId;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/";
      //  private const string StaticToken = "00-9d5fb9bcb7771f79ae7f42959d51c1c2-f7ce004fb9f46c51-00";
       // private const string LoginEmail = "mishok1@mishok1.com";
      //  private const string LoginPassword = "mishok12";


        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken ("mishok1", "mishok12"); 

            var option = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            _client = new RestClient(option);

        }

        [Test, Order(1)]

        public void CreateStorySpoiler_WithRequiredFields_ShouldReturnCreaed()
        {
            var storyRequest = new Story_DTO
            {
                Title = "Test Story",
                Description = "This is a test story description.",
                Url = ""
            };
            
                var request = new RestRequest("/api/Story/Create", Method.Post);
                request.AddJsonBody(storyRequest);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));


            var createResponse = JsonSerializer.Deserialize<ApiResponse_DTO>(response.Content);
            Assert.That(response.Content, Does.Contain("Successfully created!"));


            createdId = createResponse.StoryId;

            //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
           // var createResponse = JsonSerializer.Deserialize<ApiResponse_DTO>(response.Content, options);

           // Assert.That(createResponse.Msg, Does.Contain("Successfully created!"));
            //createdId = createResponse.StoryId;
        }

        [Test, Order(2)]

        public void EditStorySpoiler_ShouldReturnOk()
        {
            var editStoryRequest = new Story_DTO
            {
                Title = "Edited Story",
                Description = "This is an edited story description.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdId}", Method.Put);
            //request.AddQueryParameter("storyId", createdId);
            request.AddJsonBody(editStoryRequest);

            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


            var editResponse = JsonSerializer.Deserialize<ApiResponse_DTO>(response.Content);
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));

        }

        [Test, Order(3)]

        public void GetAllStorySpoilers_ShouldReturnList()
        {
            var reqiest = new RestRequest("/api/Story/All", Method.Get);
            var response = _client.Execute(reqiest);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var storySpoilers = JsonSerializer.Deserialize<List<Story_DTO>>(response.Content);

            Assert.That(storySpoilers, Is.Not.Empty);
        }


        [Test, Order(4)]

        public void DeleteStorySpoilers_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdId}", Method.Delete);
            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test, Order(5)] 
        public void CreateStorySpoiler_WithMissingFields_ShouldReturnBadRequest()
        {
            var storyRequest = new Story_DTO
            {
                Title = "",
                Description = "",
                Url = "" 
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
           
        }

        [Test, Order(6)]
        public void EditNonExistentStorySpoiler_ShouldReturnNotFound()
        {
            var editStoryRequest = new Story_DTO
            {
                Title = "Edited Non-Existent Story",
                Description = "This is an edited non-existent story description.",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Edit/nonexistent-id", Method.Put);
            request.AddJsonBody(editStoryRequest);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistentStorySpoiler_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Story/Delete/nonexistent-id", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }













        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(BaseUrl);
            var request = new RestRequest("api/User/Authentication", Method.Post); 

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [OneTimeTearDown]
        public void Cleanup() 
        { 
            _client?.Dispose();
        }
    }
}