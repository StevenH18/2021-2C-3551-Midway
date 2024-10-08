﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Artillery;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Hud;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    /// 
    public enum RenderState
    {
        Default,
        HeightMap
    }
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        public GraphicsDeviceManager Graphics { get; }

        public FreeCamera FreeCamera;
        public ShipCamera ShipCamera;
        public AimingCamera AimingCamera;
        public DefeatedCamera DefeatedCamera;
        public MenuCamera MenuCamera;
        public Camera ActiveCamera;
        public Camera CurrentCamera;
        public Camera PreviousCamera;
        private float CameraTransition = 0;
        private float CameraTransitionDuration = 2;

        private ShipsSystem ShipsSystem;
        private MapEnvironment Environment;
        private HudController Hud;
        private WeaponSystem WeaponSystem;
        private EffectSystem EffectSystem;

        private RenderState RenderState;
        private RenderTarget2D MainSceneRender;
        private RenderTarget2D HeightMapRender;

        private ShipA MenuShip;

        private Gizmos Gizmos;
        private bool DrawGizmos;

        private bool Crosshair;

        private Menu Menu;
        public GameState GameStatus;
        public enum GameState
        {
            Menu,
            Playing,
            Dead
        }

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            //Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Configuro el tamaño de la pantalla
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Graphics.ApplyChanges();

            // GraphicsDevice.BlendState = BlendState.AlphaBlend;

            FreeCamera = new FreeCamera(GraphicsDevice, this.Window);
            ShipCamera = new ShipCamera(GraphicsDevice, this.Window);
            AimingCamera = new AimingCamera(GraphicsDevice, this.Window);
            DefeatedCamera = new DefeatedCamera(GraphicsDevice, this.Window);
            MenuCamera = new MenuCamera(GraphicsDevice, this.Window);
            ActiveCamera = new Camera();
            CurrentCamera = MenuCamera;
            PreviousCamera = MenuCamera;

            MenuCamera.Position = new Vector3(16000, 50, 0);
            MenuCamera.Forward = new Vector3(-1f, 0f, 0);

            ActiveCamera.World = MenuCamera.World;
            ActiveCamera.Projection = MenuCamera.Projection;
            ActiveCamera.View = MenuCamera.View;

            Gizmos = new Gizmos();
            ShipsSystem = new ShipsSystem(GraphicsDevice, Content, Gizmos);
            Environment = new MapEnvironment(GraphicsDevice, Content, Gizmos);
            Hud = new HudController(GraphicsDevice, Content);
            Menu = new Menu(GraphicsDevice, Content, this);
            EffectSystem = new EffectSystem(GraphicsDevice, Content);
            WeaponSystem = new WeaponSystem(GraphicsDevice, Content, EffectSystem, Environment, ShipsSystem, Gizmos);

            RenderState = RenderState.Default;
            MainSceneRender = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);
            HeightMapRender = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);

            // Menu Scene
            MenuShip = new ShipPlayer(Content, GraphicsDevice, Gizmos);
            MenuShip.Position.X = 13700f;
            MenuShip.Position.Z = 150f;
            MenuShip.Angle = -1f;
            MenuShip.Acceleration = 0f;

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            ShipsSystem.Load();
            Environment.Load();
            Hud.Load();
            EffectSystem.Load();
            Gizmos.LoadContent(GraphicsDevice, Content);
            MenuShip.Load();

            base.LoadContent();
        }

        private Matrix RotationFromMatrix(Matrix view)
        {
            //Vector3 translation = view.Translation;

            view.Translation = Vector3.Zero;

            Vector3 scale = new Vector3();
            scale.X = view.Right.Length();
            scale.Y = view.Up.Length();
            scale.Z = view.Forward.Length();

            view.Right /= scale.X;
            view.Up /= scale.Y;
            view.Forward /= scale.Z;

            return view;
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            if (Inputs.isJustPressed(Keys.F11))
            {
                Graphics.ToggleFullScreen();
                Graphics.ApplyChanges();
            }

            if(ShipsSystem.Ships[0].Health <= 0)
            {
                GameStatus = GameState.Dead;
            }

            float t = CameraTransition / CameraTransitionDuration;
            t = t * t * (3f - 2f * t);

            ActiveCamera.Projection = Matrix.Lerp(PreviousCamera.Projection, CurrentCamera.Projection, t);

            /*
            Vector3 preCamViewTrans = PreviousCamera.View.Translation;
            Quaternion preCamViewQuat = Quaternion.CreateFromRotationMatrix(RotationFromMatrix(PreviousCamera.View));

            Vector3 currCamViewTrans = CurrentCamera.View.Translation;
            Quaternion currCamViewQuat = Quaternion.CreateFromRotationMatrix(RotationFromMatrix(CurrentCamera.View));

            Quaternion rotationViewSlerp = Quaternion.Slerp(preCamViewQuat, currCamViewQuat, t);
            Vector3 translationViewLerp = Vector3.Lerp(preCamViewTrans, currCamViewTrans, t);
            */

            Vector3 preCamWorldTrans = PreviousCamera.World.Translation;
            Quaternion preCamWorldQuat = Quaternion.CreateFromRotationMatrix(RotationFromMatrix(PreviousCamera.World));

            Vector3 currCamWorldTrans = CurrentCamera.World.Translation;
            Quaternion currCamWorldQuat = Quaternion.CreateFromRotationMatrix(RotationFromMatrix(CurrentCamera.World));

            Quaternion rotationWorldSlerp = Quaternion.Slerp(preCamWorldQuat, currCamWorldQuat, t);
            Vector3 translationWorldLerp = Vector3.Lerp(preCamWorldTrans, currCamWorldTrans, t);

            //ActiveCamera.View = Matrix.Lerp(PreviousCamera.View, CurrentCamera.View, t);
            //ActiveCamera.View = Matrix.CreateTranslation(translationViewLerp);
            //ActiveCamera.View = Matrix.CreateFromQuaternion(rotationViewSlerp) * Matrix.CreateTranslation(translationViewLerp);
            ActiveCamera.View = Matrix.CreateLookAt(translationWorldLerp, translationWorldLerp + Matrix.CreateFromQuaternion(rotationWorldSlerp).Forward, Vector3.Up);

            //ActiveCamera.World = Matrix.Lerp(PreviousCamera.World, CurrentCamera.World, t
            //ActiveCamera.World = Matrix.CreateTranslation(translationWorldLerp);
            ActiveCamera.World = Matrix.CreateFromQuaternion(rotationWorldSlerp) * Matrix.CreateTranslation(translationWorldLerp);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(CameraTransition < CameraTransitionDuration)
            {
                CameraTransition += deltaTime;
            }
            else
            {
                CameraTransition = CameraTransitionDuration;
            }

            Menu.Update(gameTime);

            switch (GameStatus)
            {
                case GameState.Menu:
                    CameraTransitionDuration = 5f;
                    CameraTransition = 0f;
                    PreviousCamera = MenuCamera;
                    CurrentCamera = ShipCamera;

                    MenuShip.Update(gameTime, Environment, EffectSystem, WeaponSystem, ActiveCamera, Crosshair);

                    break;

                case GameState.Playing:
                case GameState.Dead:

                    if (Inputs.isJustPressed(Keys.G))
                    {
                        CameraTransitionDuration = 2f;
                        CameraTransition = 0f;
                        PreviousCamera = CurrentCamera;

                        Environment.ChangeWeather(Weather.Storm);

                        if (CurrentCamera.Equals(ShipCamera) || CurrentCamera.Equals(AimingCamera) || CurrentCamera.Equals(DefeatedCamera))
                        {
                            FreeCamera.Position = CurrentCamera.World.Translation + new Vector3(0, 150, 0);
                            FreeCamera.Forward = CurrentCamera.World.Forward;
                            CurrentCamera = FreeCamera;
                        }
                        else
                        {
                            CurrentCamera = ShipCamera;
                        }
                    }


                    if (Inputs.mouseRightJustPressed() && ShipsSystem.ShipPlayer.Health > 0)
                    {
                        CameraTransitionDuration = 0.5f;
                        CameraTransition = 0f;
                        PreviousCamera = CurrentCamera;

                        if(CurrentCamera.Equals(AimingCamera))
                        {
                            CurrentCamera = ShipCamera;
                            Crosshair = false;
                        }
                        else
                        {
                            AimingCamera.YawAngles = 0;
                            AimingCamera.PitchAngles = 0;

                            CurrentCamera = AimingCamera;
                            Crosshair = true;
                        }
                    }

                    ((ShipPlayer)ShipsSystem.ShipPlayer).FreeCamera = CurrentCamera.Equals(FreeCamera);

                    if (ShipsSystem.ShipPlayer.Health <= 0 && (CurrentCamera.Equals(AimingCamera) || CurrentCamera.Equals(ShipCamera)))
                    {
                        CameraTransitionDuration = 4f;
                        CameraTransition = 0f;
                        PreviousCamera = CurrentCamera;
                        CurrentCamera = DefeatedCamera;
                    }

                    ShipsSystem.Update(gameTime, Environment, EffectSystem, WeaponSystem, ActiveCamera, Crosshair);
                    EffectSystem.Update(gameTime, ActiveCamera);
                    WeaponSystem.Update(gameTime);


                    break;
            }

            Environment.Update(gameTime, ShipsSystem.Ships, ActiveCamera);
            CurrentCamera.Update(gameTime, ShipsSystem.Ships[0], this);
            PreviousCamera.Update(gameTime, ShipsSystem.Ships[0], this);

            Gizmos.UpdateViewProjection(ActiveCamera.View, ActiveCamera.Projection);
            
            Hud.Update(gameTime);

            if(Inputs.isJustPressed(Keys.C))
            {
                DrawGizmos = !DrawGizmos;
            }

            if (Inputs.isJustPressed(Keys.F1))
            {
                Hud.ShowHud = !Hud.ShowHud;
            }

            if (Menu.Restart())
            {
                GameStatus = GameState.Playing;
                ShipsSystem.ResetShips();
                ShipCamera.CurrentBackVector = Vector3.Right;
                CameraTransition = 0f;
                PreviousCamera = DefeatedCamera;
                CurrentCamera = ShipCamera;
                Crosshair = false;
                Environment.ChangeWeather(Weather.Calm);
            }
            else if(Menu.GoToMainMenu())
            {
                GameStatus = GameState.Playing;
                ShipsSystem.ResetShips();
                ShipCamera.CurrentBackVector = Vector3.Right;
                CameraTransition = 0f;
                PreviousCamera = DefeatedCamera;
                CurrentCamera = MenuCamera;
                Crosshair = false;
                Environment.ChangeWeather(Weather.Calm);

                GameStatus = GameState.Menu;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // FIRST PASS DRAW OCEAN DEPTH
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Environment.DrawPreTextures(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World, RenderState);

            // SECOND PASS MAIN SCENE RENDER
            RenderState = RenderState.Default;
            GraphicsDevice.SetRenderTarget(MainSceneRender);
            GraphicsDevice.Clear(Color.Black);

            Environment.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World, RenderState);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            switch (GameStatus)
            {
                case GameState.Menu:
                    MenuShip.Draw(ActiveCamera, RenderState, Environment);
                    break;
                case GameState.Playing:
                case GameState.Dead:
                    ShipsSystem.Draw(ActiveCamera, RenderState, Environment);
                    break;
            }
            WeaponSystem.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World);
            EffectSystem.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World, RenderState);

            // THIRD PASS HEIGHT MAP RENDER
            RenderState = RenderState.HeightMap;
            GraphicsDevice.SetRenderTarget(HeightMapRender);
            GraphicsDevice.Clear(Color.Black);

            Environment.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World, RenderState);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            switch (GameStatus)
            {
                case GameState.Menu:
                    MenuShip.Draw(ActiveCamera, RenderState, Environment);
                    break;
                case GameState.Playing:
                case GameState.Dead:
                    ShipsSystem.Draw(ActiveCamera, RenderState, Environment);
                    break;
            }
            WeaponSystem.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World);
            EffectSystem.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection, ActiveCamera.World, RenderState);

            // FINAL PASS DRAW POST PROCESS
            RenderState = RenderState.Default;
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            Environment.DrawPostProcess(gameTime, MainSceneRender, HeightMapRender, ActiveCamera);

            switch (GameStatus)
            {
                case GameState.Playing:
                case GameState.Dead:
                    Hud.Draw(gameTime, ShipsSystem.Ships, ActiveCamera, Environment, Crosshair);
                    break;
            }

            Menu.Draw(gameTime);

            //Gizmos.DrawFrustum(PreviousCamera.View * PreviousCamera.Projection);

            if(DrawGizmos)
                Gizmos.Draw();

            // base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }


    }
}