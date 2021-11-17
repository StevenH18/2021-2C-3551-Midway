using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    static class Inputs
    {
        private static Boolean[] keys = new Boolean[255];
        private static Boolean[] mouse = new Boolean[]{
            false, false, true, true
        };

        public static Boolean isJustPressed(Keys key)
        {
           KeyboardState  kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(key) && !keys[((int)key)]){
                keys[((int)key)] = true;
                return true;
            }
            if (kstate.IsKeyUp(key) && keys[((int)key)])
            {
                keys[((int)key)] = false;
                return false;
            }

            return false;
        }

        public static Boolean mouseLeftJustPressed()
        {
            MouseState mstate = Mouse.GetState();

            if(mstate.LeftButton == ButtonState.Pressed && !mouse[0])
            {
                mouse[0] = true;
                return true;
            }
            if (mstate.LeftButton == ButtonState.Released && mouse[0])
            {
                mouse[0] = false;
                return false;
            }

            return false;
        }
        public static Boolean mouseRightJustPressed()
        {
            MouseState mstate = Mouse.GetState();

            if (mstate.RightButton == ButtonState.Pressed && !mouse[1])
            {
                mouse[1] = true;
                return true;
            }
            if (mstate.RightButton == ButtonState.Released && mouse[1])
            {
                mouse[1] = false;
                return false;
            }

            return false;
        }
        public static Boolean mouseLeftJustReleased()
        {
            MouseState mstate = Mouse.GetState();

            if (mstate.LeftButton == ButtonState.Released && !mouse[2])
            {
                mouse[0] = true;
                return true;
            }
            if (mstate.LeftButton == ButtonState.Pressed && mouse[2])
            {
                mouse[0] = false;
                return false;
            }

            return false;
        }
        public static Boolean mouseRightJustReleased()
        {
            MouseState mstate = Mouse.GetState();

            if (mstate.RightButton == ButtonState.Released && !mouse[3])
            {
                mouse[3] = true;
                return true;
            }
            if (mstate.RightButton == ButtonState.Pressed && mouse[3])
            {
                mouse[3] = false;
                return false;
            }

            return false;
        }
    }
}
