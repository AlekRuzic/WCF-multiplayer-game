using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ServiceModel;
using System.Drawing;
using GameLibrary;

namespace GameClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {

        IGame game = new Game();
        List<Player> players = new List<Player>();
        List<TextBox> playerTextBoxes = new List<TextBox>();
        List<Label> playerLabels = new List<Label>();
        bool gameStarted = false;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Configure the ABCs of using the Game service
                DuplexChannelFactory<IGame> channel = new DuplexChannelFactory<IGame>(this, "GameService");

                // Activate a game object
                game = channel.CreateChannel();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Loop through this when updating GUI for active players
            playerTextBoxes.Add(txtPlayer1);
            playerTextBoxes.Add(txtPlayer2);
            playerTextBoxes.Add(txtPlayer3);
            playerTextBoxes.Add(txtPlayer4);

            playerLabels.Add(Player1Label);
            playerLabels.Add(Player2Label);
            playerLabels.Add(Player3Label);
            playerLabels.Add(Player4Label);

            txtPlayer1.Text = "0";
            txtPlayer2.Text = "0";
            txtPlayer3.Text = "0";
            txtPlayer4.Text = "0";

        }

        private void btnJoin_Click(object server, RoutedEventArgs e)
        {
            if(txtPlayerName.Text == "")
            {
                MessageBox.Show("Please enter a name");
            }
            else
            {
                // Prevent user from changing his name
                txtPlayerName.IsReadOnly = true;
                btnJoin.IsEnabled = false;

                game.Join(txtPlayerName.Text);
                
            }

        }


        private void btnGuess_Click(object sender, RoutedEventArgs e)
        {
            string guess = txtPlayerGuess.Text;
            string playerName = txtPlayerName.Text;

            if (guess == "")
            {
                MessageBox.Show("Please enter a number");
                return;
            }
            else
            {
                txtCorrectAnswer.Text = game.NewNumber().ToString();
                try
                {
                    if (game.CheckAnswer(guess, playerName))
                    {
                        MessageBox.Show("Correct!");
                    }
                    else
                    {
                        MessageBox.Show("Wrong!");
                    }
                }
                catch (Exception err)
                {
                    string error = err.ToString();
                    MessageBox.Show(error);
                }
            }

            
        }


        // Update scores
        private delegate void GuiUpdateDelegate(List<Player> players);

        public void UpdatePlayers(List<Player> players)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    this.players = players;

                    for (int i = 0; i<playerTextBoxes.Count(); i++) {
                        playerTextBoxes[i].Text = "0";
                        playerTextBoxes[i].IsEnabled = false;
                        playerLabels[i].Content = "Player " + (i + 1);
                        playerLabels[i].IsEnabled = false;
                    }

                    for (int i=0; i<players.Count(); i++)
                    {
                        playerTextBoxes[i].Text = players[i].score.ToString();
                        playerLabels[i].Content = players[i].name;
                        playerTextBoxes[i].IsEnabled = true;
                        playerLabels[i].IsEnabled = true;
                    }

                    // Check how many players are expected before unlocking the rest of the game
                    if (gameStarted == false)
                    {
                        if (this.players.Count() > 1)
                        {
                            MessageBoxResult messageBoxResult = MessageBox.Show("Are you waiting for a third player?", "Wait for more players?", MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {

                                // If 3 players are expected
                                if (this.players.Count() == 3)
                                {
                                    MessageBoxResult messageBoxResult2 = MessageBox.Show("Are you waiting for a fourth player?", "Wait for more players?", MessageBoxButton.YesNo);
                                    if (messageBoxResult2 == MessageBoxResult.Yes)
                                    {
                                        return;
                                    }
                                    else if (messageBoxResult2 == MessageBoxResult.No)
                                    {
                                        gameStarted = true;
                                        StartGame();
                                    }
                                }

                                // If 4 players are expected
                                if (this.players.Count() == 4)
                                {
                                    gameStarted = true;
                                    StartGame();
                                }


                            }

                            // If only 2 players are expected
                            else if (messageBoxResult == MessageBoxResult.No)
                            {
                                gameStarted = true;
                                StartGame();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(UpdatePlayers), new object[] { players });
            }

        }

        private void StartGame()
        {
            txtPlayerGuess.IsEnabled = true;
            yourGuessLabel.IsEnabled = true;
            correctAnswerLabel.IsEnabled = true;
            txtCorrectAnswer.IsEnabled = true;
            btnGuess.IsEnabled = true;
        }

        private delegate void EndGameDelegate(string player);

        public void EndGame(string player)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    txtPlayerGuess.IsEnabled = false;
                    yourGuessLabel.IsEnabled = false;
                    correctAnswerLabel.IsEnabled = false;
                    txtCorrectAnswer.IsEnabled = false;
                    btnGuess.IsEnabled = false;

                    MessageBox.Show(player + " has won!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new EndGameDelegate(EndGame), new object[] { player });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (players.Count() > 0)
                game.Leave(txtPlayerName.Text);
        }

        private void guessInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void TxtPlayerGuess_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
