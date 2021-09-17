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

            if (kstate.IsKeyDown(Keys.W))
            {
                control.avanzar = 1;
            }
            if (kstate.IsKeyDown(Keys.S))
            {
                control.avanzar = -1;
            }
            if (kstate.IsKeyDown(Keys.D))
            {
                control.virar = -1;
            }
            if (kstate.IsKeyDown(Keys.A))
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
