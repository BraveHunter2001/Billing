using Billing;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Runtime.CompilerServices;

namespace ClinetBillingTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var channel = GrpcChannel.ForAddress("http://localhost:5069");
            var client = new Billing.Billing.BillingClient(channel);

            PrintUser(client);
            Console.WriteLine();

            EmissionAmount emissionAmount = new EmissionAmount {Amount = 10 };
            var response = await client.CoinsEmissionAsync(emissionAmount);
            Console.WriteLine(response.Comment);
            Console.WriteLine();

            PrintUser(client);
            Console.WriteLine();

            MoveCoinsTransaction transaction = new MoveCoinsTransaction
            {
                Amount = 5,
                DstUser = "maria",
                SrcUser = "boris"
            };

            var responseMove = await client.MoveCoinsAsync(transaction);
            Console.WriteLine(responseMove.Comment);

            PrintUser(client);
            Console.WriteLine();

            var coin = await client.LongestHistoryCoinAsync(new None());
            Console.WriteLine($"{coin.Id} {coin.History}");
            Console.ReadLine();

        }

        static async void PrintUser(Billing.Billing.BillingClient client)
        {
            None none = new None();
            using (var call = client.ListUsers(none))
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var user = call.ResponseStream.Current;
                    Console.WriteLine($"{user.Name} {user.Amount}");
                }
            }
        }
    }
}