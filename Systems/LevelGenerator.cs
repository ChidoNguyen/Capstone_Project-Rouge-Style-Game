using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RogueSharp;
using Rectangle = RogueSharp.Rectangle;
using Point = RogueSharp.Point;
using Capstonia;
using Capstonia.Core;

namespace Capstonia.Systems
{
    public class LevelGenerator
    {
        // columns and rows should remain equal
        private readonly int columns;
        private readonly int rows;
        private readonly int levelWidth;
        private readonly int levelHeight;
        private readonly int roomWidth;
        private readonly int roomHeight;

        private readonly LevelGrid level;
        private readonly GameManager game;

        private List<Rectangle> ExitPath;
        private Rectangle startRoom;
        private Rectangle exitRoom;

        // constructor
        public LevelGenerator(GameManager game, int width, int height, int gameRows, int gameCols, int mapLevel)
        {
            levelWidth = width;
            levelHeight = height;
            columns = gameCols;
            rows = gameRows;
            roomWidth = width / columns;
            roomHeight = height / rows;

            this.game = game;
            level = new LevelGrid(game);
            ExitPath = new List<Rectangle>();
        }

        // CreateLevel()
        // DESC:    Handler for entire process of Level Generation.     
        // PARAMS:  None.
        // RETURNS: level(LevelGrid) - Fully generated level.
        public LevelGrid CreateLevel()
        {
            // Initialize Grid
            // Creates grid that is solid/unwalkable with the given dimensions
            level.Initialize(levelWidth, levelHeight);

            int x, y;
            int roomCounter = 1;
            // assign area for rooms
            for(int col = 0; col < columns; col++)
            {
                for(int row = 0; row < rows; row++)
                {
                    x = col * roomWidth;
                    y = row * roomHeight;

                    level.Rooms.Add(AssignRoom(x, y));

                    roomCounter++;
                }
            }

            // create the rooms previously assigned
            foreach(Rectangle room in level.Rooms)
            {
                CreateRoom(room);
               
            }

            // place player start
            PlacePlayerInStartingRoom();

            // place exit
            SelectExitRoom();
            PlaceExit();

            // place doors between player start and exit
            FindExitPath();
            PlaceDoorsOnPath();

            // randomly place doors - TODO?
            
            return level;
        }

        // AssignRoom()
        // DESC:    Creates a Rectangle object based on location and size parameters.             
        // PARAMS:  x(int), y(int) - Represents grid location of top left corner of room
        // RETURNS: room(Rectangle) - Represents area assigned for this room
        public Rectangle AssignRoom(int x, int y)
        {
            var room = new Rectangle(x, y, roomWidth - 1, roomHeight - 1);

            return room;
        }

        // CreateRoom()
        // DESC:    Takes a solid/unwalkable room (Rectangle object) and chisels out the inside.
        //          It leaves a ring around the outside of the room as unwalkable to represent the walls.
        // PARAMS:  room(Rectangle)
        // RETURNS: Nothing.  Modifies the level(LevelGrid) object.
        public void CreateRoom(Rectangle room)
        {
            // loop through each space in room and make it walkable
            for(int x = room.Left + 1; x < room.Right; x++)
            {
                for(int y = room.Top + 1; y < room.Bottom; y++)
                {
                    level.SetCellProperties(x, y, true, true, false);
                }
            }
        }

        // CreateDoor()
        // DESC:    Creates a door at location in room defined by x and y
        // PARAMS:  x(int), y(int)
        // RETURNS: None.
        public void CreateDoor(int x, int y)
        {
            // TODO
        }

        // SelectRandomRoom()
        // DESC:    Chooses a random room from level.Rooms
        // PARAMS:  None
        // RETURNS: random room(rectangle)
        public Rectangle SelectRandomRoom()
        {
            return level.Rooms[GameManager.Random.Next(level.Rooms.Count - 1)];
        }

        // PlacePlayerInStartingRoom()
        // DESC:    Places the Player in a random starting room for the Level.
        //          Instantiates Player object if this is the first Level.
        // PARAMS:  None
        // RETURNS: None.  Modifies level(LevelGrid) object.
        public void PlacePlayerInStartingRoom()
        {
            // if this is the first Level, Player is not yet instantiated
            // so here we check to see if it exists, if not, we create the object
            if(game.Player == null)
            {
                Player player = new Player(game);
                player.Sprite = game.Content.Load<Texture2D>("dknight_1");
                game.Player = player;
            }

            // get random room as starting room
            startRoom = SelectRandomRoom();


            // give player position in center of room
            game.Player.X = startRoom.Center.X;
            game.Player.Y = startRoom.Center.Y;

            // add player to that room
            level.AddPlayer(game.Player);
        
        }


        // SelectExitRoom()
        // DESC:    Selects a random room that is not the starting room as the exit room.
        // PARAMS:  None
        // RETURNS: None. Creates an exit room.
        public void SelectExitRoom()
        {
            //Select random room for exit room
            exitRoom = SelectRandomRoom();

            //Ensures the exit room does not choose the same room that the player starts in
            while(exitRoom.Contains(game.Player.X, game.Player.Y))
            {
                exitRoom = SelectRandomRoom();
            }            
        }


