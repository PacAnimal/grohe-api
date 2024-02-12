namespace Application.GroheApiClasses;

// ReSharper disable once ClassNeverInstantiated.Global
public class NotificationTypes
{
    public static string GetCategoryTypeMessage(long category, long type) =>
        CategoryTypeMessageDictionary.TryGetValue(category, out var categoryTypeMessages) &&
        categoryTypeMessages.TryGetValue(type, out var message)
            ? message
            : null;

    // messages ripped from https://github.com/windkh/node-red-contrib-grohe-sense/blob/main/grohe/ondusApi.js
    // and kindly converted to C# dictionaries by ChatGPT :D
    private static readonly Dictionary<long, Dictionary<long, string>> CategoryTypeMessageDictionary = new()
    {
        { 0, new Dictionary<long, string>
            {
                { 0, "Advertising" }
            }
        },
        { 10, new Dictionary<long, string>
            {
                { 10, "Installation successful" },
                { 60, "Firmware update available" },
                { 100, "System Information [undefined]" },
                { 410, "Installation of sense guard successful" },
                { 460, "Firmware update of sense guard available" },
                { 555, "Blue: auto flush active" },
                { 556, "Blue: auto flush inactive" },
                { 557, "Catridge empty" },
                { 559, "Cleaning complete" },
                { 561, "Order fully shipped" },
                { 563, "Order fully delivered" },
                { 566, "Order partially shipped" },
                { 560, "Firmware update for blue available" },
                { 601, "Nest away mode automatic control off" },
                { 602, "Nest home mode automatic control off" },
                { 605, "Connect with your insurer" },
                { 606, "Device deactivated" }
            }
        },
        { 20, new Dictionary<long, string>
            {
                { 11, "Battery is at critical level" },
                { 12, "Battery is empty and must be changed" },
                { 20, "Temperature levels have dropped below the minimum configured limit" },
                { 21, "Temperature levels have exceeded the maximum configured limit" },
                { 30, "Humidity levels have dropped below the minimum configured limit" },
                { 31, "Humidity levels have exceeded the maximum configured limit" },
                { 40, "Frost warning!" },
                { 80, "Sense  lost WiFi" },
                { 320, "Unusual water consumption detected - water has been SHUT OFF" },
                { 321, "Unusual water consumption detected - water still ON" },
                { 330, "Pressure drop detected during check of household water pipes" },
                { 332, "Watersystem check not possible" },
                { 340, "Frost warning! Current temperature is" },
                { 380, "Sense guard lost WiFi" },
                { 420, "Multiple water pressure drops detected - water supply switched off" },
                { 421, "Multiple water pressure drops detected" },
                { 550, "Blue filter low" },
                { 551, "Blue CO2 low" },
                { 552, "Blue empty filter" },
                { 553, "Blue empty CO2" },
                { 558, "Cleaning" },
                { 564, "Filter stock empty" },
                { 565, "CO2 stock empty" },
                { 580, "Blue no connection" },
                { 603, "GROHE Sense Guard did not respond – valve open" },
                { 604, "GROHE Sense Guard did not respond – valve closed" }
            }
        },
        { 30, new Dictionary<long, string>
            {
                { 0, "Flooding detected - water has been SHUT OFF" },
                { 50, "Sensor error 50" },
                { 90, "System error 90" },
                { 100, "System error 100" },
                { 101, "RTC error" },
                { 102, "Acceleration sensor" },
                { 103, "System out of service" },
                { 104, "System memory error" },
                { 105, "System relative temperature" },
                { 106, "System water detection error" },
                { 107, "System button error" },
                { 310, "Extremely high flow rate - water supply switched off" },
                { 390, "System error 390" },
                { 400, "Maximum volume reached — water supply switched off" },
                { 430, "Water detected by GROHE Sense - water supply switched off" },
                { 431, "Water detected by GROHE Sense" }
            }
        },
        { 40, new Dictionary<long, string>
            {
                { 1, "Web URL" }
            }
        }
    };   
}