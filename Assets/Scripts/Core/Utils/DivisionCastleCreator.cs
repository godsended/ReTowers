using System.Collections.Generic;
using Core.Castle;

namespace Core.Utils
{
    public class DivisionCastleCreator : ICastleCreator
    {
        private readonly int division;
        
        public DivisionCastleCreator(int division)
        {
            this.division = division;
        }
        
        public CastleEntity CreateCastle()
        {
            Tower tower;
            Wall wall;
            switch (division)
            {
                case 1:
                    tower = new Tower(50, 20);
                    wall = new Wall(50, 10);
                    break;
                
                case 2:
                    tower = new Tower(60, 24);
                    wall = new Wall(60, 12);
                    break;
                
                case 3:
                    tower = new Tower(70, 28);
                    wall = new Wall(70, 14);
                    break;
                
                case 4:
                    tower = new Tower(85, 34);
                    wall = new Wall(85, 16);
                    break;

                case 5:
                    tower = new Tower(100, 40);
                    wall = new Wall(100, 20);
                    break;
                
                case 6:
                    tower = new Tower(120, 48);
                    wall = new Wall(120, 24);
                    break;
                
                case 7:
                    tower = new Tower(145, 58);
                    wall = new Wall(145, 28);
                    break;
                
                case 8:
                    tower = new Tower(175, 70);
                    wall = new Wall(175, 34);
                    break;
                
                case 9:
                    tower = new Tower(210, 84);
                    wall = new Wall(210, 40);
                    break;
                
                case 10:
                default:
                    tower = new Tower(250, 100);
                    wall = new Wall(250, 48);
                    break;
            }

            CastleEntity castleEntity = new CastleEntity(tower, wall, CreateResourcesList());
            return castleEntity;
        }

        private List<BattleResource> CreateResourcesList()
        {
            int income, value;
            switch (division)
            {
                case 1:
                    income = 2;
                    value = 7;
                    break;
                
                case 2:
                    income = 3;
                    value = 9;
                    break;
                
                case 3:
                    income = 3;
                    value = 10;
                    break;
                
                case 4:
                    income = 4;
                    value = 12;
                    break;

                case 5:
                    income = 4;
                    value = 14;
                    break;
                
                case 6:
                    income = 5;
                    value = 16;
                    break;
                
                case 7:
                    income = 5;
                    value = 19;
                    break;
                
                case 8:
                    income = 6;
                    value = 22;
                    break;
                
                case 9:
                    income = 7;
                    value = 26;
                    break;
                
                case 10:
                default:
                    income = 8;
                    value = 30;
                    break;
            }

            value -= income;
            
            return new List<BattleResource>()
            {
                new BattleResource("Resource_1", value, income),
                new BattleResource("Resource_2", value, income),
                new BattleResource("Resource_3", value, income)
            };
        }
    }
}