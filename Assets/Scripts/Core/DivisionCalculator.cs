using Core.Cards;

namespace Core
{
    public class DivisionCalculator
    {
        public static int CalculateDivision(CardData[] cards) 
        {
            return SpotDivision(CalculateMass(cards));
        }

        public static float AddMass(CardData card, float mass) 
        {
            CardData[] cards = { card };
            return mass + CalculateMass(cards);
        }

        public static float RemoveMass(CardData card, float mass)
        {
            CardData[] cards = { card };
            return mass - CalculateMass(cards);
        }

        public static int SpotDivision(float mass) 
        {
            int div = 1;

            if (mass > 1250)
            {
                div = 2;
            }
            if (mass > 1550)
            {
                div = 3;
            }
            if (mass > 1650)
            {
                div = 4;
            }
            if (mass > 1850)
            {
                div = 5;
            }
            if (mass > 2050)
            {
                div = 6;
            }
            if (mass > 2250)
            {
                div = 7;
            }
            if (mass > 2450)
            {
                div = 8;
            }
            if (mass > 2650)
            {
                div = 9;
            }
            if (mass > 2850)
            {
                div = 10;
            }

            return div;
        }

        public static float CalculateMass(CardData[] cards) 
        {
            float mass = 0;

            foreach (var card in cards)
            {
                if (card.InDeck)
                {
                    switch (card.Rang)
                    {
                        case 0:
                            mass += 10; break;
                        case 1:
                            mass += 16; break;
                        case 2:
                            mass += 25.6f; break;
                        case 3:
                            mass += 40.96f; break;
                        case 4:
                            mass += 65.536f; break;
                    }
                }
            }

            return mass;
        }
    }
}
