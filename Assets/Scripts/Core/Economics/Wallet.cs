using System;
using System.Collections.Generic;

namespace Core.Economics
{
    public class Wallet
    {
        private readonly Dictionary<string, Currency> balance = new ();

        public IReadOnlyDictionary<string, Currency> Balance => balance;

        public Wallet(IEnumerable<Currency> balance = null)
        {
            foreach (var currency in balance ?? Array.Empty<Currency>())
            {
                this.balance.Add(currency.Name, currency);
            }
        }

        public Currency this[string currencyName]
        {
            get
            {
                if (!balance.ContainsKey(currencyName))
                    balance.Add(currencyName, new Currency(currencyName, 0));
                
                return balance[currencyName];
            }
        }
    }
}

