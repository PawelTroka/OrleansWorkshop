using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Timers;

namespace OrleansWorkshop
{
    public class UserProperties
    {
        public HashSet<IUser> Friends { get; set; } = new HashSet<IUser>();
        public string Name { get; set; }
        public string Status { get; set; }
        public override string ToString()
        {
            string friends = string.Join(", ", Friends.Select(f => f.GetPrimaryKeyString()));

            return $"Name='{Name}' Status='{Status}' Friends='{friends}'";
        }
    }

    public interface IUser : IGrainWithStringKey, IRemindable
    {
        Task Poke(IUser user, string message);
        Task SetName(string name);
        Task SetStatus(string status);

        Task<UserProperties> GetProperties();

        Task<bool> InviteFriend(IUser user);
        Task<bool> AddFriend(IUser user);
    }
}
