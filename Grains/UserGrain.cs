using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using OrleansWorkshop;

namespace Grains
{
    public class UserGrain : Grain, IUser
    {
        private UserProperties _props = new UserProperties();
        public Task SetName(string name)
        {
            _props.Name = name;
            return Task.CompletedTask;
        }

        public Task SetStatus(string status)
        {
            _props.Status = status;
            return Task.CompletedTask;
        }

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(_props);
        }

        public Task<bool> InviteFriend(IUser user)
        {
            if (!_props.Friends.Contains(user))
                _props.Friends.Add(user);
            return Task.FromResult(true);
        }

        public async Task<bool> AddFriend(IUser user)
        {
            var t1 = Thread.CurrentThread.Name;
            var ok = await user.InviteFriend(this);
            if (ok == false)
                return false;

            if (!_props.Friends.Contains(user))
                _props.Friends.Add(user);

            var t2 = Thread.CurrentThread.Name;

            if(t1!=t2)
                Console.WriteLine($"Switched thread from {t1} to {t2}");

            return true;
        }
    }
}
