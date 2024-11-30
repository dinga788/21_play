using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace _21_играаа
{
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Card> playerCards;//карты игрока и дилера(Компьютера)
        private ObservableCollection<Card> dealerCards;

        private int playerScore;//переменные для хранения очков игрока и да
        private int dealerScore;

        private Deck deck = new Deck();//ну тут экземляр колоды карт

        private int playerChips = 100; // Фишки игрока
        private int dealerChips = 100; // Фишки дилера
        private int currentBet = 0; // Текущая ставка

        public Window1()
        {
            InitializeComponent();
            playerCards = new ObservableCollection<Card>();//те же карты игроков
            dealerCards = new ObservableCollection<Card>();

            DataContext = this;//привязка
            NewGame();
        }

        public int PlayerChips//фишки игрока
        {
            get { return playerChips; }
            set
            {
                playerChips = value;
                OnPropertyChanged(nameof(PlayerChips));
            }
        }

        public int DealerChips//фишки компухтера
        {
            get { return dealerChips; }
            set
            {
                dealerChips = value;
                OnPropertyChanged(nameof(DealerChips));
            }
        }

        public int CurrentBet//повышение ставки
        {
            get { return currentBet; }
            set
            {
                if (value < 10)
                    return; // Запретить ставку ниже 10
                currentBet = value;
                OnPropertyChanged(nameof(CurrentBet));
            }
        }

        private void IncreaseBet()//понижение ставки
        {
            if (PlayerChips >= CurrentBet + 10) // Проверяем, достаточно ли фишек
            {
                CurrentBet += 10;
            }
            else
            {
                MessageBox.Show("Недостаточно фишек для повышения ставки.");
            }
        }

        private void DecreaseBet()
        {
            if (CurrentBet > 10) // Проверяем, чтобы ставка не была ниже 10
            {
                CurrentBet -= 10;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Card> PlayerCards//доступ к колекции карт игрока с уведомлениями
        {
            get { return playerCards; }
            set
            {
                playerCards = value;
                OnPropertyChanged(nameof(PlayerCards));
            }
        }

        public ObservableCollection<Card> DealerCards//доступ к колекции карт додика с уведомлениями
        {
            get { return dealerCards; }
            set
            {
                dealerCards = value;
                OnPropertyChanged(nameof(DealerCards));
            }
        }

        public int PlayerScore//хранение очков игрока
        {
            get { return playerScore; }
            set
            {
                playerScore = value;
                OnPropertyChanged(nameof(PlayerScore));
            }
        }

        public int DealerScore//хранение очков компутахтера
        {
            get { return dealerScore; }
            set
            {
                dealerScore = value;
                OnPropertyChanged(nameof(DealerScore));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)//кнопка выхода
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//кнопка для перезапуска
        {
            NewGame();
        }

        internal void NewGame() // новая игра
        {
            if (deck.Count < 4) // 4 карты на старт (2 игроку и 2 дилеру)
            {
                MessageBox.Show("Карты закончились, игра завершена.");
                Close(); // Закрываем игру
                return;
            }

            PlayerCards.Clear();
            DealerCards.Clear();

            // Сброс начальной ставки
            CurrentBet = 10;

            // Проверка на наличие фишек у игрока
            if (PlayerChips <= 0)
            {
                MessageBox.Show("У вас закончились фишки! Игра окончена.");
                Close();
                return;
            }

            for (int i = 0; i < 2; i++) // раздаём всем по 2 карты, но карта у компа первая скрыта
            {
                PlayerCards.Add(deck.DrawCard());
                Card dealerCard = deck.DrawCard();

                if (i == 0)
                {
                    dealerCard.ImagePath = "/pp/back.png";
                }

                DealerCards.Add(dealerCard);
            }

            PlayerScore = 0;
            DealerScore = 0;

            UpdateScores();

            CheckForBust();
        }

        // Метод для завершения игры
        private void EndGame()
        {
            MessageBox.Show("Игра завершена. Спасибо за игру!");
            Close(); // Закрываем окно игры
        }

        private void CheckForBust()//проверка на перебор в начале игры(*исправить*)
        {
            if (PlayerScore > 21 || DealerScore > 21)
            {
                MessageBox.Show("Кто-то перебрал, раздача карт заново.");
                NewGame();
            }
        }

        private string displayDealerScore;//отображаем правильно счёт компутахтера
        public string DisplayDealerScore
        {
            get { return displayDealerScore; }
            set
            {
                displayDealerScore = value;
                OnPropertyChanged(nameof(DisplayDealerScore));
            }
        }

        public int MinPossibleDealerScore { get; set; }//переменные для мак. и мин. счёта дилера
        public int MaxPossibleDealerScore { get; set; }
        private void UpdateScores()//обновление счёта у игрока и компутахтера
        {
            PlayerScore = CalculateScore(PlayerCards);

            int knownDealerScore = CalculateScore(DealerCards.Skip(1).Where(card => card != null && !card.IsFaceDown).ToList());

            DisplayDealerScore = $"? + {knownDealerScore}";

            MinPossibleDealerScore = knownDealerScore + 2;
            MaxPossibleDealerScore = knownDealerScore + 14;
        }

        internal int CalculateScore(IEnumerable<Card> cards)//подсчёт очков на основе карт
        {
            int score = 0;
            int aceCount = 0;

            foreach (var card in cards)
            {
                if (card.Rank == 14)
                {
                    aceCount++;
                    score += 11;
                }
                else
                {
                    score += card.Rank;
                }
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        private void DealCardToDealer() // метод для раздачи карт дилеру
        {
            if (deck.Count == 0)
            {
                EndGame(); // Если карт нет, завершаем игру
                return;
            }

            if (MinPossibleDealerScore < 19)
            {
                var dealerCard = deck.DrawCard();
                if (dealerCard != null)
                {
                    DealerCards.Add(dealerCard);
                    UpdateScores();

                    if (MaxPossibleDealerScore > 21)
                    {
                        MessageBox.Show("Противник перебрал! Вы выиграли.");
                        NewGame();
                    }
                }
            }
        }

        private void CheckWinner() // проверка победителей
        {
            if (deck.Count == 0)
            {
                MessageBox.Show("Карты закончились, никто не выиграл.");
                NewGame();
                return;
            }

            OpenDealerHiddenCard();

            DealerScore = CalculateScore(DealerCards);
            DisplayDealerScore = DealerScore.ToString();

            if (PlayerScore > 21)
            {
                MessageBox.Show("Увы, перебор! Вы проиграли.");
                DealerChips += CurrentBet;
                PlayerChips -= CurrentBet;
                NewGame();
                return;
            }

            if (PlayerScore == 21 && DealerScore == 21)
            {
                MessageBox.Show("У всех игроков блэкджек! Ничья!");
                NewGame();
                return;
            }

            if (PlayerScore == DealerScore)
            {
                MessageBox.Show($"Счёт у всех: {PlayerScore}, ничья!");
                NewGame();
                return;
            }

            if (DealerScore > 21)
            {
                MessageBox.Show("Противник перебрал! Вы выиграли.");
                PlayerChips += CurrentBet;
                DealerChips -= CurrentBet;
                NewGame();
                return;
            }

            if (PlayerScore == 21 || DealerScore == 21)
            {
                if (PlayerScore == 21 && DealerScore != 21)
                {
                    MessageBox.Show("Блэкджек! Вы победили!");
                    PlayerChips += CurrentBet;
                    DealerChips -= CurrentBet;
                }
                else if (PlayerScore != 21 && DealerScore == 21)
                {
                    MessageBox.Show("У противника блэкджек. Вы проиграли!");
                    DealerChips += CurrentBet;
                    PlayerChips -= CurrentBet;
                }
            }
            else
            {
                if (PlayerScore > DealerScore)
                {
                    MessageBox.Show($"Ваш счёт: {PlayerScore}, счёт противника: {DealerScore}. Вы выиграли!");
                    PlayerChips += CurrentBet;
                    DealerChips -= CurrentBet;
                }
                else
                {
                    MessageBox.Show($"Ваш счёт: {PlayerScore}, счёт противника: {DealerScore}. К сожалению, вы проиграли...");
                    DealerChips += CurrentBet;
                    PlayerChips -= CurrentBet;
                }
            }

            if (PlayerChips <= 0)
            {
                MessageBox.Show("У вас закончились фишки! Игра окончена.");
                Close();
                return;
            }

            if (DealerChips <= 0)
            {
                MessageBox.Show("У дилера закончились фишки! Игра окончена.");
                Close();
                return;
            }

            NewGame();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)//раздача карт игроку и дилеру и проверка, не перебрал ли игрок
        {
            var playerCard = deck.DrawCard();
            PlayerCards.Add(playerCard);

            var dealerCard = deck.DrawCard();
            DealerCards.Add(dealerCard);

            UpdateScores();

            if (PlayerScore > 21)
            {
                MessageBox.Show("Увы, перебор! Вы проиграли.");
                NewGame();
            }
        }

        private void ButtonSkip_Click(object sender, RoutedEventArgs e)//типо закончить ход и подведение результатов
        {
            if (deck.Count == 0)
            {
                EndGame();
                return;
            }

            while (DealerScore < 19 && DealerCards.Count < 2)
            {
                DealCardToDealer();
            }

            CheckWinner();
        }

        private void OpenDealerHiddenCard()//открытие скрытной карты(*исправить*)
        {
            if (DealerCards[0].IsFaceDown)
            {
                DealerCards[0].IsFaceDown = false;
                UpdateScores();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            IncreaseBet();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            DecreaseBet();
        }
    }

    public class Card//ну тут сами карты, масти и т.д
    {
        public string Suit { get; set; }
        public int Rank { get; set; }
        public string ImagePath { get; set; }
        public bool IsFaceDown { get; set; }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }

    public class Deck//ну тут мы уже чекаем где карты и в конструкторе создаются карты
    {
        private List<Card> cards;
        public Deck()
        {
            cards = new List<Card>();

            foreach (string suit in new[] { "H", "B", "S", "C" })
            {
                for (int rank = 2; rank <= 14; rank++)
                {
                    string fileName = $"/pp/{rank}{suit}.png";
                    cards.Add(new Card { Suit = suit, Rank = rank, ImagePath = fileName });
                }
            }
        }

        public void Shuffle() //перемешиваем колоду
        {
            Random random = new Random();
            cards = cards.OrderBy(x => random.Next()).ToList();
        }

        public Card DrawCard()//раздача карт
        {
            if (cards.Count == 0)
            {
                MessageBox.Show("Карты закончились, никто не выиграл.");
                return null;
            }
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        public int Count => cards.Count;//получение количества карт
        public List<Card> Cards => cards;
    }
}