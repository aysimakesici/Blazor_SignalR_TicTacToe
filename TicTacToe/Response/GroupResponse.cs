using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Hubs;

namespace TicTacToe.Response
{
    public class GroupResponse
    {
        
            public string ClientId { get; set; }
            public string GroupName { get; set; }
            public bool ClienInGroup { get; set; }
            public List<User> members { get; set; }

        }
    }



