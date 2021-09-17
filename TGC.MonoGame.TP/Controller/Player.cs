using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Controller
{
    class Player : Controller
    {
        public override Controll GetControlls()
        {
            Controll control = new Controll();

            KeyboardState kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.NumPad8))
            {
                control.avanzar = 1;
            }
            if (kstate.IsKeyDown(Keys.NumPad5))
            {
                control.avanzar = -1;
            }
            if (kstate.IsKeyDown(Keys.NumPad6))
            {
                control.virar = -1;
            }
            if (kstate.IsKeyDown(Keys.NumPad4))
            {
                control.virar = 1;
            }
            return control;
        }

        public override void update()
        {
            
        }
    }
}
