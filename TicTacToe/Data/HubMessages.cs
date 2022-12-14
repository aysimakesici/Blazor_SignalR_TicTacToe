namespace TicTacToe.Data
{
    public static class HubMessages
    {
    }

    public static class ClientEndpoints
    {
        public const string NewHumanGame = "NewHumanGame";
        public const string NewBotGame = "NewBotGame";
        public const string ReceiveMessage = "ReceiveMessage";
        public const string EndTurn = "EndTurn";
        public const string GameOver = "GameOver";
        public const string ListGroup = "ListGroup";
    }

    public static class ServerEndpoints
    {
        public const string TakePlayerTurn = "TakePlayerTurn";
        public const string TakeBotTurn = "TakeBotTurn";
        public const string NewBotGame = "NewBotGame";
        public const string NewHumanGame = "NewHumanGame";
        public const string SendMessage = "SendMessage";
        public const string AddGroup = "AddGroup";
        public const string JoinGroup = "JoinGroup";
        public const string LeaveGroup = "LeaveGroup";
     
    }
}

