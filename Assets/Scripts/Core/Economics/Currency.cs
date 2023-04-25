using System;

namespace Core.Economics
{
    public class Currency
    {
        private double amount = 0;
        
        public string Name { get; set; }

        public double Amount
        {
            get => amount;
            set
            {
                amount = value;
                OnCurrencyChanged?.Invoke(this);
            }
        }

        public delegate void CurrencyHandler(Currency currency);

        public event CurrencyHandler OnCurrencyChanged;

        public Currency(string name, double amount)
        {
            Name = name;
            Amount = amount;
        }
    }
}