using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConcursRestTest
{
    /// <summary>
    /// C# REST Client test for Participant API
    /// </summary>
    public class ParticipantRestClientTest
    {
        private readonly HttpClient httpClient;
        private const string BaseUrl = "http://localhost:8080/api/participants";
        
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ParticipantRestClientTest()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public static async Task Main(string[] args)
        {
            var test = new ParticipantRestClientTest();
            await test.RunAllTestsAsync();
        }

        public async Task RunAllTestsAsync()
        {
            Console.WriteLine("Starting REST API tests for Participant");
            
            try
            {
                
                await TestGetAllParticipantsAsync();
                
                
                int participantId = await TestCreateParticipantAsync();
                
               
                await TestGetParticipantByIdAsync(participantId);
                
               
                await TestUpdateParticipantAsync(participantId);
                
                
                await TestGetAllParticipantsAsync();
                
                
                await TestDeleteParticipantAsync(participantId);
                
                
                await TestGetNonExistentParticipantAsync(participantId);
                
                
                await TestValidationErrorAsync();
                
                Console.WriteLine("All tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        private async Task TestGetAllParticipantsAsync()
        {
            Console.WriteLine("Testing GET /api/participants");
            
            var response = await httpClient.GetAsync(BaseUrl);
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response body: {content}");
            
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Expected 200 OK, got {response.StatusCode}");
            }
        }

        private async Task<int> TestCreateParticipantAsync()
        {
            Console.WriteLine("Testing POST /api/participants");
            
            var newParticipant = new ParticipantRequest
            {
                Name = "John Doe",
                Age = 25
            };
            
            var json = JsonSerializer.Serialize(newParticipant, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(BaseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response body: {responseBody}");
            
            if (response.Headers.Location != null)
            {
                Console.WriteLine($"Location header: {response.Headers.Location}");
            }
            
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception($"Expected 201 Created, got {response.StatusCode}");
            }
            
            var participant = JsonSerializer.Deserialize<ParticipantResponse>(responseBody, jsonOptions);
            return participant.Id;
        }

        private async Task TestGetParticipantByIdAsync(int id)
        {
            Console.WriteLine($"Testing GET /api/participants/{id}");
            
            var response = await httpClient.GetAsync($"{BaseUrl}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response body: {content}");
            
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Expected 200 OK, got {response.StatusCode}");
            }
        }

        private async Task TestUpdateParticipantAsync(int id)
        {
            Console.WriteLine($"Testing PUT /api/participants/{id}");
            
            var updatedParticipant = new ParticipantRequest
            {
                Name = "Jane Doe",
                Age = 30
            };
            
            var json = JsonSerializer.Serialize(updatedParticipant, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PutAsync($"{BaseUrl}/{id}", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response body: {responseBody}");
            
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Expected 200 OK, got {response.StatusCode}");
            }
        }

        private async Task TestDeleteParticipantAsync(int id)
        {
            Console.WriteLine($"Testing DELETE /api/participants/{id}");
            
            var response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception($"Expected 204 No Content, got {response.StatusCode}");
            }
        }

        private async Task TestGetNonExistentParticipantAsync(int id)
        {
            Console.WriteLine($"Testing GET /api/participants/{id} (should not exist)");
            
            var response = await httpClient.GetAsync($"{BaseUrl}/{id}");
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"Expected 404 Not Found, got {response.StatusCode}");
            }
        }

        private async Task TestValidationErrorAsync()
        {
            Console.WriteLine("Testing POST /api/participants with invalid data");
            
            var invalidParticipant = new ParticipantRequest
            {
                Name = "", // Invalid name
                Age = -5   // Invalid age
            };
            
            var json = JsonSerializer.Serialize(invalidParticipant, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(BaseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response body: {responseBody}");
            
            if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
            {
                throw new Exception($"Expected 400 Bad Request, got {response.StatusCode}");
            }
        }
    }

    // DTO Classes
    public class ParticipantRequest
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ParticipantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}