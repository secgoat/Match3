using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BasicMatch3
{
   
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont font;

		MouseState mouseState;
		MouseState mousePrevState;
		Point mouseCords;
		Rectangle mouseRect;
	
		Random rand;
		int [,] board = new int[10, 10];

		//use this structure to keep a list of each cell clicked, need to add some  
		//logic to make clear the list of more than 2 selected
		Point?[] selected = new Point?[2];
		
		//the following are to help check for mathicng cells in one of 4 directions

		Point up = new Point(0, -1);
		Point down = new Point(0, 1);
		Point left = new Point(-1, 0);
		Point right = new Point(1, 0);
		/* use this to set true /false to see if we should check in any give direction
		 *  0 up
		 *  1 doen
		 *  2 left
		 *  3 right
		 *  reset all to true when we clear the selected list
		 *  check pos1 - pos2 to see which is left from the POint list above up/down/left/right and we can set that to
		 *  false automatically so we are not checking the spot in which it came from.
		 */
		enum CheckDirs
		{
			up,
			down,
			left,
			right
		};
		bool[] pos1CheckDirs = new bool[4];
		bool[] pos2CheckDirs = new bool[4];
		/*
		 * now the things we need to keep track of what is in each position, and how to match it against the right pos
		 * to check for matches
		 * IE: value of pos1 should be checked against the numbers surrounding pos2
		 * actually i can just use selected[0] and [1] as my data, so you check selected[0] against the numbers surrounidng pos2
		 * and use selected[1] to check against the numbers surrounding pos1
		 */

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

	   
		protected override void Initialize()
		{
			rand = new Random();
			int xMax = board.GetUpperBound(0);
			int yMax = board.GetUpperBound(1);
			for (int x = 0; x <= xMax; x++)
			{
				for (int y = 0; y <= yMax; y++)
				{
					int num = rand.Next(0, 5);
					board[x, y] = num;
				}
			}
			IsMouseVisible = true;
			base.Initialize();

		}

	   
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			font = Content.Load<SpriteFont>("Font");
		}

		
		protected override void UnloadContent()
		{
		}

		
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();
			mousePrevState = mouseState;
			mouseState = Mouse.GetState();
			mouseCords = new Point(Mouse.GetState().X, Mouse.GetState().Y);
			mouseRect = new Rectangle(mouseCords.X, mouseCords.Y, 1, 1);
			


			base.Update(gameTime);
		}

	   
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin();
			Color drawColor;
			Color color = Color.Firebrick;
			Color hilite = Color.LightGoldenrodYellow;

			int xMax = board.GetUpperBound(0);
			int yMax = board.GetUpperBound(1);
			//Set the color to draw the circle as
			for (int x = 0; x <= xMax; x++)
			{
				for (int y = 0; y <= yMax; y++)
				{
					if (MouseHover(x,y))
						drawColor = hilite;
					else if(selected.Contains(new Point(x,y)))
						drawColor = hilite;
					else
						drawColor = color;
					spriteBatch.DrawString(font, board[x,y].ToString(), new Vector2((x * 15), (y * 15)), drawColor); //15 is the size of the font (14) + 1 pixel for spacing
				}
				
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}

		protected bool MouseHover(int x, int y)
		{
			//create a rectangle we can chekc for collisons with that shoudl surround each cell
			Rectangle arrayRect = new Rectangle((x * 15) , (y * 15), 15, 15); //15 is the size of the font(14) + 1 pixel for padding
			//check for mouse / cell intersection
			if (mouseRect.Intersects(arrayRect))
			{
				//this will only be true if the left mouse button was released and pressed, otherwise it fills selected[] with the same cell
				//this is like using single click instead of registerign click over and over
				if (mousePrevState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
				{
					//if the first slot in slected is empty fill it else fill the second slot, else clear it
					if (selected[0] == null)
						selected[0] = new Point(x, y);
					else if (selected[1] == null)
                    {
                        selected[1] = new Point(x, y);
                        if (selected[0] != null && selected[1] != null)
                        {
                            CheckForValidMove();
                            for (int i = 0; i < 2; i++)
                            {
                                selected[i] = null;
                            }
                            pos1CheckDirs = new bool[4];
                            pos2CheckDirs = new bool[4];
                        }
                    }
						
				/*	else if (selected[0] != null && selected[1] != null)
					{
						CheckForValidMove();
						for (int i = 0; i < 2; i++ )
						{
							selected[i] = null;
						}
						pos1CheckDirs = new bool[4];
                        pos2CheckDirs = new bool[4];
                    } */

				}
				return true;
			}
				
			else
				return false;
		}

		private void CheckForValidMove()
		{
			/* have a list of bools for position1 and position2
			 * hold for up down left right, check each direction for matching numbers if matching numbers >= 2 then true
			 * else false only return valid move if one of the 4 bools is true else return false
			 * use a recursive function:
			 * if(pos1 + up == board[pos1.x][pos1.y])
			 *      incrment pos one to up while remberign intial pos
			 *      send new pos1 to check for valid moves to check next space up
			 * else
			 *     bool up = false
			 *     
			 * 
			 * or alternatively could i say:
			 *  if(board[po1.x][pos1.y] == board[pos1.x -1][pos1.y] &board[pos1.x -2][pos1.y])
			 *          this means there is a valid match going up ( but no, that will nbot work what if it is more than three.
			 *          I think I need to check each of the 3 surrounding tiles and see if any of them match then recurse in that direction
			 *          until the numbers no longer match and if matches + pos1 >= 3 then remove those tiles
			 */
			var pos1 = selected[0];
			var pos2 = selected[1];
			//get the differences between pos1 and pos 2 each of these should return a point that match the 
			//up / down / left / right points and will let us knwo what direction not to check in
			Point diff1 = new Point((pos1.Value.X - pos2.Value.X), (pos1.Value.Y - pos2.Value.Y)); // this value should be applied to pos2 in which way not to check
			Point diff2 = new Point((pos2.Value.X - pos1.Value.X), (pos2.Value.Y - pos1.Value.Y)); //same for this, pos1 not check this dir
			/* need to set each of the 4 directions to check to true, then iterate through each and find out which are false 
			 * then send it to a recursive function that tells us which pieces match and which do not.
			 */
			for (int i = 0; i < 4; i++)
			{
				pos2CheckDirs[i] = true;
				pos1CheckDirs[i] = true;
			}
			if (diff1 == up || pos1.Value.Y == 0)
				pos2CheckDirs[0] = false; //0 is up in the array so we set it to false since it came from up,
										//or if pos1 is already at the top no reason to test up any furtherr
			if (diff1 == down || pos1.Value.Y == 9)
				pos2CheckDirs[1] = false; //checks for down direction and marks it as false

			if (diff1 == left || pos1.Value.X == 0)
				pos2CheckDirs[2] = false;
			if (diff1 == right || pos1.Value.X == 9)
				pos2CheckDirs[3] = false;

			if (diff2 == up || pos2.Value.Y == 0)
				pos1CheckDirs[0] = false; //0 is up in the array so we set it to false since it came from up,
			//or if pos1 is already at the top no reason to test up any furtherr
			if (diff2 == down || pos2.Value.Y == 9)
				pos1CheckDirs[1] = false; //checks for down direction and marks it as false

			if (diff2 == left || pos2.Value.X == 0)
				pos1CheckDirs[2] = false;
			if (diff2 == right || pos2.Value.X == 9)
				pos1CheckDirs[3] = false;
		}
	}
}
