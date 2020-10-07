using System;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using SolTechnology.Avro;
using MsgPack.Serialization;
using System.Security.Cryptography;

namespace yolo
{
    public class Program
    {
        // Your DeviceId goes here
        public static string deviceId = "yolo";
        public static string iotHubName = "poorlyfundedskynet";
        public static string primaryKey = "DevicePrimaryKeyGoesHere=";
        // And your device connection string here
        public static string connStr = $"HostName={iotHubName}.azure-devices.net;DeviceId={deviceId};SharedAccessKey={primaryKey}";

        // Pick your transport here
        public static TransportType transport = TransportType.Amqp;

        public class TelemetryPoint
        {
            public string deviceId { get; set; }
            public int vibrationLevel { get; set; }
        }

        static async Task Main(string[] args)
        {
            DeviceClient deviceClient =
                DeviceClient.CreateFromConnectionString(connStr, transport);
            await deviceClient.OpenAsync();
            Console.WriteLine("Connected to Azure IoT Hub.");

            // Fake vibration reading
            Random rnd = new Random();
            int vibrationReading = rnd.Next(2, 10);

            TelemetryPoint payload = new TelemetryPoint
            {
                deviceId = deviceId,
                vibrationLevel = vibrationReading
            };

            // Serialize as JSON
            Message message =
                new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(payload)));
            message.Properties.Add("encoding", "json");

            // Serialize as AVRO
            //
            // byte[] avroObject = AvroConvert.Serialize(payload);
            // Message message = new Message(avroObject);
            // message.Properties.Add("PayloadEncoding", "avro");

            // Serialize as MessagePack
            //
            // var serializer = MessagePackSerializer.Get<TelemetryPoint>();
            // var ms = new MemoryStream();
            // serializer.Pack(ms, payload);
            // Message message = new Message(ms.ToArray());
            // message.Properties.Add("PayloadEncoding", "messagepack");

            // Need Encrypted Payload?
            // ========= WARNING ==========
            // NOT FOR PRODUCTION USE
            // USE AS PROOF OF CONCEPT ONLY
            // ============================
            // string encryptedPayload = EncryptString(
            //      "TSLA price tomorrow based on board room sentiment (insider trading?): $49400",
            //      "Sup3Sup3rS3cre7keYSup3rS3cre7keY");
            // Message message = new Message(Encoding.ASCII.GetBytes(encryptedPayload));
            // message.Properties.Add("PayloadEncrypted", "true");
            // message.Properties.Add("EncryptionKeyId", "FFFE43");
            // message.Properties.Add("EncryptionKeyIV", "INIT_VECTOR_VALUE");

            await deviceClient.SendEventAsync(message);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Sent: {JsonConvert.SerializeObject(payload)}");
            Console.ResetColor();
        }

        // ========= WARNING ==========
        // NOT FOR PRODUCTION USE
        // USE AS PROOF OF CONCEPT ONLY
        // ============================
        public static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        byte[] iv = aesAlg.IV;
                        byte[] decryptedContent = msEncrypt.ToArray();
                        byte[] result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }
    }
}
