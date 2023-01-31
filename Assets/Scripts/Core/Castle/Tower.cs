namespace Core.Castle
{
    public class Tower
    {
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        
        public Tower(int maxHealth, int health)
        {
            MaxHealth = maxHealth;
            Health = health;
        }

        public void Damage(int value)
        {
            Health -= value;

            if (Health < 0)
                Health = 0;
        }

        public void Heal(int value)
        {
            Health += value;

            if (Health > MaxHealth)
                Health = MaxHealth;
        }
    }
}