using System;
using Core.Castle;
using Core.Server;
using UnityEngine;

namespace Core
{
    public class Fatigue
    {
        private static readonly int DeafultIncome = 1;
        private static readonly int DeafultDamage = 1;
        private static readonly int DeafultMaxDamage = 8;

        private readonly int division;

        private int multiplier;

        public int Damage { get; private set; }

        public int MaxDamage { get; private set; }

        public int Income { get; private set; }

        public Fatigue(int division)
        {
            this.division = division;
            Init();
        }

        public static Fatigue operator ++(Fatigue fatigue)
        {
            fatigue.Damage = Math.Min(fatigue.MaxDamage, fatigue.Damage + fatigue.Income);
            return fatigue;
        }

        private void Init()
        {
            Income = DeafultIncome;
            Damage = DeafultDamage;
            MaxDamage = DeafultMaxDamage;

            switch (division)
            {
                case 1:
                    multiplier = 1;
                    break;

                case 2:
                    multiplier = 1;
                    break;

                case 3:
                    multiplier = 2;
                    break;

                case 4:
                    multiplier = 2;
                    break;

                case 5:
                    multiplier = 3;
                    break;

                case 6:
                    multiplier = 3;
                    break;

                case 7:
                    multiplier = 4;
                    break;

                case 8:
                    multiplier = 4;
                    break;

                case 9:
                    multiplier = 5;
                    break;

                case 10:
                default:
                    multiplier = 6;
                    break;
            }

            Damage *= multiplier;
            MaxDamage *= multiplier;
            Income *= multiplier;
        }
    }
}