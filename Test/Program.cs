using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using OrleansDashboard;
using OrleansWorkshop;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
			var clusterId = Guid.NewGuid().ToString();
			StartAzureTableStorageSilo(1,clusterId);
            StartAzureTableStorageSilo(2,clusterId);
			
            var client = InitAzureClient(clusterId);//InitLocalStorageAndClient();
			Console.WriteLine("Client started...");

            Test(client).Wait();

            Console.WriteLine("Write Enter to close");
            Console.ReadLine();
        }



        private static IClusterClient InitAzureClient(string clusterId)
        {
            var clientConfiguration = ClientConfiguration.LocalhostSilo();
            clientConfiguration.GatewayProvider=ClientConfiguration.GatewayProviderType.AzureTable;
            clientConfiguration.DataConnectionString = azureConnectionString;
            clientConfiguration.DeploymentId = clusterId;
            var client = new ClientBuilder().UseConfiguration(clientConfiguration).Build();
            client.Connect().Wait();
            return client;
        }


        private  const string azureConnectionString = "UseDevelopmentStorage=true";
        public static void StartAzureTableStorageSilo(int index, string cluserId)
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo(11110+index,29999+index);
            siloConfig.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;
            siloConfig.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
            siloConfig.Globals.DataConnectionString = azureConnectionString;
            siloConfig.Globals.DeploymentId = cluserId;
            siloConfig.AddAzureTableStorageProvider("Storage");

            if(index==1)
                siloConfig.Globals.RegisterDashboard();

            siloConfig.Defaults.DefaultTraceLevel = Severity.Warning;


            var silo = new SiloHost("Test silo", siloConfig);
            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();

            Console.WriteLine($"Silo {index} started...");
            //return client;
        }

        private static IClusterClient InitLocalStorageAndClient()
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo();
            siloConfig.Defaults.DefaultTraceLevel = Severity.Warning;


            var silo = new SiloHost("Test silo", siloConfig);
            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();

            Console.WriteLine("Silo started...");

            var clientConfiguration = ClientConfiguration.LocalhostSilo();
            var client = new ClientBuilder().UseConfiguration(clientConfiguration).Build();
            client.Connect().Wait();
            return client;
        }


        public static async Task PopulateUsers(IClusterClient client, IUser mark, IUser jack)
        {
            await mark.SetName("Mark");
            await mark.SetStatus("Share your life with me!");
            
            await jack.SetName("Jack");
            await jack.SetStatus("Tweet me!");



            var ok = await mark.AddFriend(jack);

            if (ok)
                Console.WriteLine("Mark added Jack as friend");

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                await user.SetName($"User #{i}");
                await user.SetStatus((i % 3 == 0) ? "Sad" : "Happy!");
                await ((i % 2 == 0) ? mark : jack).AddFriend(user);
                var p = await user.GetProperties();
                //Console.WriteLine(p);
            }
            sw.Stop();

            Console.WriteLine($"Serial execution: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var tasks = new List<Task>();
            for (int i = 100; i < 200; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                tasks.Add(user.SetName($"User #{i}"));
                tasks.Add(user.SetStatus((i % 3 == 0) ? "Sad" : "Happy!"));
                tasks.Add(((i % 2 == 0) ? mark : jack).AddFriend(user));
                //tasks.Add(user.GetProperties());
            }
            await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine($"Parallel execution: {sw.ElapsedMilliseconds}");

        }

        public static async Task Test(IClusterClient client)
        {
            var mark = client.GetGrain<IUser>("mark@fb.com");

            var jack = client.GetGrain<IUser>("jack@twitter.com");

            //await PopulateUsers(client, mark, jack);


            



            var props = await mark.GetProperties();
            Console.WriteLine($"Mark: {props}");


            props = await jack.GetProperties();
            Console.WriteLine($"Jack: {props}");
        }
        
    }
}
