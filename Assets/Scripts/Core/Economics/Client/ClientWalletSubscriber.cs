using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Economics.Client
{
    public class ClientWalletSubscriber : MonoBehaviour
    {
        public string currencyName;
        
        public TMP_Text textField;

        private void Start()
        {
            try
            {
                var c = ClientWalletController.Instance.Wallet[currencyName];
                OnCurrencyUpdated(c);
                c.OnCurrencyChanged += OnCurrencyUpdated;
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"ClientWalletController Instance not found!\n{e}");
                throw;
            }
        }

        private void OnCurrencyUpdated(Currency currency)
        {
            textField.text =
                currency.Amount.ToString();
        }
    }
}