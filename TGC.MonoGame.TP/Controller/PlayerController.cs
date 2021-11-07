using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Effects;

namespace TGC.MonoGame.TP.Controller
{
    public abstract class PlayerController
    {
        public EffectSystem EffectSystem;
        public abstract Control GetControls();

        public abstract void update();

    }
}
