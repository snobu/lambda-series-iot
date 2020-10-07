using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

class Program
{
    public static string generateSasToken(string resourceUri, string key, int expiryInSeconds = 3600)
    {
        TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
        string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + expiryInSeconds);

        string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;
        Console.WriteLine($"String to sign: \n    {WebUtility.UrlEncode(resourceUri)} + \\n + {expiry}\n");

        HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(key));
        string signature = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

        string token = String.Format(
            CultureInfo.InvariantCulture,
            "SharedAccessSignature sr={0}&sig={1}&se={2}",
            WebUtility.UrlEncode(resourceUri),
            WebUtility.UrlEncode(signature),
            expiry);

        return token;
    }

    public static void Main()
    {
        string sas = generateSasToken("poorlyfundedskynet.azure-devices.net/devices/yolo", "DevicePrimaryKeyGoesHere=");
        Console.WriteLine(sas);
    }
}
