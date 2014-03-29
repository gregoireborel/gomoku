using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gomoku
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager       graphics;
        SpriteBatch                 spriteBatch;
        private Texture2D           _grid, _fond;
        private Texture2D           _blue, _red, _green;
        private bool                _inGame;
        private bool                _clicked;
        private MouseState          _mouseState;
        private MenuButton          _jVsJButton;
        private MenuButton          _jVsBotButton;
        private MenuButton          _menuButton;
        private typeOfGame          _typeOfGame;
        private Referee             _referee;
        private Player              _j1, _j2;
        private SpriteFont          _font;
        private Ia                  _ia;
        private bool                _doubleThree, _breakingFive;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.graphics.IsFullScreen = false;
            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.ApplyChanges();
            this.IsMouseVisible = true;

            this.Window.Title = "Gomoku";
            this.Window.AllowUserResizing = false;

            _inGame = false;
            _clicked = false;
            _referee = new Referee();
            _ia = new Ia();
            _j1 = new Player();
            _j2 = new Player();
            _font = Content.Load<SpriteFont>("font");

            _doubleThree = true;
            _breakingFive = false;

            #region IA_INIT

            /*Cette partie sera a placer apres avoir recuperer les infos du serveur*/
            _ia.setDoubleThree(_doubleThree);
            _ia.setBreakingFive(_breakingFive);
            _ia.setTimeout(0);

            #endregion



            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _fond = Content.Load<Texture2D>("fond_title");
            _grid = Content.Load<Texture2D>("gomoku_grid");
            _blue = Content.Load<Texture2D>("black_jeton");
            _red = Content.Load<Texture2D>("white_jeton");
            _green = Content.Load<Texture2D>("smiley");
            _menuButton = new MenuButton(Content.Load<Texture2D>("menuButton"), Content.Load<Texture2D>("menuButton_overlay"), 32, 32);
            _menuButton._position.X = 750; _menuButton._position.Y = 550;
            _jVsJButton = new MenuButton(Content.Load<Texture2D>("J_vs_J_normal"),
                                            Content.Load<Texture2D>("J_vs_J_overlay"), 
                                            400, 100);
            _jVsJButton._position.X = 200; _jVsJButton._position.Y = 160;
            _jVsBotButton = new MenuButton(Content.Load<Texture2D>("J_vs_IA_normal"),
                                            Content.Load<Texture2D>("J_vs_IA_overlay"),
                                            400, 100);
            _jVsBotButton._position.X = 200; _jVsBotButton._position.Y = 320;

        }

        protected override void UnloadContent()
        {

        }

        private void            resetGrid()
        {
            _inGame = false;
            _referee.clearData();
            _j1.setTokens(0); _j2.setTokens(0);
            _referee._count = 0;
            _ia.reset();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            _mouseState = Mouse.GetState();

            if (_inGame)
            {
               if (_referee._status == gameStatus.WIN)
                {
                    _clicked = false;
                    if (_menuButton.Update(_mouseState.X, _mouseState.Y) && (_mouseState.LeftButton == ButtonState.Pressed)
                    && _clicked == false)
                        resetGrid();                   
                }
                else
                {
                    if (_typeOfGame == typeOfGame.JVSJ)
                    {
                        #region JVSJ

                        if (_menuButton.Update(_mouseState.X, _mouseState.Y) && (_mouseState.LeftButton == ButtonState.Pressed)
                    && _clicked == false)
                            resetGrid();
                        if (_clicked == false && _mouseState.LeftButton == ButtonState.Pressed)
                        {
                            _clicked = true;
                           if (_referee._turn == playerTurn.J1)
                               _referee.Update(_mouseState.X, _mouseState.Y, ref _j1._tokens, 0);
                           else
                               _referee.Update(_mouseState.X, _mouseState.Y, ref _j2._tokens, 0);
                        }
                        if (_mouseState.LeftButton == ButtonState.Released)
                            _clicked = false;

                        #endregion
                    }
                    else
                    {
                       if (_menuButton.Update(_mouseState.X, _mouseState.Y) && (_mouseState.LeftButton == ButtonState.Pressed) && _clicked == false)
                            resetGrid();
                       if (_clicked == false && _mouseState.LeftButton == ButtonState.Pressed)
                       {
                           _clicked = true;
                           if (_referee._turn == playerTurn.J1)
                           {
                               List<Vector2> caseNb = _referee.Update(_mouseState.X, _mouseState.Y, ref _j1._tokens, 0);

                               #region IA_MAJ_INFO_SERVEUR

                               if (_referee._turn == playerTurn.J2)
                               {
                                   for (int i = 0; i < caseNb.Count(); ++i)
                                   {
                                       if (i == 0)
                                       {
                                           _ia.updateCase((int)caseNb[i].X, (int)caseNb[i].Y, 1);
                                           _ia.setLastPlay(caseNb[i]);
                                       }
                                       else
                                           _ia.updateCase((int) caseNb[i].X, (int) caseNb[i].Y, 0);
                                   }
                                   _ia.setCaptureEnnemi(_j1._tokens);
                               }

                               #endregion

                           }
                       }
                       if (_referee._turn == playerTurn.J2)
                       {
                           /*faire le truc du bot*/
                           //_referee.UpdateBot();

                           #region BOT_EXECUTION

                           // APRES AVOIR RECUPERER LES INFOS DU SERVEUR ET SI C'EST LE TOUR DE L'IA

                           Vector2 coord = _ia.findPlay();
                           List<Vector2> caseNb = _referee.Update(coord.X, coord.Y, ref _j2._tokens, 1);

                           if (_referee._turn == playerTurn.J1)
                           {
                               for (int i = 0; i < caseNb.Count(); ++i)
                               {
                                   if (i == 0)
                                   {
                                       _ia.updateCase((int)caseNb[i].X, (int)caseNb[i].Y, 2);
                                       _ia.setLastPlay(caseNb[i]);
                                   }
                                   else
                                       _ia.updateCase((int)caseNb[i].X, (int)caseNb[i].Y, 0);
                               }
                               _ia.setCapture(_j2._tokens);
                           }

                           // - Vérifie si victoire/defaite
	                       //     - Si Oui 
		                   //         => leave
                           // - Boucle sur tout*/

                           #endregion



                       }
                       if (_mouseState.LeftButton == ButtonState.Released)
                               _clicked = false;
                    }
                }
            }
            else
            {
                if (_jVsJButton.Update(_mouseState.X, _mouseState.Y) && (_mouseState.LeftButton == ButtonState.Pressed) 
                    && _clicked == false)
                {
                    _clicked = true;
                    _inGame = true;
                    _typeOfGame = typeOfGame.JVSJ;
                }
                if (_jVsBotButton.Update(_mouseState.X, _mouseState.Y) && (_mouseState.LeftButton == ButtonState.Pressed) 
                    && _clicked == false)
                {
                    _clicked = true;
                    _inGame = true;
                    _typeOfGame = typeOfGame.JVSB;
                }
                if (_mouseState.LeftButton == ButtonState.Released)
                    _clicked = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Vector2             vector = new Vector2(0, 0);
            String              text = "";

            spriteBatch.Begin();
            if (_inGame)
            {
                spriteBatch.Draw(_grid, vector, Color.White);
               
                spriteBatch.Draw(_menuButton.Draw(), _menuButton._position, Color.White);
                _referee.Draw(ref spriteBatch, ref _blue, ref _red, ref _font);

                /*Texte J1*/
                text = "Joueur 1";
                vector.X = 620; vector.Y = 66;
                spriteBatch.DrawString(_font, text, vector, Color.Black);
                vector.X = 720; vector.Y = 66;
                spriteBatch.Draw(_blue, vector, Color.White);
                text = "Pierres prises:";
                vector.X = 620; vector.Y = 106;
                spriteBatch.DrawString(_font, text, vector, Color.Black);
                text = "" + _j1.getTokens();
                vector.X = 620; vector.Y = 126;
                spriteBatch.DrawString(_font, text, vector, Color.Black);

                /*Texte J2*/
                text = "Joueur 2";
                vector.X = 620; vector.Y = 306;
                spriteBatch.DrawString(_font, text, vector, Color.Black);
                vector.X = 720; vector.Y = 306;
                spriteBatch.Draw(_red, vector, Color.White);
                text = "Pierres prises:";
                vector.X = 620; vector.Y = 346;
                spriteBatch.DrawString(_font, text, vector, Color.Black);
                if (_typeOfGame == typeOfGame.JVSJ)
                    text = "" + _j2.getTokens();
                else
                    text = "" + _ia.getCapture();
                vector.X = 620; vector.Y = 366;
                spriteBatch.DrawString(_font, text, vector, Color.Black);

                
              if (_referee._status == gameStatus.WIN)
                {
                    vector.X = 620; vector.Y = 450;
                    if (_j1.getTokens() >= 10)
                        text = "Joueur 1 gagne !";
                    else if (_j2.getTokens() >= 10)
                        text = "Joueur 2 gagne !";
                    else if (_referee._winner == 1)
                        text = "Joueur 1 gagne !";
                    else
                        text = "Joueur 2 gagne !";
                  spriteBatch.DrawString(_font, text, vector, Color.Black);
                 }
            }
            else
            {
                spriteBatch.Draw(_fond, vector, Color.White);
                spriteBatch.Draw(_jVsJButton.Draw(), _jVsJButton._position, Color.White);
                spriteBatch.Draw(_jVsBotButton.Draw(), _jVsBotButton._position, Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

