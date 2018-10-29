using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerTournament
{
    class Player8 : Player
    {
        int pot_value;
        bool bet = false; 
        public Player8(int idnum,string nm,int mny):base(idnum,nm,mny)
        {
            pot_value = 0;
        }


        //Creating a dummy deck to calculate hand probability
        private List<Card> CreateDummyDeck(Card[] hand)
        {
            string[] suits = { "Hearts", "Clubs", "Diamonds", "Spades" };
            List<Card> demodeck = new List<Card>();

            foreach (string s in suits)
            {
                for(int i = 2; i <= 14; i++)
                {
                    demodeck.Add(new Card(s, i));
                }
            }
            foreach(Card c in hand)
            {
                demodeck.Remove(c);
            }

            return demodeck;
        }

        /// <summary>
        /// This method simulates 1000 scenarios with the current hand AI has to calculate winning percentage.
        /// </summary>
        /// <param name="hand">Current hand</param>
        /// <returns>
        /// Strength of cards player has.
        /// </returns>
        private double CalculateHandStrength(Card[] hand)
        {

            
            Card [] sample_hand = new Card[5];
            float win_count = 0f;
            Random r = new Random();
            int pos;
            Card high,new_high;
            int rating = Evaluate.RateAHand(hand, out high);
            int new_rating;
            for(int run = 0; run < 1000; run++)
            {
                List<Card> demodeck = CreateDummyDeck(hand);
                for (int i = 0; i < 5; i++)
                {
                    pos = r.Next(demodeck.Count);
                    sample_hand[i] = demodeck[pos];
                    demodeck.RemoveAt(pos);

                }
                new_rating = Evaluate.RateAHand(sample_hand, out new_high);
                if (rating > new_rating)
                {
                    win_count++;
                }
                else if (rating==new_rating && high.Value >= new_high.Value)
                {
                    win_count++;
                }
            }
            double hand_strength = win_count / 1000f;
            hand_strength = Math.Round(hand_strength, 2);
            return hand_strength;
        }

        /// <summary>
        /// This method calculates the possibility of return from the action we choose to play.
        /// </summary>
        
        private double CalculateRateOfReturn(PlayerAction action,Card[] hand,out String action_String,out int amount)
        {
            double hand_strength = CalculateHandStrength(hand);
            int last_play_amount = (action != null) ? action.Amount : 0;
            pot_value += last_play_amount;
            int call_value = last_play_amount;
            double pot_odds;
            int  bet_value = last_play_amount + ((call_value!=0)?(int)(hand_strength * call_value*1.5):20);
            double call_odds = (double)call_value / (double)(pot_value + call_value);
            double bet_odds = (double)bet_value / (double)(pot_value + bet_value);

            Console.WriteLine(call_odds + " " + bet_odds);
            if (bet_odds > call_odds || last_play_amount==0)
            {
                action_String = "bet";
                pot_odds = bet_odds;
                amount = bet_value;
                Console.WriteLine("In bet");
            }
            else
            {
                action_String = "call";
                pot_odds = call_odds;
                amount = call_value;
                Console.WriteLine("In call");
            }

            double rateOfReturn = hand_strength / pot_odds;

            return Math.Round(rateOfReturn,2);
        }



        public override PlayerAction BettingRound1(List<PlayerAction> actions, Card[] hand)
        {
            PlayerAction decision = null;
            int amount = 0;
            string action_suggestion = "";
            PlayerAction last_action = (actions.Count > 0) ? actions[actions.Count - 1] : null;

            //bet needs to be played to raise or call
            if (bet==false && last_action != null)
            {
                if (last_action.ActionName == "bet")
                {
                    bet = true;
                }
            }
            double rateOfReturn = CalculateRateOfReturn(last_action, hand, out action_suggestion, out amount);
            Console.Write(rateOfReturn);
            Console.Write(action_suggestion);
            Console.Write(amount);
            Random rnd = new Random();

            //These values are the probabilities that the AI will choose a given action based on the rate of return
            //There is some randomness to ensure that you can never tell how strong the AI's hand is based on its actions
            //This is so the AI is able to bluff
            /*
            If RR < 0.8 then 95% fold, 0 % call, 5% raise (bluff)
            If RR < 1.0 then 80%, fold 5% call, 15% raise (bluff)
            If RR <1.3 the 0% fold, 60% call, 40% raise
            Else (RR >= 1.3) 0% fold, 30% call, 70% raise
            If fold and amount to call is zero, then call.
            */

            //Low rate of return, AI is very likely to fold but may occasionally raise
            if (rateOfReturn < 0.8)
            {
                int tmp = rnd.Next(100);
                int tmp_momey = (Money > 10) ? 10 : Money;
                if (last_action == null)
                {
                    decision = FoldAction(last_action);
                }
                else
                {
                    if (tmp > 95)//Bluff
                    {
                        if (bet)
                        {
                            decision = new PlayerAction(Name, "Bet1", "raise", tmp_momey);
                        }
                        else
                        {
                            decision = new PlayerAction(Name, "Bet1", "bet", tmp_momey);
                            bet = true;
                        }
                    }
                    else
                    {
                        decision = FoldAction(last_action);

                    }
                }
            }
            //somewhat low rate of return, likely to fold, but also may call and somewhat more likely to raise than call
            else if (rateOfReturn < 1) 
            {
                int tmp = rnd.Next(100);
                if (last_action == null)
                {
                    decision = FoldAction(last_action);
                }
                else
                {
                    if (tmp < 80)
                    {
                        decision = FoldAction(last_action);
                    }
                    else if (tmp > 80 && tmp <= 85)
                    {
                        if (bet)
                        {
                            decision = new PlayerAction(Name, "Bet1", "call", 0);
                        }
                        else
                        {
                            decision = new PlayerAction(Name, "Bet1", "fold", 0);
                        }
                    }
                    else//bluff
                    {
                        int tmp_money = (Money > 20) ? 20 : Money;
                        decision = new PlayerAction(Name, "Bet1", "raise", tmp_money);
                    }
                }
            }
            //high rate of return, AI will not fold. It may raise, but is more likely to call
            else if (rateOfReturn<1.3){

                int tmp = rnd.Next(100);
                int tmp_money = (Money>amount)?amount:Money;
                if (last_action == null)
                {
                    decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                }
                else
                {
                    switch (action_suggestion)
                    {
                        case "call":
                            if (bet)
                            {
                                decision = new PlayerAction(Name, "Bet1", "call", tmp_money);
                            }
                            else
                            {
                                decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                            }
                            break;
                        case "bet":
                            tmp_money = (Money > amount * 2) ? tmp_money : 0;
                            if (bet && tmp_money != 0)
                            {
                                decision = new PlayerAction(Name, "Bet1", "raise", amount);
                            }

                            break;
                    }
                }
            }
            //very high rate of return. AI is very likely to raise but may also call.
            else if (rateOfReturn >= 1.3)
            {
                int tmp = rnd.Next(100);
                int tmp_money = (Money > amount) ? amount : Money;
                if (last_action == null)
                {
                    decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                }
                else
                {
                    switch (action_suggestion)
                    {
                        case "call":
                            if (bet)
                            {
                                decision = new PlayerAction(Name, "Bet1", "call", tmp_money);
                            }
                            else
                            {
                                decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                            }
                            break;
                        case "bet":
                            tmp_money = (Money > amount * 2) ? tmp_money : 0;
                            if (bet && tmp_money != 0)
                            {
                                decision = new PlayerAction(Name, "Bet1", "raise", amount);
                            }

                            break;
                    }
                }
            }
            Console.Write("Decision" + decision.ActionName);
            return decision;

        }

        private PlayerAction FoldAction(PlayerAction last_action)
        {
            PlayerAction decision;
            if ((last_action!= null && last_action.Name == "check") || Dealer == false)
            {
                decision = new PlayerAction(Name, "Bet1", "check", 0);
            }
            else
            {
                decision = new PlayerAction(Name, "Bet1", "fold", 0);
            }

            return decision;
        }
        
        public override PlayerAction BettingRound2(List<PlayerAction> actions, Card[] hand)
        {
            //reset values
            PlayerAction decision = null;
            int amount = 0;
            string action_suggestion = "";
            PlayerAction last_action = (actions.Count > 0) ? actions[actions.Count - 1] : null;

            //bet needs to be played to raise or call
            if (bet == false && last_action != null)
            {
                if (last_action.ActionName == "bet")
                {
                    bet = true;
                }
            }
            double rateOfReturn = CalculateRateOfReturn(last_action, hand, out action_suggestion, out amount);
            Console.Write(rateOfReturn);
            Console.Write(action_suggestion);
            Console.Write(amount);
            Random rnd = new Random();

            //These values are the probabilities that the AI will choose a given action based on the rate of return
            //There is some randomness to ensure that you can never tell how strong the AI's hand is based on its actions
            //This is so the AI is able to bluff
            /*
            If RR < 0.8 then 95% fold, 0 % call, 5% raise (bluff)
            If RR < 1.0 then 80% fold, 5% call, 15% raise (bluff)
            If RR <1.3 the 0% fold, 60% call, 40% raise
            Else (RR >= 1.3) 0% fold, 30% call, 70% raise
            If fold and amount to call is zero, then call.
            */

            //Low rate of return, AI is very likely to fold but may occasionally raise
            if (rateOfReturn < 0.8)
            {
                int tmp = rnd.Next(100);
                int tmp_momey = (Money > 10) ? 10 : Money;
                if (last_action == null)
                {
                    decision = FoldAction(last_action);
                }
                else
                {
                    if (tmp > 95)//Bluff
                    {
                        if (bet)
                        {
                            decision = new PlayerAction(Name, "Bet1", "raise", tmp_momey);
                        }
                        else
                        {
                            decision = new PlayerAction(Name, "Bet1", "bet", tmp_momey);
                            bet = true;
                        }
                    }
                    else
                    {
                        decision = FoldAction(last_action);

                    }
                }
            }
            //somewhat low rate of return, likely to fold, but also may call and somewhat more likely to raise than call
            else if (rateOfReturn < 1)
            {
                int tmp = rnd.Next(100);
                if (last_action == null)
                {
                    decision = FoldAction(last_action);
                }
                else
                {
                    if (tmp < 80)
                    {
                        decision = FoldAction(last_action);
                    }
                    else if (tmp > 80 && tmp <= 85)
                    {
                        if (bet)
                        {
                            decision = new PlayerAction(Name, "Bet1", "call", 0);
                        }
                        else
                        {
                            decision = new PlayerAction(Name, "Bet1", "fold", 0);
                        }
                    }
                    else//bluff
                    {
                        int tmp_money = (Money > 20) ? 20 : Money;
                        decision = new PlayerAction(Name, "Bet1", "raise", tmp_money);
                    }
                }
            }
            //high rate of return, AI will not fold. It may raise, but is more likely to call
            else if (rateOfReturn < 1.3)
            {

                int tmp = rnd.Next(100);
                int tmp_money = (Money > amount) ? amount : Money;
                if (last_action == null)
                {
                    decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                }
                else
                {
                    switch (action_suggestion)
                    {
                        case "call":
                            if (bet)
                            {
                                decision = new PlayerAction(Name, "Bet1", "call", tmp_money);
                            }
                            else
                            {
                                decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                            }
                            break;
                        case "bet":
                            tmp_money = (Money > amount * 2) ? tmp_money : 0;
                            if (bet && tmp_money != 0)
                            {
                                decision = new PlayerAction(Name, "Bet1", "raise", amount);
                            }

                            break;
                    }
                }
            }
            //very high rate of return. AI is very likely to raise but may also call.
            else if (rateOfReturn >= 1.3)
            {
                int tmp = rnd.Next(100);
                int tmp_money = (Money > amount) ? amount : Money;
                if (last_action == null)
                {
                    decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                }
                else
                {
                    switch (action_suggestion)
                    {
                        case "call":
                            if (bet)
                            {
                                decision = new PlayerAction(Name, "Bet1", "call", tmp_money);
                            }
                            else
                            {
                                decision = new PlayerAction(Name, "Bet1", "bet", tmp_money);
                            }
                            break;
                        case "bet":
                            tmp_money = (Money > amount * 2) ? tmp_money : 0;
                            if (bet && tmp_money != 0)
                            {
                                decision = new PlayerAction(Name, "Bet1", "raise", amount);
                            }

                            break;
                    }
                }
            }
            Console.Write("Decision" + decision.ActionName);
            return decision;

        }

        public override PlayerAction Draw(Card[] hand)
        {
            // Sort the hand
            Evaluate.SortHand(hand);

            // Figure out what cards to replace
            bool[] cardsToDelete = GetCardsToDelete(hand);

            // How many cards to replace
            int numOfCardsToDelete = 0;
            for(int i = 0; i < cardsToDelete.Length; i++)
            {
                if(cardsToDelete[i])
                {
                    numOfCardsToDelete++;
                }
            }

            PlayerAction pa = null;
            if(numOfCardsToDelete > 0 && numOfCardsToDelete < 5)
            {
                for(int i = 0; i < cardsToDelete.Length; i++)
                {
                    if(cardsToDelete[i])
                    {
                        hand[i] = null;
                    }
                }

                pa = new PlayerAction(Name, "Draw", "draw", numOfCardsToDelete);
            }
            else if(numOfCardsToDelete == 5)
            {
                for(int i = 0; i < hand.Length; i++)
                {
                    hand[i] = null;
                }

                pa = new PlayerAction(Name, "Draw", "draw", 5);
            }
            else
            {
                pa = new PlayerAction(Name, "Draw", "stand pat", 0);
            }

            return pa;
        }

        public bool[] GetCardsToDelete(Card[] hand)
        {
            bool[] cardPositionToDelete = { false, false, false, false, false }; 
            Card highCard = null;
            int rank = Evaluate.RateAHand(hand, out highCard);

            // Do not change any cards for Royal Flush, Straight Flush, Full House, Flush or Straight

            // In case of four of a kind try and get a better value card for the kicker
            if(rank == 8 && highCard.Value < 7)
            {
                if(hand[0].Value == hand[2].Value)
                {
                    cardPositionToDelete[4] = true;
                }
                else
                {
                    cardPositionToDelete[0] = true;
                }
            }

            // In case of 3 of a kind, change one card if the kicker is high, else discard 2 cards
            if (rank == 4 && highCard.Value > 7)
            {
                if((hand[0].Value == hand[1].Value) && (hand[0].Value == hand[2].Value))
                {
                    cardPositionToDelete[3] = true;
                }
                else
                {
                    cardPositionToDelete[0] = true;
                }
            }
            else if(rank == 4 && highCard.Value < 7)
            {
                if ((hand[0].Value == hand[1].Value) && (hand[0].Value == hand[2].Value))
                {
                    cardPositionToDelete[3] = true;
                    cardPositionToDelete[4] = true;
                }
                else if ((hand[1].Value == hand[2].Value) && (hand[1].Value == hand[3].Value))
                {
                    cardPositionToDelete[0] = true;
                    cardPositionToDelete[4] = true;
                }
                else
                {
                    cardPositionToDelete[0] = true;
                    cardPositionToDelete[1] = true;
                }
            }

            if(rank == 3 && highCard.Value < 7)
            {
                if((hand[4].Value == hand[3].Value) && (hand[2].Value == hand[1].Value))
                {
                    cardPositionToDelete[0] = true;
                }
                else if ((hand[4].Value == hand[3].Value) && (hand[1].Value == hand[0].Value))
                {
                    cardPositionToDelete[2] = true;
                }
                else
                {
                    cardPositionToDelete[4] = true;
                }
            }

            // If it is one pair or high card, check if there is a possibility of flush or straight and discard cards accordingly
            if(rank == 2 || rank == 1)
            {
                if(IsFlushPossible(hand, out cardPositionToDelete))
                {
                    return cardPositionToDelete;
                }
                else if (IsStraightPossible(hand, out cardPositionToDelete))
                {
                    return cardPositionToDelete;
                }
                else
                {
                    // Discard cards with value lower than 10
                    for(int i = 0; i < hand.Length; i++)
                    {
                        if(hand[i].Value < 10)
                        {
                            cardPositionToDelete[i] = true;
                        }
                    }
                }
            }

            return cardPositionToDelete;
        }

        public bool IsFlushPossible(Card[] hand, out bool[] cardsPosition)
        {
            int spades = 0;
            int hearts = 0;
            int clubs = 0;
            int diamonds = 0;

            cardsPosition = new bool[5] { true, true, true, true, true };

            for(int i = 0; i < hand.Length; i++)
            {
                if (hand[i].Suit.Equals("Spades"))
                    spades++;
                else if (hand[i].Suit.Equals("Hearts"))
                    hearts++;
                else if (hand[i].Suit.Equals("Clubs"))
                    clubs++;
                else
                    diamonds++;
            }

            if(spades >= 3)
            {
                for(int i = 0; i < hand.Length; i++)
                {
                    if(hand[i].Suit.Equals("Spades"))
                    {
                        cardsPosition[i] = false;
                    }
                }
                return true;
            }
            else if(hearts >= 3)
            {
                for (int i = 0; i < hand.Length; i++)
                {
                    if (hand[i].Suit.Equals("Hearts"))
                    {
                        cardsPosition[i] = false;
                    }
                }
                return true;
            }
            else if (clubs >= 3)
            {
                for (int i = 0; i < hand.Length; i++)
                {
                    if (hand[i].Suit.Equals("Clubs"))
                    {
                        cardsPosition[i] = false;
                    }
                }
                return true;
            }
            else if (diamonds >= 3)
            {
                for (int i = 0; i < hand.Length; i++)
                {
                    if (hand[i].Suit.Equals("Diamonds"))
                    {
                        cardsPosition[i] = false;
                    }
                }
                return true;
            }
            
            for(int i = 0; i < hand.Length; i++)
            {
                cardsPosition[i] = false;
            }

            return false;
        }

        public bool IsStraightPossible(Card[] hand, out bool[] cardsPosition)
        {
            int numCards = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < hand.Length; j++)
                {
                    if (hand[j].Value - hand[i].Value > 4)
                    {
                        if (j - i + 1 > numCards)
                        {
                            numCards = j - i + 1;
                        }
                        break;
                    }
                }
            }

            if (numCards == 3)
            {
                if (hand[2].Value - hand[0].Value < 5)
                {
                    cardsPosition = new bool[5] { false, false, false, true, true };
                }
                else if (hand[3].Value - hand[1].Value < 5)
                {
                    cardsPosition = new bool[5] { true, false, false, false, true };
                }
                else
                {
                    cardsPosition = new bool[5] { true, true, false, false, false };
                }
                return true;
            }
            else if (numCards == 4)
            {
                if (hand[3].Value - hand[0].Value < 5)
                {
                    cardsPosition = new bool[5] { false, false, false, false, true };
                }
                else
                {
                    cardsPosition = new bool[5] { true, false, false, false, false };
                }
                return true;
            }

            cardsPosition = new bool[5] { false, false, false, false, false };
            return false;
        }
    }
}
