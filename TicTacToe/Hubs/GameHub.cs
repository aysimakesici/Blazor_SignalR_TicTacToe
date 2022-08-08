using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Extensions;
using TicTacToe.Helper;
using TicTacToe.Response;
using static TicTacToe.Enums.groupEnum;


namespace TicTacToe.Hubs
{
    public class GameHub : Hub
    {
        public static List<Group> groups = new List<Group>();
        public static List<User> clients = new List<User>();
        readonly GameModelService _service = new GameModelService();
        const string O = "O";
        const string X = "X";


        public async Task NewHumanGame(GameModel gamesettings)
        {
            gamesettings = await _service.GetHumanGameBoardAsync();
            await Clients.All.SendAsync(ClientEndpoints.NewHumanGame, gamesettings);
           
        }

       
     
        public override Task OnConnectedAsync()
        {
            User newUser = new User(Context.ConnectionId);
            clients.Add(newUser);
            return base.OnConnectedAsync();

        }




        public async Task AddGroup(string connectionId, string groupName)
        {
            var groupAlreadyExists = true;
            if (groups.Count >= (int)GroupEnum.maxGroupCount)
            {
                await Clients.All.SendAsync("groupLimitReached");
                return;
            }
            // if there isn't already a group with that groupName
            // create group and add the creator to the group
            if (!GroupHelper.GroupExists(groups, groupName))
            {
                groupAlreadyExists = false;
                await Groups.AddToGroupAsync(connectionId, groupName);
                groups = GroupHelper.AddGroup(groups, groupName, clients, connectionId);

            }
            await Clients.All.SendAsync("checkAddGroup", groupAlreadyExists, groupName, connectionId);
            List<string> groupNameList = new List<string>();
            //Group grp = new Group(groupName, connectionId);
            //Group grp2 = new Group("3", "4");          
            //groups.Add(grp);
            //groupNameList.Add(grp2.groupName);
            foreach (var group in groups)
            {
                groupNameList.Add(group.groupName);
            }
            
            await Clients.All.SendAsync("ListGroup", groupNameList);
            Console.WriteLine("groups");
           

        }
        public async Task JoinGroup(string connectionId, string groupName)
        {
            GroupResponse response = new GroupResponse();
            User usr = new User();
            var theGroup = groups.First();
            if (GroupHelper.GroupExists(groups, groupName)) // if there's a group with that groupName :: aslında gerekli degil ama her ihtimale karsı
            {
                usr = UserHelper.FindUser(clients, connectionId);
                theGroup = GroupHelper.FindGroup(groups, groupName);

                response = new GroupResponse { ClientId = connectionId, GroupName = groupName, members = theGroup.members, ClienInGroup = false };

                // if the user is not already in the group, add the user to the group

                if (!UserHelper.UserExists(theGroup.members, usr))
                {
                    //await Groups.AddToGroupAsync(connectionId, groupName);
                    theGroup.members.Add(usr);
                    response.ClienInGroup = true;
                }

            }

            await Clients.Caller.SendAsync("checkJoinGroup", JsonConvert.SerializeObject(response));
            await Clients.OthersInGroup(groupName).SendAsync("notificationJoinGroup", usr.Username);
        }
        public async Task LeaveGroup(string connectionId, string groupName)
        {
            User usr = new User();
            var theGroup = groups.First();
            if (GroupHelper.GroupExists(groups, groupName)) // if there's a group with that groupName
            {
                usr = UserHelper.FindUser(clients, connectionId);
                theGroup = GroupHelper.FindGroup(groups, groupName);
                if (UserHelper.UserExists(theGroup.members, usr))
                {
                   // await Groups.RemoveFromGroupAsync(connectionId, groupName);
                    theGroup.members.Remove(usr);
                }

            }
            await Clients.Caller.SendAsync("checkLeaveGroup", groupName);
            // there should be another function 
            //await Clients.Group(groupName).SendAsync("notificationJoinGroup", usr.Username);
        }

        public async Task AddUserName(string userName, string connectionId)
        {
            var client = clients.FirstOrDefault(o => o.ConnectionId == connectionId);
            if (string.IsNullOrEmpty(userName) || client == null || clients.Where(o => o.Username == userName).Count() > 0)
            {
                await Clients.Caller.SendAsync("checkUserName", userName);
                return;
            };
            client.Username = userName;

            await Clients.Caller.SendAsync("userJoined", userName);
            await Clients.All.SendAsync("clients", clients.Where(o => o.Username != null).Select(o => o.Username));
            await Clients.Others.SendAsync("notifyUserJoined", userName);

        }


