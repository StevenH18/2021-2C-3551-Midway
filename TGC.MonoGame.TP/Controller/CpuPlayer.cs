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
        public int PreviousWayPoint = 0;
        public float WayPointRadius = 500f;
        public Ship Ship;

        private int asd = 0;

        public CpuPlayer()
        {
            Control = new Control();

            var wayPointsSet = new Vector3[][] { 
                /*
                new Vector3[]
                {
                    new Vector3(10000, 0, -9000),
                    new Vector3(15000, 0, -4000),
                    new Vector3(15000, 0, -15000),
                },
                new Vector3[]
                {
                    new Vector3(10000, 0, 9000),
                    new Vector3(15000, 0, 4000),
                    new Vector3(15000, 0, 15000),
                },
                */
                new Vector3[]
                {
                    new Vector3(0, 0, 8000),
                    new Vector3(-12000, 0, 9000),
                    new Vector3(-11000, 0, 0),
                    new Vector3(-11000, 0, -12000),
                    new Vector3(-2000, 0, -14000),
                }
            };
            Random random = new Random();

            WayPoints = wayPointsSet[random.Next(0, wayPointsSet.Length)];
            CurrentWayPoint = 0; // random.Next(0, WayPoints.Length);
        }

        public override Control GetControls()
        {
            Ship.Aceleration = 1f;
            Ship.MaxSpeed = 20f;

            Vector3 wayPoint = WayPoints[CurrentWayPoint];
            Vector3 shipPosition = Ship.World.Translation;

            float distance = Vector3.Distance(wayPoint, shipPosition);

            if(distance < WayPointRadius)
            {
                var previousWayPoint = CurrentWayPoint;
                CurrentWayPoint = ClosestWayPoint(shipPosition);
                PreviousWayPoint = previousWayPoint;
            }

            Vector3 shipForward = Ship.World.Forward;
            Vector3 goTo = wayPoint - shipPosition;

            var angleShip = MathF.Atan2(shipForward.Z, shipForward.X) * 180f / Math.PI;
            var angleWayPoint = MathF.Atan2(goTo.Z, goTo.X) * 180f / Math.PI;

            var angle = angleWayPoint - angleShip;

            /*
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
            */
            KeyboardState kstate = Keyboard.GetState();

            Control.Avanzar = 0;
            Control.Virar = 0;

            if (kstate.IsKeyDown(Keys.W))
            {
                Control.Avanzar = 1;
            }
            if (kstate.IsKeyDown(Keys.S))
            {
                Control.Avanzar = -1;
            }
            if (kstate.IsKeyDown(Keys.D))
            {
                Control.Virar = -1;
            }
            if (kstate.IsKeyDown(Keys.A))
            {
                Control.Virar = 1;
            }

            if (asd % 50 == 0)
            {
                Debug.WriteLine("------------------");
                Debug.WriteLine("ship: "+ angleShip);
                Debug.WriteLine("way: " + angleWayPoint);
                Debug.WriteLine("angle: " + angle);
                EffectSystem.PlayExplosion(wayPoint);
            }

            asd++;
            return Control;
        }

        private int ClosestWayPoint(Vector3 shipPosition)
        {
            int wayPointToGo = 0;
            float minDistance = 500000f;

            for(var i = 0; i < WayPoints.Length; i++)
            {
                var distanceToWayPoint = Vector3.Distance(WayPoints[i], shipPosition);
                if (i != CurrentWayPoint && i != PreviousWayPoint && distanceToWayPoint < minDistance)
                {
                    wayPointToGo = i;
                    minDistance = distanceToWayPoint;
                }
            }

            return wayPointToGo;
        }

        public override void update()
        {
            throw new NotImplementedException();
        }
    }
}
