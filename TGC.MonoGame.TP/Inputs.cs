using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    static class Inputs
    {
        private static Boolean[] keys= new Boolean[255];

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

    }
}
