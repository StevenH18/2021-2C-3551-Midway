using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Controller
{
    public class Controll
    {
        public int avanzar;
        public int virar;
        public Controll()
        {
            avanzar = 0;
            virar = 0;
        }
        public void reset()
        {
            avanzar = 0;
            virar = 0;
        }

        
    }
}
