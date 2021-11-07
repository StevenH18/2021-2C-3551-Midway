using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Super optimizado con instanciamiento, se pueden poner 500000 particulas de lluvia
    ///     y anda a 60fps
    /// </summary>
    public class RainSystem
    {
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;
        protected MapEnvironment Environment;
        protected Effect Effect;

        protected VertexDeclaration InstanceVertexDeclaration;

        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;
        protected VertexBuffer InstanceBuffer;
        private VertexBufferBinding[] Bindings;
        private RainParticle[] Instances;

        struct RainParticle
        {
            public Vector4 PositionOffset;
        }

        public RainSystem(GraphicsDevice graphics, ContentManager content, MapEnvironment environment)
        {
            GraphicsDevice = graphics;
            Content = content;
            Environment = environment;
        }
        public void Load()
        {
            InitializeVertexDeclaration();
            InitializeGeometry();
            InitializeInstances();

            // Load Shader
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "RainShader");
        }
        public void Draw(Matrix view, Matrix proj, Matrix cameraWorld, GameTime gameTime)
        {
            var time = (float)gameTime.TotalGameTime.TotalSeconds;

            Effect.Parameters["World"]?.SetValue(Matrix.Identity);
            Effect.Parameters["View"]?.SetValue(view);
            Effect.Parameters["Projection"]?.SetValue(proj);
            Effect.Parameters["Time"]?.SetValue(time);
            Effect.Parameters["CameraPosition"]?.SetValue(cameraWorld.Translation);
            Effect.Parameters["ParticleSeparation"]?.SetValue(Environment.RainParticleSeparation);
            Effect.Parameters["ParticleHeight"]?.SetValue(Environment.RainParticleHeight);
            Effect.Parameters["ParticleWidth"]?.SetValue(Environment.RainParticleWidth);
            Effect.Parameters["ParticlesTotal"]?.SetValue(Environment.RainParticles);
            Effect.Parameters["HeightStart"]?.SetValue(Environment.RainHeightStart);
            Effect.Parameters["HeightEnd"]?.SetValue(Environment.RainHeightEnd);
            Effect.Parameters["Speed"]?.SetValue(Environment.RainSpeed);
            Effect.Parameters["Progress"]?.SetValue(Environment.RainProgress);

            //UpdateInstances();

            GraphicsDevice.SetVertexBuffers(Bindings);
            GraphicsDevice.Indices = IndexBuffer;

            Effect.CurrentTechnique.Passes[0].Apply();

            var triangles = 2;

            GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles, Environment.RainParticles);
        }

        private void UpdateInstances()
        {
            InstanceBuffer = new VertexBuffer(GraphicsDevice, InstanceVertexDeclaration, Environment.RainParticles, BufferUsage.None);
            InstanceBuffer.SetData(Instances);

            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(InstanceBuffer, 0, 1);
            Bindings[1] = new VertexBufferBinding(VertexBuffer);
        }

        private void InitializeInstances()
        {
            Instances = new RainParticle[Environment.RainParticles];

            for (var i = 0; i < Environment.RainParticles; i++)
            {
                Random random = new Random();

                Vector4 offset = Vector4.Zero;
                offset.X = (float)random.NextDouble() * Environment.RainParticleSeparation - Environment.RainParticleSeparation / 2;
                offset.Y = (float)random.NextDouble() * Environment.RainParticleVerticalSeparation - Environment.RainParticleVerticalSeparation / 2;
                offset.Z = (float)random.NextDouble() * Environment.RainParticleSeparation - Environment.RainParticleSeparation / 2;
                offset.W = (float)i;

                Instances[i].PositionOffset = offset;
            }
            UpdateInstances();
        }

        private void InitializeVertexDeclaration()
        {
            InstanceVertexDeclaration = new VertexDeclaration(
                new VertexElement[]
                {
                    // Position Offset
                    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0)
                }
            );
        }

        private void InitializeGeometry()
        {
            // Creo vertices en base al GridWidth y GridHeight
            VertexPosition[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(GraphicsDevice, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            ushort[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Crea una grilla de vertices que representa un plano para enviar al VertexBuffer
        /// </summary>
        private VertexPosition[] CalculateVertices()
        {
            var vertices = new VertexPosition[] {
                new VertexPosition(new Vector3(-Environment.RainParticleWidth / 2,  Environment.RainParticleHeight / 2, 0f)),
                new VertexPosition(new Vector3( Environment.RainParticleWidth / 2,  Environment.RainParticleHeight / 2, 0f)),
                new VertexPosition(new Vector3(-Environment.RainParticleWidth / 2, -Environment.RainParticleHeight / 2, 0f)),
                new VertexPosition(new Vector3( Environment.RainParticleWidth / 2, -Environment.RainParticleHeight / 2, 0f))
            };

            return vertices;
        }
        /// <summary>
        /// Crea los indices de los quads (2 triangulos) para enviarlos al IndexBuffer
        /// </summary>
        private ushort[] CalculateIndices()
        {
            var indices = new ushort[6]{
                0, 3, 2,
                3, 0, 1
            };

            return indices;
        }
    }
}
