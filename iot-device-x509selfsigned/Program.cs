﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace iot_device_x509selfsigned
{
    class Program
    {
        // You probably want something like dotenv here, don't hardcode secrets in,
        // do what i say not what i do :)
        // Also you you can get away with using RegistryReadWrite connection string
        // here, now hub owner.
        private static string hubConnStr = "HostName=poorlyfundedskynet.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=YourIoTHubKeyGoesHere=";
        private static string hubUrl = "poorlyfundedskynet.azure-devices.net";
        private static string deviceId = "x509-selfsigned-device";

        public static async Task Main(string[] args)
        {
            CreateSelfSignedDevice(TransportType.Amqp, deviceId);
            await ReceiveC2dAsync();
        }

        public static DeviceClient CreateSelfSignedDevice(TransportType transport, string deviceId)
        {
            Console.Write("Connecting to hub....  ");
            var registryManager = RegistryManager.CreateFromConnectionString(hubConnStr);
            Console.WriteLine("done");
            Console.Write("Creating device with self signed key....  ");

            var certificate = GetSelfSigned();
            var device = new Microsoft.Azure.Devices.Device(deviceId)
            {
                Authentication = new AuthenticationMechanism
                {
                    X509Thumbprint = new X509Thumbprint
                    {
                        PrimaryThumbprint = certificate.Thumbprint
                    }
                }
            };

            var createdDevice = registryManager.AddDeviceAsync(device).Result;
            Console.WriteLine($"done. Id='{deviceId}', X509Thumbprint = '{createdDevice.Authentication.X509Thumbprint.PrimaryThumbprint}'");
            Console.Write("Connecting to device...  ");
            var auth = new DeviceAuthenticationWithX509Certificate(deviceId, certificate);
            var client = DeviceClient.Create(hubUrl, auth, transport);
            Console.WriteLine("done");

            return client;
        }

        public static X509Certificate2 GetSelfSigned()
        {
            string thisAssembly = new DirectoryInfo(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
            string directory = new FileInfo(thisAssembly).Directory.FullName;
            //Console.WriteLine($"Assembly and work directory:\n--{thisAssembly}\n--{directory}");
            X509Certificate2 certificate = new X509Certificate2(Path.Combine(directory, "certs/selfsigned.pfx"), "azure");
            //var certificate = new X509Certificate2(Path.Combine(directory, "certs/malicecert.pfx"), "azure");

            return certificate;
        }

        public static DeviceClient CreateClient(TransportType transport, string deviceId)
        {
            Console.WriteLine("Connecting to IoT Hub with certificate:");
            var certificate = GetSelfSigned();
            DeviceAuthenticationWithX509Certificate auth = new DeviceAuthenticationWithX509Certificate(deviceId, certificate);
            Console.WriteLine($"----------\n{auth.Certificate}----------");
            DeviceClient client = DeviceClient.Create(hubUrl, auth, transport);

            return client;
        }

        private static async Task ReceiveC2dAsync()
        {
            DeviceClient client = CreateClient(TransportType.Amqp, deviceId);
            Console.WriteLine("Waiting for cloud to device messages...");
            while (true)
            {
                Microsoft.Azure.Devices.Client.Message receivedMessage = await client.ReceiveAsync();
                if (receivedMessage == null) continue;
                string receivedMessageText = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine($"\nGot {receivedMessage}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Received message: {receivedMessageText}");
                if (receivedMessage.Properties.Count > 0)
                {
                    Console.WriteLine($"  Props: ");
                    foreach (var prop in receivedMessage.Properties)
                    {
                        Console.WriteLine($"    {prop}");
                    }
                }
                Console.ResetColor();

                await client.CompleteAsync(receivedMessage);
            }
        }

    }
}
