using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Controller
{
    public class Control
    {
        public int Avanzar;
        public int Virar;
        public Control()
        {
            Avanzar = 0;
            Virar = 0;
        }
        public void Reset()
        {
            Avanzar = 0;
            Virar = 0;
        }

        
    }
}
