using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Controller
{
    public abstract class Controller
    {
        public abstract Control GetControlls();

        public abstract void update();

    }
}