        // PlaceExit()
        // DESC:    Creates an exit at a random point within the exit room.
        // PARAMS:  None.
        // RETURNS: None.  Modifies exit room.
        public void PlaceExit()
        {
            //Select random point within exit room
            Point randomPoint = GetRandomPointInRoom(exitRoom);

            //Ensures that the selected tile is walkable (i.e. not a wall or door)
            while(!level.IsWalkable(randomPoint.X, randomPoint.Y))
            {
                randomPoint = GetRandomPointInRoom(exitRoom);
            }

            //Creates LevelExit object and places the exit at the selected location
            level.LevelExit = new Exit(game);
            level.LevelExit.X = randomPoint.X;
            level.LevelExit.Y = randomPoint.Y;
        }

        // GetRandomPointInRoom()
        // DESC:    Gets and returns a random coordinate within a given room.
        // PARAMS:  room(Rectangle)
        // RETURNS: Point with the x and y coordinates of random point within the room.
        public Point GetRandomPointInRoom(Rectangle room)
        {
            return new Point(GameManager.Random.Next(room.Left, room.Right), 
                             GameManager.Random.Next(room.Top, room.Bottom));
        }

        // FindExitPath()
        // DESC:    Finds a path from start room to exit room and stores the path in a exit path object.
        // PARAMS:  None.
        // RETURNS: None.  Modifies ExitPath by storing room objects inside the exitPath List.
        public void FindExitPath()
        {
            //Create variables 
            Rectangle currentRoom = startRoom;
            int roomIndex = GetCurrentRoomIndex(currentRoom);
            int randomDirection;

            ExitPath.Add(startRoom);

            //Loop until we find the exit room
            while(currentRoom != exitRoom)
            {
                //Gives 50/50 chance of checking x or y axis for a path to the exit
                randomDirection = GameManager.Random.Next(0, 1);

                if (randomDirection == 0) // check X
                {
                    if(currentRoom.Center.X < exitRoom.Center.X) // currentRoom is to left of exitRoom
                    {
                        roomIndex += columns;                       
                    }
                    else if(currentRoom.Center.X > exitRoom.Center.X) // currentRoom is to right of exitRoom
                    {
                        roomIndex -= columns;
                    }                    
                }
                else // check Y
                {
                    if(currentRoom.Center.Y < exitRoom.Center.Y) // currentRoom is above exitRoom
                    {
                        roomIndex += 1;
                    }
                    else if(currentRoom.Center.Y > exitRoom.Center.Y) // currentRoom is below exitRoom
                    {
                        roomIndex -= 1;
                    }
                }

                //Assign current room to the current room index and adds to the Exit path
                currentRoom = level.Rooms[roomIndex];
                ExitPath.Add(currentRoom);                
            }            
        }

        // GetCurrentRoomIndex()
        // DESC:    Loops through the list of rooms and returns the index of the selected room.
        // PARAMS:  room(Rectangle)
        // RETURNS: x(int) - the index of the room within the Room list
        public int GetCurrentRoomIndex(Rectangle room)
        {
            int x;

            //Loop through the list of rooms and check if current room index matches the room passed in
            for(x = 0; x < level.Rooms.Count; x++)
            {
                //Once room is found, break out of loop and return the index
                if (level.Rooms[x] == room)
                    break;
            }

            return x;
        }

        // PlaceDoorsOnPath()
        // DESC:    Takes the exitPath and places appropriate doors along the path from start to exit.
        // PARAMS:  None.
        // RETURNS: None.  Modifies level to create doors.
        public void PlaceDoorsOnPath() 
        {
            for(int x = 0; x < ExitPath.Count - 1; x++)
            {
                // determine proper wall to place door
                // make door spot walkable
                // add door logic here later if necessary
                if(ExitPath[x+1].Center.X > ExitPath[x].Center.X)       // next room is to the right
                {
                    // open center of right wall in this room and center of left wall in next room
                    level.SetIsWalkable(ExitPath[x].Right, ExitPath[x].Center.Y, true);
                    level.SetIsWalkable(ExitPath[x+1].Left, ExitPath[x+1].Center.Y, true);

                }
                else if(ExitPath[x+1].Center.X < ExitPath[x].Center.X)  // next room is to the left
                {
                    // open center of left wall in this room and center of right wall in next room
                    level.SetIsWalkable(ExitPath[x].Left, ExitPath[x].Center.Y, true);
                    level.SetIsWalkable(ExitPath[x+1].Right, ExitPath[x+1].Center.Y, true);
                }
                else if(ExitPath[x+1].Center.Y > ExitPath[x].Center.Y)  // next room is below
                {
                    // open center of bottom wall in this room and center of top wall in next room
                    level.SetIsWalkable(ExitPath[x].Center.X, ExitPath[x].Bottom, true);
                    level.SetIsWalkable(ExitPath[x+1].Center.X, ExitPath[x+1].Top, true);
                } 
                else if(ExitPath[x+1].Center.Y < ExitPath[x].Center.Y)  // next room is above
                {
                    // open center of top wall in this room and center of bottom wall in next room
                    level.SetIsWalkable(ExitPath[x].Center.X, ExitPath[x].Top, true);
                    level.SetIsWalkable(ExitPath[x+1].Center.X, ExitPath[x+1].Bottom, true);
                }

            }
        }

        // TODO - PlaceRandomDoors()
        // DESC:    Loops through all of the rooms and places doors on random walls.    
        // PARAMS:  None.
        // RETURNS: None. Modifies level to add doors.
        public void PlaceRandomDoors()
        {

        }

    }
}

