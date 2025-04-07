using System.Net.Http.Headers;
using System.Text.Json;

// Helper classes to deserialize People API response.
public class GooglePeopleApiResponse
{
    public Birthday[] Birthdays { get; set; }
    public Gender[] Genders { get; set; }
    public PhoneNumber[] PhoneNumbers { get; set; }
    public Address[] Addresses { get; set; }
}

public class Birthday
{
    public DateObj Date { get; set; }
}

public class DateObj
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
}

public class Gender
{
    public string Value { get; set; }
}

public class PhoneNumber
{
    public string Value { get; set; }
}

public class Address
{
    public string FormattedValue { get; set; }
}

// Class to hold the additional info we need.
public class GoogleAdditionalUserInfo
{
    public DateOnly? DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

// Method to call the People API and extract additional info.
public class GooglePeopleApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GooglePeopleApiHelper> _logger;

    public GooglePeopleApiHelper(ILogger<GooglePeopleApiHelper> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GoogleAdditionalUserInfo?> GetGoogleAdditionalUserInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var url = "https://people.googleapis.com/v1/people/me?personFields=birthdays,genders,phoneNumbers,addresses";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve additional user info: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<GooglePeopleApiResponse>(json, options);

            if (data == null) return null;

            var additionalInfo = new GoogleAdditionalUserInfo();

            // Extract birthday (first valid entry)
            if (data.Birthdays != null)
            {
                foreach (var birthday in data.Birthdays)
                {
                    if (birthday.Date != null &&
                        birthday.Date.Year > 0 &&
                        birthday.Date.Month > 0 &&
                        birthday.Date.Day > 0)
                    {
                        additionalInfo.DateOfBirth = new DateOnly(birthday.Date.Year, birthday.Date.Month, birthday.Date.Day);
                        break;
                    }
                }
            }

            // Extract gender (first entry if available)
            if (data.Genders != null && data.Genders.Length > 0)
            {
                additionalInfo.Gender = data.Genders[0].Value;
            }

            // Extract phone number (first entry if available)
            if (data.PhoneNumbers != null && data.PhoneNumbers.Length > 0)
            {
                additionalInfo.PhoneNumber = data.PhoneNumbers[0].Value;
            }

            // Extract address (first entry if available)
            if (data.Addresses != null && data.Addresses.Length > 0)
            {
                additionalInfo.Address = data.Addresses[0].FormattedValue;
            }

            return additionalInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while retrieving additional user info: {ex.Message}");
            return null;
        }
    }
}
