using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel; // WCF types
using System.Runtime.Serialization;

namespace GameLibrary
{

    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdatePlayers(List<Player> players);
        [OperationContract(IsOneWay = true)]
        void EndGame(string name);
    }

    [ServiceContract (CallbackContract = typeof(ICallback))]
    public interface IGame
    {
        [OperationContract]
        int NewNumber();
        [OperationContract]
        bool CheckAnswer(string guess, string playerName);
        [OperationContract]
        bool Join(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        void updateClients();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Game: IGame
    {
        int currentAnswer = 0;

        bool gameStarted = false;

        [DataMember]
        public List<Player> players = new List<Player>();

        // When a player joins the game
        public bool Join(string name)
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            Player newPlayer = new Player(name, cb);
            players.Add(newPlayer);

            // Trigger callback that updates clients player names
            updateClients();
            return true;
        }

        // When a player closes their client
        public void Leave(string name)
        {
            foreach (Player player in players)
            {
                if (player.name == name)
                {
                    players.Remove(player);
                    updateClients();
                }
            }
        }

        // generate new 'correct answer'
        public int NewNumber()
        {
            Random rnd = new Random();
            int num = rnd.Next(1, 5);
            currentAnswer = num;
            return num;
        }

        // check if current users guess is correct
        public bool CheckAnswer(string guess, string playerName)
        {
            int guessInt = Convert.ToInt32(guess);

            if (guessInt == currentAnswer)
            {
                foreach (Player player in players)
                {
                    if (player.name == playerName)
                    {
                        player.score++;
                    }
                }
                updateClients();

                foreach (Player player in players)
                {
                    if (player.score == 3)
                    {
                        endGame(player.name);
                    }
                }


                return true;
            }
            else
            {
                return false;
            }
        }

        public void updateClients()
        {
            if (players.Count > 0)
            {
                foreach (Player player in players)
                {
                    player.callback.UpdatePlayers(players);
                }
            }
        }

        private void endGame(string name)
        {
            foreach (Player player in players)
            {
                player.callback.EndGame(name);
            }
        }
    }
}
