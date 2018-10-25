﻿using Capstonia.Core;
using Microsoft.Xna.Framework.Graphics;

namespace Capstonia.Monsters
{
    public class Beholder : Monster
    {
        // constructor
        public Beholder(GameManager game) : base(game)
        {

        }

        // create monster
        public static Beholder Create(GameManager game, int level)
        {
            // TODO - Update how level impacts stats
            Beholder beholder = new Beholder(game)
            {
                // every point above 10 gives a health bonus
                Constitution = 10,
                // every point above 10 gives a dodge bonus
                Dexterity = 10,
                // health total for Capstonian; if the values reaches 0, the Capstonain is killed
                Health = 50,
                // max dmg Capstonian can cause
                MaxDamage = 3,
                // min dmg Capstonain can cause
                MinDamage = 1,
                // name of monster
                Name = "Beholder",
                // every point above 10 gives a dmg bonus
                Strength = 10
            };

            return beholder;
        }
    }
}