using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Hubs;
using TicTacToe.Helper;

namespace TicTacToe.Data
{
    public class Group
    {
        public string groupName { get; set; }
        public string createdByConnId;
        public List<User> members { get; set; } = new List<User>();

        public Group(string groupName, string createdByConnId)
        {
            this.groupName = groupName;
            this.createdByConnId = createdByConnId;
        }

        public void addMember(User user)
        {
            members.Add(user);
        }

        public void removeMember(User user)
        {
            members.Remove(user);
        }
    }
}


    
    