        public async Task NewBotGame(GameModel gamesettings)
        {
            gamesettings = await _service.GetBotGameBoardAsync();
            await Clients.All.SendAsync(ClientEndpoints.NewBotGame, gamesettings);
        }

        public async Task SendMessage(GameModel gamesettings)
        {
            await Clients.All.SendAsync(ClientEndpoints.ReceiveMessage, gamesettings);
        }

        public async Task TakePlayerTurn(int i, GameModel gamesettings)
        { 
            if (gamesettings.PlayerXTurn)
            {
                gamesettings.Game[i] = X;
            }
            else if (gamesettings.PlayerOTurn)
            {
                gamesettings.Game[i] = O;
            }
            await EndPlayerTurn(gamesettings);
        }


        private async Task EndPlayerTurn(GameModel gamesettings)
        {
            await HandleGameOver(gamesettings);
            if (!gamesettings.Gameover)
            {
                gamesettings.TurnCount++;
                gamesettings.PlayerXTurn = !gamesettings.PlayerXTurn;
                gamesettings.PlayerOTurn = !gamesettings.PlayerOTurn;
                if (gamesettings.Botgame)
                    gamesettings.Botturn = !gamesettings.Botturn;
                await Clients.All.SendAsync(ClientEndpoints.EndTurn, gamesettings);
            }
        }

        public async Task TakeBotTurn(GameModel gamesettings)
        {
            var rng = new Random();
            Thread.Sleep(rng.Next(100, 3000));

            // 0 1 2
            // 3 4 5
            // 6 7 8
            bool shouldSetMiddle = string.IsNullOrEmpty(gamesettings.Game[4]) || gamesettings.Game.ShouldSetMiddle();
            if (shouldSetMiddle)
            {
                gamesettings.Game[4] = O;
            }
            else if (gamesettings.Game.ShouldSetUpperLeft())
                gamesettings.Game[0] = O;
            else if (gamesettings.Game.ShouldSetUpperMiddle())
                gamesettings.Game[1] = O;
            else if (gamesettings.Game.ShouldSetUpperRight())
                gamesettings.Game[2] = O;
            else if (gamesettings.Game.ShouldSetLeftMiddle())
                gamesettings.Game[3] = O;
            else if (gamesettings.Game.ShouldSetRightMiddle())
                gamesettings.Game[5] = O;
            else if (gamesettings.Game.ShouldSetLowerLeft())
                gamesettings.Game[6] = O;
            else if (gamesettings.Game.ShouldSetLowerMiddle())
                gamesettings.Game[7] = O;
            else if (gamesettings.Game.ShouldSetLowerRight())
                gamesettings.Game[8] = O;
            else
            {
                var randomEmpty = Array.IndexOf(gamesettings.Game, string.Empty, rng.Next(0, 8));
                gamesettings.Game[randomEmpty] = O;
            }

            await EndPlayerTurn(gamesettings);
        }


        private async Task HandleGameOver(GameModel gamesettings)
        {
            if (gamesettings.Game.XWins())
            {
                gamesettings.PlayerXWins = new MarkupString("<strong>Game Over:</strong> Player X has won the game!");
                gamesettings.XWon = true;
                gamesettings.Gameover = true;
            }
            else if (gamesettings.Game.OWins())
            {
                gamesettings.PlayerOWins = new MarkupString("<strong>Game Over:</strong> Player O has won the game!");
                gamesettings.OWon = true;
                gamesettings.Gameover = true;
            }
            else if (gamesettings.Game.CatGame())
            {
                gamesettings.PlayerCatGame = new MarkupString("<strong>Game Over:</strong> Cat game! Everyone wins!");
                gamesettings.CatGame = true;
                gamesettings.Gameover = true;
            }

            if (gamesettings.Gameover)
            {
                gamesettings.PlayerXTurn = false;
                gamesettings.PlayerOTurn = false;
                gamesettings.Botturn = false;
                await Clients.All.SendAsync("GameOver", gamesettings);
            }              
        }
    }
   
}
