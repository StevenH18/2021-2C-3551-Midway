using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Controller
{
    class Player : Controller
    {
        public override Control GetControls()
        {
            Control control = new Control();

            KeyboardState kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.W))
            {
                control.Avanzar = 1;
            }
            if (kstate.IsKeyDown(Keys.S))
            {
                control.Avanzar = -1;
            }
            if (kstate.IsKeyDown(Keys.D))
            {
                control.Virar = -1;
            }
            if (kstate.IsKeyDown(Keys.A))
            {
                control.Virar = 1;
            }
            return control;
        }

        public override void update()
        {
            
        }
    }
}
