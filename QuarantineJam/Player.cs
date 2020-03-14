using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;


namespace QuarantineJam
{
    public class Player : PhysicalObject
    {
        private const float MaxSpeed = 15;
        static Sprite idle, run, brake, fall, rise, roll;
        //static SoundEffect ;
        public enum PlayerState { idle, walk, jump, doublejump } //etc
        public PlayerState CurrentState, PreviousState;
        public int state_frames;
        World world;
        Random random = new Random();

        KeyboardState prevKbState;

        public int PlayerDirection;

        new public static void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            idle = new Sprite(2, 193, 168, 350, Content.Load<Texture2D>("player_idle"));
            run = new Sprite(5, 193, 168, 70, Content.Load<Texture2D>("player_run"));
            brake = new Sprite(Content.Load<Texture2D>("brake"));
            rise = new Sprite(Content.Load<Texture2D>("rise"));
            fall = new Sprite(Content.Load<Texture2D>("fall"));
            roll = new Sprite(6, 232, 168, 60, Content.Load<Texture2D>("roll"));
        }
        public Player():base(new Vector2(40, 100), new Vector2(0,0))
        {
            FeetPosition = new Vector2(200, 200);
            CurrentState = PlayerState.idle;
            WallBounceFactor = 0f;
            GroundBounceFactor = 0f;
            GroundFriction = 0.8f;
            Gravity = 0.5f;

            Velocity = new Vector2(0, 0);
            PlayerDirection = 1;
            prevKbState = new KeyboardState();
        }

        public override void Update(GameTime gameTime, World world)
        {
            this.world = world;
            KeyboardState KbState = Keyboard.GetState();

            if (PreviousState == CurrentState) state_frames += 1;
            else state_frames = 0;
            PreviousState = CurrentState;

            switch (CurrentState)
            {
                case PlayerState.idle:
                    /*if (KbState.IsKeyDown(Input.Left) && KbState.IsKeyUp(Input.Right))
                    {
                        if (Velocity.X > -MaxSpeed)
                            ApplyForce(new Vector2(-2f, 0));
                        CurrentState = PlayerState.walk;
                    } else if (KbState.IsKeyDown(Input.Right))
                    {
                        if (Velocity.X < MaxSpeed)
                            ApplyForce(new Vector2(2f, 0));
                        CurrentState = PlayerState.walk;
                    }*/
                    if (!IsOnGround(world)) CurrentState = PlayerState.jump;
                    else if (Input.direction != 0) CurrentState = PlayerState.walk;
                    else if (KbState.IsKeyDown(Input.Jump))
                    {
                        ApplyForce(new Vector2(0, -15f));
                        CurrentState = PlayerState.jump;
                    }
                    break;
                case PlayerState.walk:
                    if (KbState.IsKeyDown(Input.Jump))
                    {
                        // Velocity = 0;
                        ApplyForce(new Vector2(0, -15f));
                        CurrentState = PlayerState.jump;
                    }
                    else if (!IsOnGround(world)) CurrentState = PlayerState.jump;
                    else if (Input.direction != 0) // player is inputing a direction (either left or right)
                    {
                        PlayerDirection = Input.direction;
                        if (Math.Sign(Velocity.X) * Math.Sign(Input.direction) >= 0) // if inputed direction is the same as current movement direction
                        {
                            if (Velocity.X * Velocity.X < MaxSpeed * MaxSpeed) // if norm of velocity below max speed
                                ApplyForce(new Vector2(Input.direction * 2f, 0));
                        }
                        else // if player is inputing the direction against the current movement (brake)
                            ApplyForce(new Vector2(Input.direction * 5f, 0));
                    }
                    else CurrentState = PlayerState.idle;
                    break;

                case PlayerState.jump:
                    if (KbState.IsKeyDown(Input.Jump) && !prevKbState.IsKeyDown(Input.Jump))
                    {
                        Velocity = new Vector2(0, 0);
                        if (Input.direction == 0) ApplyForce(new Vector2(0, -15));
                        else
                        {
                            PlayerDirection = Input.direction;
                            ApplyForce(new Vector2(Input.direction * 10, -10));
                        }
                        CurrentState = PlayerState.doublejump;
                    }
                    else if (Input.direction != 0) // player is inputing a direction (either left or right)
                    {
                        if (Math.Sign(Velocity.X) * Math.Sign(Input.direction) >= 0) // if inputed direction is the same as current movement direction
                        {
                            if (Velocity.X * Velocity.X < 5) // if norm of velocity below max air speed
                                ApplyForce(new Vector2(Input.direction * 2f, 0));
                        }
                        else // if player is inputing the direction against the current movement (brake)
                            ApplyForce(new Vector2(Input.direction * 2f, 0));
                    }
                    if (IsOnGround(world))
                        CurrentState = PlayerState.idle;
                    break;
                case (PlayerState.doublejump):
                {
                        if (Input.direction != 0) // player is inputing a direction (either left or right)
                        {
                            if (Math.Sign(Velocity.X) * Math.Sign(Input.direction) >= 0) // if inputed direction is the same as current movement direction
                            {
                                if (Velocity.X * Velocity.X < 5) // if norm of velocity below max air speed
                                    ApplyForce(new Vector2(Input.direction * 2f, 0));
                            }
                            else // if player is inputing the direction against the current movement (brake)
                                ApplyForce(new Vector2(Input.direction * 2f, 0));
                        }
                        if (IsOnGround(world))
                            CurrentState = PlayerState.idle;
                        break;
                }
            }
            //
            // SPRITE DETERMINATION
            //
            PreviousSprite = CurrentSprite;
            switch (CurrentState)
            {
                case (PlayerState.idle):
                    {
                        if (Velocity.X * Velocity.X < 1) CurrentSprite = idle;
                        else CurrentSprite = brake;
                        break;
                    }
                case (PlayerState.walk):
                    {
                        CurrentSprite = run;
                        break;
                    }
                case (PlayerState.jump):
                    {
                        if (Velocity.Y < -4) CurrentSprite = rise;
                        //else if (Velocity.Y > 4) CurrentSprite = fall;
                        else CurrentSprite = fall;
                        break;
                    }
                case (PlayerState.doublejump):
                    {
                        if (Velocity.Y < 3) CurrentSprite = roll;
                        else CurrentSprite = fall;
                        break;
                    }
            }

            /*if (Input.double_tap_waiting && IsOnGround(world) && CurrentSprite != slug_walk)
            {
                CurrentSprite = slug_charge_attack;
                PlayerDirection = Input.direction;
            }*/

            if (CurrentSprite != PreviousSprite)
            {
                CurrentSprite.ResetAnimation();
                //Console.WriteLine("Switched to sprite " + CurrentSprite.Texture.Name);
            }
            CurrentSprite.direction = PlayerDirection;
            CurrentSprite.UpdateFrame(gameTime);

            PhysicalObject BeeToRemove = null;
            foreach (PhysicalObject o in world.Stuff)
            {
                if (o is Bee b)
                {
                    b.AttractFromPlayer(this);
                    if (CheckCollision(b.Hurtbox, b.Velocity))
                    {
                        //Console.WriteLine("collision between bee and player");
                        BeeToRemove = b;
                    }
                }
                // do something;
            }
            if (BeeToRemove != null)
                world.Stuff.Remove(BeeToRemove);

            prevKbState = KbState;
            base.Update(gameTime, world);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void Death()
        {

        }
    }
}