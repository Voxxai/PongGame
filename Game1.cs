using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _ballTexture, _paddleTexture, _boundaryTexture;
        private Vector2 _ballPosition, _ballVelocity;
        private Rectangle _boundary;
        private Rectangle[] _paddles;
        private int[] _scores;
        private int[] _lives;
        private bool[] _isPlayerControlled;
        private KeyboardState _previousKeyboardState;
        private Random _random = new Random();
        private SpriteFont _scoreFont;
        private SpriteFont _controlsFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();
            _ballPosition = new Vector2(512, 384); // Center of the window
            ResetBall(); // Set initial velocity
            _scores = new int[4];
            _lives = new int[] { 5, 5, 5, 5 };
            // Initialize lives to 5 for all paddles
            _isPlayerControlled = new bool[4];

            // Define the square boundary for gameplay (centered in the window)
            _boundary = new Rectangle(162, 134, 500, 500);

            // Define paddles (Top, Bottom, Left, Right)
            _paddles = new Rectangle[4];
            _paddles[0] = new Rectangle(412, _boundary.Top + 10, 100, 10); // Top
            _paddles[1] = new Rectangle(412, _boundary.Bottom - 20, 100, 10); // Bottom
            _paddles[2] = new Rectangle(_boundary.Left + 10, 284, 10, 100); // Left
            _paddles[3] = new Rectangle(_boundary.Right - 20, 284, 10, 100); // Right

            // Adjust paddles to the middle of their respective sides
            _paddles[0].X = _boundary.Left + (_boundary.Width - _paddles[0].Width) / 2; // Top
            _paddles[1].X = _boundary.Left + (_boundary.Width - _paddles[1].Width) / 2; // Bottom
            _paddles[2].Y = _boundary.Top + (_boundary.Height - _paddles[2].Height) / 2; // Left
            _paddles[3].Y = _boundary.Top + (_boundary.Height - _paddles[3].Height) / 2; // Right
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create ball texture
            _ballTexture = new Texture2D(GraphicsDevice, 10, 10);
            Color[] ballColor = new Color[10 * 10];
            for (int i = 0; i < ballColor.Length; i++) ballColor[i] = Color.White;
            _ballTexture.SetData(ballColor);

            // Create paddle texture
            _paddleTexture = new Texture2D(GraphicsDevice, 1, 1);
            _paddleTexture.SetData(new Color[] { Color.White });

            // Create boundary texture
            _boundaryTexture = new Texture2D(GraphicsDevice, 1, 1);
            _boundaryTexture.SetData(new Color[] { Color.White });

            // Load fonts
            _scoreFont = Content.Load<SpriteFont>("ScoreFont");
            _controlsFont = Content.Load<SpriteFont>("ControlsFont");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
            UpdateBall();
            UpdatePaddles(keyboardState);
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void RemovePaddle(int index)
        {
            _paddles[index] = Rectangle.Empty; // Remove the paddle
            _isPlayerControlled[index] = false; // Disable controls
        }

        private bool IsSideSolid(int index)
        {
            return _lives[index] <= 0;
        }

        private void UpdateBall()
        {
            _ballPosition += _ballVelocity;
            Rectangle ballRect = new Rectangle((int)_ballPosition.X, (int)_ballPosition.Y, 10, 10);

            // Ball collision with paddles
            bool hitPaddle = false;
            for (int i = 0; i < _paddles.Length; i++)
            {
                if (_paddles[i] != Rectangle.Empty && _paddles[i].Intersects(ballRect))
                {
                    hitPaddle = true;
                    if (i < 2) // Top or Bottom paddle
                    {
                        float ballCenter = _ballPosition.X + 5;
                        float paddleCenter = _paddles[i].X + _paddles[i].Width / 2;
                        float offset = (ballCenter - paddleCenter) / (_paddles[i].Width / 2);
                        _ballVelocity.Y *= -1;
                        _ballVelocity.X += offset * 2;
                    }
                    else // Left or Right paddle
                    {
                        float ballCenter = _ballPosition.Y + 5;
                        float paddleCenter = _paddles[i].Y + _paddles[i].Height / 2;
                        float offset = (ballCenter - paddleCenter) / (_paddles[i].Height / 2);
                        _ballVelocity.X *= -1;
                        _ballVelocity.Y += offset * 2;
                    }

                    // Normalize and slightly increase speed
                    _ballVelocity = Vector2.Normalize(_ballVelocity) * (_ballVelocity.Length() + 0.5f);
                }
            }

            // Ball collision with solid walls (sides with zero lives)
            if (_ballPosition.X < _boundary.Left && IsSideSolid(2))
            {
                _ballVelocity.X *= -1; // Reflect off the solid left wall
            }

            if (_ballPosition.X + 10 > _boundary.Right && IsSideSolid(3))
            {
                _ballVelocity.X *= -1; // Reflect off the solid right wall
            }

            if (_ballPosition.Y < _boundary.Top && IsSideSolid(0))
            {
                _ballVelocity.Y *= -1; // Reflect off the solid top wall
            }

            if (_ballPosition.Y + 10 > _boundary.Bottom && IsSideSolid(1))
            {
                _ballVelocity.Y *= -1; // Reflect off the solid bottom wall
            }

            // Ball missed paddle (if side is not solid)
            if (!hitPaddle)
            {
                if (_ballPosition.X < _boundary.Left && !IsSideSolid(2))
                {
                    _lives[2]--;
                    if (_lives[2] <= 0) RemovePaddle(2);
                    ResetBall();
                }

                if (_ballPosition.X + 10 > _boundary.Right && !IsSideSolid(3))
                {
                    _lives[3]--;
                    if (_lives[3] <= 0) RemovePaddle(3);
                    ResetBall();
                }

                if (_ballPosition.Y < _boundary.Top && !IsSideSolid(0))
                {
                    _lives[0]--;
                    if (_lives[0] <= 0) RemovePaddle(0);
                    ResetBall();
                }

                if (_ballPosition.Y + 10 > _boundary.Bottom && !IsSideSolid(1))
                {
                    _lives[1]--;
                    if (_lives[1] <= 0) RemovePaddle(1);
                    ResetBall();
                }
            }
        }

        private void ResetBall()
        {
            _ballPosition = new Vector2(_boundary.Left + _boundary.Width / 2, _boundary.Top + _boundary.Height / 2);
            float speed = 3f;
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            _ballVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
        }

        private void UpdatePaddles(KeyboardState keyboardState)
        {
            // Check for player input to take control of paddles
            if (keyboardState.IsKeyDown(Keys.Q) || keyboardState.IsKeyDown(Keys.E)) _isPlayerControlled[0] = true;
            if (keyboardState.IsKeyDown(Keys.U) || keyboardState.IsKeyDown(Keys.O)) _isPlayerControlled[1] = true;
            if (keyboardState.IsKeyDown(Keys.F) || keyboardState.IsKeyDown(Keys.V)) _isPlayerControlled[2] = true;
            if (keyboardState.IsKeyDown(Keys.J) || keyboardState.IsKeyDown(Keys.N)) _isPlayerControlled[3] = true;

            // Top paddle (left and right control)
            if (_isPlayerControlled[0])
            {
                if (keyboardState.IsKeyDown(Keys.Q) && _paddles[0].X > _boundary.Left) _paddles[0].X -= 5;
                if (keyboardState.IsKeyDown(Keys.E) && _paddles[0].X + _paddles[0].Width < _boundary.Right)
                    _paddles[0].X += 5;
            }

            // Bottom paddle (left and right control)
            if (_isPlayerControlled[1])
            {
                if (keyboardState.IsKeyDown(Keys.U) && _paddles[1].X > _boundary.Left) _paddles[1].X -= 5;
                if (keyboardState.IsKeyDown(Keys.O) && _paddles[1].X + _paddles[1].Width < _boundary.Right)
                    _paddles[1].X += 5;
            }

            // Left paddle (up and down control)
            if (_isPlayerControlled[2])
            {
                if (keyboardState.IsKeyDown(Keys.F) && _paddles[2].Y > _boundary.Top) _paddles[2].Y -= 5;
                if (keyboardState.IsKeyDown(Keys.V) && _paddles[2].Y + _paddles[2].Height < _boundary.Bottom)
                    _paddles[2].Y += 5;
            }

            // Right paddle (up and down control)
            if (_isPlayerControlled[3])
            {
                if (keyboardState.IsKeyDown(Keys.J) && _paddles[3].Y > _boundary.Top) _paddles[3].Y -= 5;
                if (keyboardState.IsKeyDown(Keys.N) && _paddles[3].Y + _paddles[3].Height < _boundary.Bottom)
                    _paddles[3].Y += 5;
            }

            // Control paddles with AI if not player-controlled
            ControlPaddlesWithAI();
        }

        private void ControlPaddlesWithAI()
        {
            float aiSpeed = 7f; // Adjust this value to control the AI speed

            for (int i = 0; i < _paddles.Length; i++)
            {
                if (!_isPlayerControlled[i])
                {
                    if (i < 2) // Top or Bottom paddle
                    {
                        float targetX = _ballPosition.X - _paddles[i].Width / 2;
                        _paddles[i].X += (int)MathHelper.Clamp(targetX - _paddles[i].X, -aiSpeed, aiSpeed);
                    }
                    else // Left or Right paddle
                    {
                        float targetY = _ballPosition.Y - _paddles[i].Height / 2;
                        _paddles[i].Y += (int)MathHelper.Clamp(targetY - _paddles[i].Y, -aiSpeed, aiSpeed);
                    }

                    // Ensure paddles stay within the boundary
                    if (_paddles[i].X < _boundary.Left) _paddles[i].X = _boundary.Left;
                    if (_paddles[i].X + _paddles[i].Width > _boundary.Right)
                        _paddles[i].X = _boundary.Right - _paddles[i].Width;
                    if (_paddles[i].Y < _boundary.Top) _paddles[i].Y = _boundary.Top;
                    if (_paddles[i].Y + _paddles[i].Height > _boundary.Bottom)
                        _paddles[i].Y = _boundary.Bottom - _paddles[i].Height;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            // Draw boundary
            _spriteBatch.Draw(_boundaryTexture, new Rectangle(_boundary.Left, _boundary.Top, _boundary.Width, 2),
                Color.White);
            _spriteBatch.Draw(_boundaryTexture, new Rectangle(_boundary.Left, _boundary.Bottom, _boundary.Width, 2),
                Color.White);
            _spriteBatch.Draw(_boundaryTexture, new Rectangle(_boundary.Left, _boundary.Top, 2, _boundary.Height),
                Color.White);
            _spriteBatch.Draw(_boundaryTexture, new Rectangle(_boundary.Right, _boundary.Top, 2, _boundary.Height),
                Color.White);

            // Draw ball
            _spriteBatch.Draw(_ballTexture, _ballPosition, Color.White);

            // Draw paddles
            foreach (Rectangle paddle in _paddles)
            {
                _spriteBatch.Draw(_paddleTexture, paddle, Color.White);
            }

            // Draw lives outside the playing area with the score font
            Vector2 topLivesPosition =
                new Vector2(_boundary.Left + (_boundary.Width - _scoreFont.MeasureString(_lives[0].ToString()).X) / 2,
                    _boundary.Top - 60);
            Vector2 bottomLivesPosition =
                new Vector2(_boundary.Left + (_boundary.Width - _scoreFont.MeasureString(_lives[1].ToString()).X) / 2,
                    _boundary.Bottom + 40);
            Vector2 leftLivesPosition = new Vector2(_boundary.Left - 60,
                _boundary.Top + (_boundary.Height - _scoreFont.MeasureString(_lives[2].ToString()).Y) / 2);
            Vector2 rightLivesPosition = new Vector2(_boundary.Right + 40,
                _boundary.Top + (_boundary.Height - _scoreFont.MeasureString(_lives[3].ToString()).Y) / 2);

            _spriteBatch.DrawString(_scoreFont, _lives[0].ToString(), topLivesPosition, Color.White); // Top
            _spriteBatch.DrawString(_scoreFont, _lives[1].ToString(), bottomLivesPosition, Color.White); // Bottom
            _spriteBatch.DrawString(_scoreFont, _lives[2].ToString(), leftLivesPosition, Color.White); // Left
            _spriteBatch.DrawString(_scoreFont, _lives[3].ToString(), rightLivesPosition, Color.White); // Right

            // Draw controls
            _spriteBatch.DrawString(_controlsFont, "Top Paddle: Q (Left), E (Right)", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_controlsFont, "Bottom Paddle: U (Left), O (Right)", new Vector2(10, 30),
                Color.White);
            _spriteBatch.DrawString(_controlsFont, "Left Paddle: F (Up), V (Down)", new Vector2(10, 50), Color.White);
            _spriteBatch.DrawString(_controlsFont, "Right Paddle: J (Up), N (Down)", new Vector2(10, 70), Color.White);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}