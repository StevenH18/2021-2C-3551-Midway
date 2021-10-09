using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Controller;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        private GraphicsDeviceManager Graphics { get; }
        private FollowCamera FollowCamera { get; set; }
        private FreeCamera FreeCamera { get; set; }
        private Camera Camera { get; set; }
        private ShipCamera ShipCamera { get; set; }

        private int naves = 20;

        private Ship[] ships;

        private MapEnvironment Environment;


        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = false;
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

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Creo una camara para seguir a nuestro auto
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            FreeCamera = new FreeCamera(GraphicsDevice, this.Window);

            ShipCamera = new ShipCamera(GraphicsDevice.Viewport.AspectRatio);

            Camera = new Camera(60, GraphicsDevice,0.1f,1000f);

            ships = new Ship[naves];

            Environment = new MapEnvironment(GraphicsDevice, Content);

            ships[0] = new ShipA(Content, Environment.Ocean, Color.Yellow);
            ships[0].Position.Z = 6000; //en el medio del oceano
            ships[0].Position.X = 6000;
            for (int i = 1; i < naves; i++)
            {
                var repeticion = 5;
                var variacion = 400;
                var offset = 1000;
                var separation = 500;
                var rand = new Random();

                if( i%2 == 0)
                {
                    ships[i] = new ShipA(Content, Environment.Ocean, Color.White);
                    ships[i].Position.Z = ((i % repeticion) * separation) + rand.Next(-variacion, variacion) + offset;
                    ships[i].Position.X = ((int)Math.Floor(i / (float)repeticion) * separation * 2) + rand.Next(-variacion * 2, variacion * 2) + offset;

                }else
                {
                    ships[i] = new ShipB(Content, Environment.Ocean, Color.Blue);
                    ships[i].Position.Z = ((i % repeticion) * separation) + rand.Next(-variacion, variacion) + offset;
                    ships[i].Position.X = ((int)Math.Floor(i / (float)repeticion) * separation * 2) + rand.Next(-variacion * 2, variacion * 2) + offset;
                }

            }


            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            for (int i = 0; i < naves; i++)
            {
                ships[i].Load();
            }

            Environment.Load();

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

            Player p = new Player();

            ships[0].Update(gameTime, p.GetControlls());

            for (int i = 1; i < naves; i++)
            {
                ships[i].Update(gameTime, new Controll());
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter)){
                FreeCamera = new FreeCamera(GraphicsDevice, this.Window);
            }
            ShipCamera.Update(gameTime, ships[0].Rotation, ships[0].World, ships[0].speed);
            Camera.Update(gameTime);
            FreeCamera.Update(gameTime);

            Environment.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            for (int i = 0; i < naves; i++)
            {
                ships[i].Draw(ShipCamera.View, ShipCamera.Projection);
            }

            Environment.Draw(gameTime, ShipCamera.View, ShipCamera.Projection, Matrix.CreateTranslation(ShipCamera.Position));

            base.Draw(gameTime);
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