using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using OrleansWorkshop;

namespace Grains
{
    [StorageProvider(ProviderName = "Storage")]
    public class UserGrain : Grain<UserProperties>, IUser
    {
        public override Task OnActivateAsync()
        {
            random = new Random(this.GetHashCode());
            //RegisterTimer(OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            RegisterOrUpdateReminder("poke", TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            return base.OnActivateAsync();
        }

        private Random random;

        private async Task OnTimer(object state)
        {
            if (State.Friends.Count > 0)
            {
                var friend = State.Friends.ToList()[random.Next(State.Friends.Count)];
                await friend.Poke(this,"I'm bored!");
            }
        }

        public Task Poke(IUser user, string message)
        {
            Console.WriteLine($"[{this.GetPrimaryKeyString()}] User {user.GetPrimaryKeyString()} poked me with '{message}'");
            return Task.CompletedTask;
        }

        public Task SetName(string name)
        {
            State.Name = name;
            return WriteStateAsync();
        }

        public Task SetStatus(string status)
        {
            State.Status = status;
            return WriteStateAsync();
        }

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(State);
        }

        public async Task<bool> InviteFriend(IUser user)
        {
            if (!State.Friends.Contains(user))
                State.Friends.Add(user);
            await WriteStateAsync();
            return true;
        }

        public async Task<bool> AddFriend(IUser user)
        {
            var t1 = Thread.CurrentThread.Name;
            var ok = await user.InviteFriend(this);
            if (ok == false)
                return false;

            if (!State.Friends.Contains(user))
                State.Friends.Add(user);

            var t2 = Thread.CurrentThread.Name;

            if(t1!=t2)
                Console.WriteLine($"Switched thread from {t1} to {t2}");

            await WriteStateAsync();
            return true;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return OnTimer(null);
        }
    }
}
