using System;
using Newtonsoft.Json;

namespace Core.Castle
{
    [Serializable]
    public class BattleResource
    {
        public string Name;
        public int Value;
        public int Income;

        [JsonConstructor]
        public BattleResource(string name, int value = 0, int income = 0)
        {
            Name = name;
            Value = value;
            Income = income;
        }

        public void AddResource(int value)
        {
            Value += value;
        }

        public void RemoveResource(int value)
        {
            Value -= value;

            if (Value < 0)
                Value = 0;
        }

        public void AddIncome(int income)
        {
            Income += income;
        }

        public void RemoveIncome(int income)
        {
            Income -= income;

            if (Income < 1)
                Income = 1;
        }
    }
}