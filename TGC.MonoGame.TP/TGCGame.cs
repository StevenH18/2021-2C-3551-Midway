using System;
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

        private GraphicsDeviceManager Graphics { get; }
        private FreeCamera FreeCamera;
        private ShipCamera ShipCamera;
        private AimingCamera AimingCamera;
        private DefeatedCamera DefeatedCamera;
        private Camera ActiveCamera;
        private Camera CurrentCamera;
        private Camera PreviousCamera;
        private float CameraTransition = 1;
        private float CameraTransitionDuration = 2;

        private ShipsSystem ShipsSystem;
        private MapEnvironment Environment;
        private HudController Hud;
        private WeaponSystem WeaponSystem;
        private EffectSystem EffectSystem;

        private RenderState RenderState;
        private RenderTarget2D MainSceneRender;
        private RenderTarget2D HeightMapRender;

        private SpriteBatch SpriteBatch;
        private SpriteFont Font;
        private ShipA MenuShip;

        private Gizmos Gizmos;

        public const int ST_MENU = 0;
        public const int ST_LEVEL_1 = 1;
        public int MenuStatus = ST_MENU;


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

            FreeCamera.Position = new Vector3(16000, 50, 0);
            FreeCamera.Forward = new Vector3(-0.8f, 0.2f, 0);

            ShipCamera = new ShipCamera(GraphicsDevice, this.Window);
            AimingCamera = new AimingCamera(GraphicsDevice, this.Window);
            DefeatedCamera = new DefeatedCamera(GraphicsDevice, this.Window);
            ActiveCamera = new Camera();
            CurrentCamera = ShipCamera;
            PreviousCamera = ShipCamera;

            ActiveCamera.World = FreeCamera.World;
            ActiveCamera.Projection = FreeCamera.Projection;
            ActiveCamera.View = FreeCamera.View;

            Gizmos = new Gizmos();
            ShipsSystem = new ShipsSystem(GraphicsDevice, Content, Gizmos);
            Environment = new MapEnvironment(GraphicsDevice, Content, Gizmos);
            Hud = new HudController(GraphicsDevice, Content);
            EffectSystem = new EffectSystem(GraphicsDevice, Content);
            WeaponSystem = new WeaponSystem(GraphicsDevice, Content, EffectSystem, Environment, ShipsSystem, Gizmos);

            RenderState = RenderState.Default;
            MainSceneRender = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);
            HeightMapRender = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);

            // Menu Scene
            MenuShip = new ShipPlayer(Content, GraphicsDevice, Gizmos);
            MenuShip.Position.X = 15600f;
            MenuShip.Position.Z = 200f;
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

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Font = Content.Load<SpriteFont>("Fonts/Basic");

            base.LoadContent();
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

            float t = CameraTransition / CameraTransitionDuration;
            t = t * t * (3f - 2f * t);

            ActiveCamera.Projection = Matrix.Lerp(PreviousCamera.Projection, CurrentCamera.Projection, t);
            ActiveCamera.View = Matrix.Lerp(PreviousCamera.View, CurrentCamera.View, t);
            ActiveCamera.World = Matrix.Lerp(PreviousCamera.World, CurrentCamera.World, t);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(CameraTransition < CameraTransitionDuration)
            {
                CameraTransition += deltaTime;
            }
            else
            {
                CameraTransition = CameraTransitionDuration;
            }

            switch (MenuStatus)
            {
                case ST_MENU:

                    CameraTransition = 0f;
                    PreviousCamera = FreeCamera;
                    CurrentCamera = ShipCamera;

                    MenuShip.Update(gameTime, Environment, EffectSystem, WeaponSystem, ActiveCamera);

                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                        MenuStatus = ST_LEVEL_1;

                    break;

                case ST_LEVEL_1:

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

                        AimingCamera.YawAngles = 0;
                        AimingCamera.PitchAngles = 0;

                        CurrentCamera = AimingCamera;
                    }
                    if(Inputs.mouseRightJustReleased() && ShipsSystem.ShipPlayer.Health > 0)
                    {
                        CameraTransitionDuration = 0.5f;

                        CameraTransition = 0f;
                        PreviousCamera = CurrentCamera;

                        CurrentCamera = ShipCamera;
                    }

                    if(ShipsSystem.ShipPlayer.Health <= 0 && (CurrentCamera.Equals(AimingCamera) || CurrentCamera.Equals(ShipCamera)))
                    {
                        CameraTransitionDuration = 4f;
                        CameraTransition = 0f;
                        PreviousCamera = CurrentCamera;
                        CurrentCamera = DefeatedCamera;
                    }

                    ShipsSystem.Update(gameTime, Environment, EffectSystem, WeaponSystem, ActiveCamera);
                    CurrentCamera.Update(gameTime, ShipsSystem.Ships[0]);
                    PreviousCamera.Update(gameTime, ShipsSystem.Ships[0]);
                    EffectSystem.Update(gameTime, ActiveCamera);
                    Environment.Update(gameTime, ShipsSystem.Ships, ActiveCamera);
                    WeaponSystem.Update(gameTime);

                    Hud.Update(gameTime);

                    IsMouseVisible = false;

                    break;
            }

            Gizmos.UpdateViewProjection(ActiveCamera.View, ActiveCamera.Projection);

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
            switch (MenuStatus)
            {
                case ST_MENU:
                    MenuShip.Draw(ActiveCamera, RenderState, Environment);
                    break;
                case ST_LEVEL_1:
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
            switch (MenuStatus)
            {
                case ST_MENU:
                    MenuShip.Draw(ActiveCamera, RenderState, Environment);
                    break;
                case ST_LEVEL_1:
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

            switch (MenuStatus)
            {
                case ST_MENU:
                    //PARA AGREGAR OTRAS TEXTURAS
                    SpriteBatch.Begin();
                    SpriteBatch.DrawString(Font, "MIDWAY TP TGC ", new Vector2(100, 100), Color.White);
                    SpriteBatch.DrawString(Font, "Presione ESPACIO para comenzar", new Vector2(100, 200), Color.White);
                    SpriteBatch.DrawString(Font, "W S A D para controlar", new Vector2(100, 300), Color.White);
                    SpriteBatch.End();

                    break;
                case ST_LEVEL_1:
                    Hud.Draw(gameTime, ShipsSystem.Ships, ActiveCamera.World, Environment);
                    break;
            }

            //Gizmos.DrawFrustum(PreviousCamera.View * PreviousCamera.Projection);

            //Gizmos.Draw();

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