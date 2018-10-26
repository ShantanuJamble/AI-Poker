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

            /*
            If RR < 0.8 then 95% fold, 0 % call, 5% raise (bluff)
            If RR < 1.0 then 80%, fold 5% call, 15% raise (bluff)
            If RR <1.3 the 0% fold, 60% call, 40% raise
            Else (RR >= 1.3) 0% fold, 30% call, 70% raise
            If fold and amount to call is zero, then call.
            */   

            
            if(rateOfReturn < 0.8)
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
            else if(rateOfReturn<1.3){

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
            throw new NotImplementedException();
        }

        public override PlayerAction Draw(Card[] hand)
        {
            throw new NotImplementedException();
        }
    }
}
