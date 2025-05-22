using System.Text.RegularExpressions;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace Bus_Station_Ticket_Management.Services
{
    public interface IChatbotService
    {
        Task<string> ProcessMessage(string userMessage);
    }

    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint = "https://api.openai.com/v1/chat/completions";
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(ApplicationDbContext context, IConfiguration configuration, ILogger<ChatbotService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey is not configured");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _logger = logger;
        }

        public async Task<string> ProcessMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "I didn't catch that. Could you please repeat?";

            try
            {
                // Get available locations for context
                var locations = await _context.Locations
                    .Select(l => new { l.Name, l.Address })
                    .ToListAsync();

                // Prepare the system message with context
                var systemMessage = new
                {
                    role = "system",
                    content = $@"You are a bus station assistant. You can help with:
                                - Bus schedules
                                - Ticket prices
                                - Routes
                                - Booking tickets

                                Available locations:
                                {string.Join("\n", locations.Select(l => $"- {l.Name} ({l.Address})"))}

                                When asked about schedules, prices, or routes, extract the location name and respond with a JSON object in this format:
                                {{
                                    ""intent"": ""schedule|price|route|booking"",
                                    ""location"": ""extracted location name"",
                                    ""response"": ""your natural language response""
                                }}

                                For other queries, respond naturally without the JSON format."
                };

                // Prepare the user message
                var userMessageObj = new
                {
                    role = "user",
                    content = userMessage
                };

                // Prepare the request
                var request = new
                {
                    model = "gpt-4o-mini",
                    store = true,
                    messages = new[] { systemMessage, userMessageObj },
                    temperature = 0.7
                };

                // Send request to OpenAI
                var response = await _httpClient.PostAsync(
                    _apiEndpoint,
                    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API request failed. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
                    return "I'm having trouble connecting to my brain right now. Please try again later.";
                }


                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (responseObj?.Choices == null || !responseObj.Choices.Any())
                {
                    return "I'm not sure how to respond to that. Could you please rephrase your question?";
                }

                var assistantMessage = responseObj.Choices[0].Message.Content;

                // Try to parse as JSON if it's a query about schedules, prices, or routes
                try
                {
                    var queryResponse = JsonSerializer.Deserialize<QueryResponse>(assistantMessage);
                    if (queryResponse != null && !string.IsNullOrEmpty(queryResponse.Intent))
                    {
                        // Process the query based on intent
                        var result = queryResponse.Intent switch
                        {
                            "schedule" => await HandleScheduleQuery(queryResponse.Location),
                            "price" => await HandlePriceQuery(queryResponse.Location),
                            "route" => await HandleRouteQuery(queryResponse.Location),
                            "booking" => "To book a ticket, please visit our booking page at /Booking/Create. You can select your route, date, and number of tickets there.",
                            _ => queryResponse.Response
                        };

                        return result;
                    }
                }
                catch
                {
                    // If parsing fails, return the assistant's message as is
                    return assistantMessage;
                }

                return assistantMessage;
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error processing chat message");
                return "I encountered an error while processing your request. Please try again later.";
            }
        }

        private async Task<string> HandleScheduleQuery(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return "Could you please specify which destination you're interested in? For example: 'When is the next bus to Hanoi?'";

            var trips = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Where(t => t.Route.DestinationLocation.Name.ToLower().Contains(location.ToLower()))
                .OrderBy(t => t.DepartureTime)
                .Take(5)
                .ToListAsync();

            if (!trips.Any())
            {
                // Try to find similar locations
                var similarLocations = await _context.Locations
                    .Where(l => l.Name.ToLower().Contains(location.ToLower()))
                    .Select(l => l.Name)
                    .ToListAsync();

                if (similarLocations.Any())
                {
                    return $"I couldn't find any trips to {location}. Did you mean one of these locations?\n" +
                           string.Join("\n", similarLocations.Select(l => $"- {l}"));
                }
                return $"I couldn't find any trips to {location}. Please check the spelling or try a different location.";
            }

            var response = $"Here are the next 5 trips to {trips[0].Route.DestinationLocation.Name}:\n";
            foreach (var trip in trips)
            {
                response += $"- From {trip.Route.StartLocation.Name}: Departure at {trip.DepartureTime:g}, Arrival at {trip.ArrivalTime:g}\n";
            }
            return response;
        }

        private async Task<string> HandlePriceQuery(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return "Could you please specify which destination you're interested in? For example: 'How much is a ticket to Ho Chi Minh City?'";

            var routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .Where(r => r.DestinationLocation.Name.ToLower().Contains(location.ToLower()))
                .ToListAsync();

            if (!routes.Any())
            {
                // Try to find similar locations
                var similarLocations = await _context.Locations
                    .Where(l => l.Name.ToLower().Contains(location.ToLower()))
                    .Select(l => l.Name)
                    .ToListAsync();

                if (similarLocations.Any())
                {
                    return $"I couldn't find any routes to {location}. Did you mean one of these locations?\n" +
                           string.Join("\n", similarLocations.Select(l => $"- {l}"));
                }
                return $"I couldn't find any routes to {location}. Please check the spelling or try a different location.";
            }

            var response = $"Here are the ticket prices to {routes[0].DestinationLocation.Name}:\n";
            foreach (var route in routes)
            {
                response += $"- From {route.StartLocation.Name}: {route.Price:C}\n";
            }
            return response;
        }

        private async Task<string> HandleRouteQuery(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return "Could you please specify which destination you're interested in? For example: 'How do I get to Da Nang?'";

            var routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .Where(r => r.DestinationLocation.Name.ToLower().Contains(location.ToLower()))
                .ToListAsync();

            if (!routes.Any())
            {
                // Try to find similar locations
                var similarLocations = await _context.Locations
                    .Where(l => l.Name.ToLower().Contains(location.ToLower()))
                    .Select(l => l.Name)
                    .ToListAsync();

                if (similarLocations.Any())
                {
                    return $"I couldn't find any routes to {location}. Did you mean one of these locations?\n" +
                           string.Join("\n", similarLocations.Select(l => $"- {l}"));
                }
                return $"I couldn't find any routes to {location}. Please check the spelling or try a different location.";
            }

            var response = $"Here are the routes to {routes[0].DestinationLocation.Name}:\n";
            foreach (var route in routes)
            {
                string approximateDuration = GetApproximateDuration(route.Duration);
                response += $"- From {route.StartLocation.Name}: {approximateDuration} journey\n";
            }
            response += "\nWould you like to know the schedule or price for any of these routes?";
            return response;
        }

        private string GetApproximateDuration(double durationInSeconds)
        {
            return durationInSeconds switch
            {
                <= 3600 => "less than 1 hour",
                <= 7200 => "1 to 2 hours",
                <= 10800 => "2 to 3 hours",
                _ => "3 hours or more"
            };
        }
    }

    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }

    public class QueryResponse
    {
        public string Intent { get; set; }
        public string Location { get; set; }
        public string Response { get; set; }
    }
}