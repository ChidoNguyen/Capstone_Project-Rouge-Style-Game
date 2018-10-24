﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capstonia.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Capstonia.Systems
{
    // InventorySystem class
    // DESC:  Contains attributes and methods for the Inventory

    public class InventorySystem
    {
        private GameManager game;
        public readonly List<Item> Inventory;   //public because player needs to manipulate inventory
        private readonly int maxItems = 9;
        private int currentItems = 0;
        private Vector2[] coords =
        {
            new Vector2(670,50),
            new Vector2(779,50),
            new Vector2(888,50),
            new Vector2(670,100),
            new Vector2(779,100),
            new Vector2(888,100),
            new Vector2(670,150),
            new Vector2(779,150),
            new Vector2(888,150),
        };
        //coordinates for each slot on the inventory outline


        // InventorySystem()
        // DESC:    Constructor.
        // PARAMS:  GameManager object.
        // RETURNS: None.
        public InventorySystem(GameManager game)
        {
            this.game = game;
            Inventory = new List<Item>();

        }


        // AddItem()
        // DESC:    Adds item to the inventory.
        // PARAMS:  Item object.
        // RETURNS: None.
        public void AddItem(Item name)
        {
            Inventory.Add(name);
            currentItems++;
        }

        // RemoveItem()
        // DESC:    Removes item to the inventory.
        // PARAMS:  Item object.
        // RETURNS: None.
        public void RemoveItem(Item name)
        {
            int i;

            //Cycle through inventory and look for names that match the name parameter
            for (i = 0; i < maxItems; i++)
            {
                if (name.Name == Inventory[i].Name)
                {
                    Inventory.RemoveAt(i);
                    currentItems--;
                    break;
                }
            }
        }

        // useItem()
        // DESC:    attempts to use item in inventory slot
        // PARAMS:  slot number.
        // RETURNS: None.
        public void UseItem(int slot)
        {
            int index = slot - 1;
            Inventory[index].Broadcast();
        }

        // Draw()
        // DESC:    Draws the contents of the inventory to the screen
        // PARAMS:  None.
        // RETURNS: None.

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw our skeleton //
            spriteBatch.Draw(game.Outline, new Vector2(672, 1), Color.White);
            spriteBatch.DrawString(game.mainFont, "INVENTORY", new Vector2(795, 15), Color.White);
            int index = 0; // used for accessing coordinates
            foreach (Item things in Inventory)
            {
                switch (things.Name)
                {
                    case "Armor":
                        spriteBatch.Draw(game.armor, coords[index], Color.White);
                        index++;
                        break;
                    case "Food":
                        spriteBatch.Draw(game.food, coords[index], Color.White);
                        index++;
                        break;
                    case "Weapon":
                        spriteBatch.Draw(game.weapon, coords[index], Color.White);
                        index++;
                        break;
                    case "Book":
                        spriteBatch.Draw(game.book, coords[index], Color.White);
                        index++;
                        break;
                    case "Potion":
                        spriteBatch.Draw(game.potion, coords[index], Color.White);
                        index++;
                        break;

                }
            }
        }
    }
}
