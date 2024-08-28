using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Newtonsoft.Json;
using WorkFlowService.Helpers;
public class Program
{
    private static readonly AmazonEventBridgeClient client = new AmazonEventBridgeClient(Amazon.RegionEndpoint.USEast1);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        SendAdaptiveCardTeamsNotification();

        app.Run();
    }

    private static async Task SendAdaptiveCardTeamsNotification()
    {
        var adaptiveCardJsonobject = System.IO.File.ReadAllText("AdaptiveCard.json");
        string eventId = EventIdGenerator.GenerateEventId();

        var adaptiveCardJson = new
        {
            eventId = eventId,
            adaptiveCardJson = adaptiveCardJsonobject
        };

        var sss = JsonConvert.SerializeObject(adaptiveCardJson);

        var request = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        Source = "t.notify",
                        DetailType = "TNotify",
                        Detail = JsonConvert.SerializeObject(adaptiveCardJson),
                        EventBusName = "arn:aws:events:us-east-1:010928217401:event-bus/RE_teams_EventBus",

                    }
                }
        };


        try
        {
            var response = await client.PutEventsAsync(request);
            Console.WriteLine($"Event published. Result: {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing event: {ex.Message}");
        }
    }

    public static async Task<PutEventsResponse> EvtHandler(IDictionary<string, object> eventData)
    {
        // Extract the id from the 'api_url' field
        var apiUrl = "https://dummyjson.com/products/1"; // eventData["api_url"].ToString(); 
                                                         //var apiUrl = "https://dummyjson.com/products/1";
        if (string.IsNullOrEmpty(apiUrl) || apiUrl.Length < 1)
        {
            throw new ArgumentException("Invalid api_url provided");
        }
        //var id = apiUrl[^1]; // Get the last character

        // Add 'id' to the eventData
        //eventData["id"] = 1;

        // Prepare the event details
        var putEventsRequest = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        Source = "microservice.request",
                        DetailType = "RequestEvent",
                        Detail = JsonConvert.SerializeObject(eventData),
                        EventBusName = "arn:aws:events:us-east-1:010928217401:event-bus/RE_teams_EventBus"
                    }
                }
        };

        // Send the event
        var response = await client.PutEventsAsync(putEventsRequest);

        return response;
    }

    public static async Task ApigateWayHandler()
    {
        var apiGatewayUrl = "https://7sgeufzpij.execute-api.us-east-1.amazonaws.com/stage2/products";
        HttpClient httpClient = new HttpClient();
        try
        {
            // Make the HTTP GET request to the API Gateway
            HttpResponseMessage response = await httpClient.GetAsync(apiGatewayUrl);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // Read the response content
            string responseBody = await response.Content.ReadAsStringAsync();

            // Output the response
            Console.WriteLine("Response from API Gateway:");
            Console.WriteLine(responseBody);
        }
        catch (HttpRequestException e)
        {
            // Handle request errors
            Console.WriteLine($"Request error: {e.Message}");
        }
    }

    public static async Task SendTeamsNotification()
    {
        //var putRuleRequest = new PutRuleRequest
        //{
        //    Name = "teams-notification-lambda",
        //    EventPattern = @"{
        //                      ""source"": [""teams.notification""],
        //                      ""detail-type"": [""Teams Notification""],
        //                      ""detail"": {
        //                        ""text"": [""Hi, Message""]
        //                      }
        //                    }",
        //    State = RuleState.ENABLED,
        //    Description = "Rule to match 'Teams Notification' events from 'teams.notification'"
        //};
        /*

                    Source = "teams.notification",
                    DetailType = "TeamsNotification",
                    Detail = JsonConvert.SerializeObject(details),
                    EventBusName = "arn:aws:events:us-east-1:010928217401:event-bus/RE_teams_EventBus",

                }
         */
        var details = new Dictionary<string, string>();
        details.Add("text", "Welcome Real Estate Manager");
        var request = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        Source = "t.notify",
                        DetailType = "TNotify",
                        Detail = JsonConvert.SerializeObject(details),
                        EventBusName = "arn:aws:events:us-east-1:010928217401:event-bus/RE_teams_EventBus",

                    }
                }
        };


        try
        {
            var response = await client.PutEventsAsync(request);
            Console.WriteLine($"Event published. Result: {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing event: {ex.Message}");
        }
    }
}
