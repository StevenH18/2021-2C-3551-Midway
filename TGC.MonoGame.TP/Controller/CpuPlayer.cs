using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Controller
{
    class CpuPlayer : PlayerController
    {
        Control Control;

        public Vector3[] WayPoints;
        public int CurrentWayPoint = 0;
        public float WayPointRadius = 500f;
        public Ship Ship;

        private int asd = 0;

        public CpuPlayer()
        {
            Control = new Control();
            WayPoints = new Vector3[]
            {
                new Vector3(10000, 0, -9000),
                new Vector3(15000, 0, -4000),
                new Vector3(15000, 0, -15000),
            };

            Random random = new Random();

            CurrentWayPoint = random.Next(0, WayPoints.Length);
        }

        public override Control GetControls()
        {
            Vector3 wayPoint = WayPoints[CurrentWayPoint];
            Vector3 shipPosition = Ship.World.Translation;

            Vector3 shipForward = Ship.World.Forward;
            Vector3 goTo = wayPoint - shipPosition;

            var angleShip = MathF.Atan2(shipForward.Z, shipForward.X) * 180f / Math.PI;
            var angleWayPoint = MathF.Atan2(goTo.Z, goTo.X) * 180f / Math.PI;

            var angle = angleWayPoint - angleShip;

            //Debug.WriteLine(angle);

            float distance = Vector3.Distance(wayPoint, Ship.World.Translation);

            if(distance < WayPointRadius)
            {
                CurrentWayPoint++;
                if (CurrentWayPoint >= WayPoints.Length)
                    CurrentWayPoint = 0;
            }

            /*
            if(EffectSystem != null && asd % 400 == 0)
                EffectSystem.PlayExplosion(wayPoint + Vector3.Up * 300);
            */

            asd++;

            Control.Virar = 0;
            if (angle > 0)
            {
                Control.Virar = -1;
            }
            else if (angle < 0)
            {
                Control.Virar = 1;
            }

            Control.Avanzar = 1;

            return Control;
        }

        public override void update()
        {
            throw new NotImplementedException();
        }
    }
}
