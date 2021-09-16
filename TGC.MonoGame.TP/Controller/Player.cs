using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Controller
{
    class Player : Controller
    {
        public override Control GetControlls()
        {
            Control control = new Control();

            KeyboardState kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Y))
            {
                control.avanzar += 1;
            }
            if (kstate.IsKeyDown(Keys.U))
            {
                control.avanzar += -1;
            }
            if (kstate.IsKeyDown(Keys.G))
            {
                control.virar += 1;
            }
            if (kstate.IsKeyDown(Keys.J))
            {
                control.virar += -1;
            }
            return control;
        }

        public override void update()
        {
            
        }
    }
}
