using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        
        private FollowCamera FollowCamera { get; set; }
        private FreeCamera FreeCamera { get; set; }
        private Camera Camera { get; set; }

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = false;
        }

        private GraphicsDeviceManager Graphics { get; }

        private int naves = 10;

        private ShipA[] shipsA;
        private ShipB[] shipsB;
        private Ocean Ocean;

        // Estoy guardando la posiciones originales de los barcos aca, hay que hacer algo con respecto a esto
        private Vector3[] positions;

        private float waveAngle = - MathF.PI * 0.5f;

        private float rotation = 0f;

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
            // Creo una camara para seguir a nuestro auto

            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            FreeCamera = new FreeCamera(GraphicsDevice, this.Window);

            Camera = new Camera(60, GraphicsDevice,0.1f,1000f);

            shipsA = new ShipA[naves];
            shipsB = new ShipB[naves];
            positions = new Vector3[naves];
            Ocean = new Ocean(GraphicsDevice, Content);

            for (int i = 0; i < naves*2; i++)
            {
                var repeticion = 5;
                var variacion = 400;
                var offset = 1000;
                var separation = 500;
                var rand = new Random();

                if(i < naves)
                {
                    shipsA[i] = new ShipA(Content);
                    shipsA[i].Position.Z = ((i % repeticion) * separation) + rand.Next(-variacion, variacion) + offset;
                    shipsA[i].Position.X = ((int)Math.Floor(i / (float)repeticion) * separation * 2) + rand.Next(-variacion * 2, variacion * 2) + offset;
                    positions[i] = shipsA[i].Position;
                }
                else
                {
                    shipsB[i - naves] = new ShipB(Content);
                    shipsB[i - naves].Position.Z = ((i % repeticion) * separation) + rand.Next(-variacion, variacion) + offset;
                    shipsB[i - naves].Position.X = ((int)Math.Floor(i / (float)repeticion) * separation * 2) + rand.Next(-variacion * 2, variacion * 2) + offset;
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
                shipsA[i].Load();
            }
            for (int i = 0; i < naves; i++)
            {
                shipsB[i].Load();
            }
            Ocean.Load();

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

            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Temporal para probar el movimiento de los barcos
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad4)) rotation -= time * 2f;
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad6)) rotation += time * 2f;

            // I J para controlar la inclinacion de las olas
            if (Keyboard.GetState().IsKeyDown(Keys.I) && Ocean.Steepness <= 1f) Ocean.Steepness += time;
            if (Keyboard.GetState().IsKeyDown(Keys.J) && Ocean.Steepness >= 0f) Ocean.Steepness -= time;

            // O K para controlar la longitud de las olas
            if (Keyboard.GetState().IsKeyDown(Keys.O)) Ocean.WaveLength += time * 100f;
            if (Keyboard.GetState().IsKeyDown(Keys.K)) Ocean.WaveLength -= time * 100f;

            // P L para controlar la direccion de las olas
            if (Keyboard.GetState().IsKeyDown(Keys.P)) waveAngle += time;
            if (Keyboard.GetState().IsKeyDown(Keys.L)) waveAngle -= time;

            Ocean.Direction = new Vector2(MathF.Sin(waveAngle), MathF.Cos(waveAngle));

            for (int i = 0; i < naves; i++)
            {
                // Temporal para rotar los barcos
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad8)) positions[i] += Vector3.Transform(Vector3.Forward, shipsA[i].Rotation) * time * 100f;
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad2)) positions[i] -= Vector3.Transform(Vector3.Forward, shipsA[i].Rotation) * time * 100f;

                (Vector3, Vector3) result = Ocean.WaveNormalPosition(positions[i], gameTime);
                Vector3 normal = result.Item1;
                Vector3 position = result.Item2;

                // MAGIA MAGIA MAGIA NEGRA !!!!!!!!!!!!!!!!!!!
                shipsA[i].Rotation = Matrix.CreateFromYawPitchRoll(0f, normal.Z, -normal.X) * Matrix.CreateFromAxisAngle(normal, rotation);

                positions[i].Y = position.Y;
                shipsA[i].Position = position;
                shipsA[i].Update(gameTime);
            }
            for (int i = 0; i < naves; i++)
            {
                shipsB[i].Update(gameTime);
            }
            

            if (Keyboard.GetState().IsKeyDown(Keys.Enter)){
                FreeCamera = new FreeCamera(GraphicsDevice, this.Window);
            }
            Camera.Update(gameTime);
            FreeCamera.Update(gameTime);
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
                shipsA[i].Draw(FreeCamera.View,FreeCamera.Projection,Color.White);
            }
             
            for (int i = 0; i < naves; i++)
            {
                shipsB[i].Draw(FreeCamera.View, FreeCamera.Projection, Color.Blue);
            }

            Ocean.Draw(FreeCamera.View, FreeCamera.Projection, gameTime);

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