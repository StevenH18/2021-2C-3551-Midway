using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP
{
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

        private RainParticle[] Instances;

        struct RainParticle
        {
            public float Index;
            public Vector4 PositionOffset;
            public Matrix BillboardMatrix;
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

            // Hacer que las particulas formen un cubo donde la camara esta en el centro

            // NO se necesita el World porque por cada particula le defino su correspondiente Matriz
            Effect.Parameters["View"]?.SetValue(view);
            Effect.Parameters["Projection"]?.SetValue(proj);
            Effect.Parameters["Time"]?.SetValue(time);
            Effect.Parameters["ParticlesTotal"]?.SetValue(Environment.RainParticles);
            Effect.Parameters["HeightStart"]?.SetValue(Environment.RainHeightStart);
            Effect.Parameters["HeightEnd"]?.SetValue(Environment.RainHeightEnd);
            Effect.Parameters["Speed"]?.SetValue(Environment.RainSpeed);
            Effect.Parameters["Progress"]?.SetValue(Environment.RainProgress);

            UpdateInstances(cameraWorld);

            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffers(new VertexBufferBinding(VertexBuffer), new VertexBufferBinding(InstanceBuffer, 0, 1));

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = 2;

                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles, Environment.RainParticles);
            }
        }

        private void UpdateInstances(Matrix cameraWorld)
        {
            for (var i = 0; i < Instances.Length; i++)
            {
                Vector3 cameraPosition = cameraWorld.Translation;
                Vector3 instancePosition = new Vector3(Instances[i].PositionOffset.X, 0f, Instances[i].PositionOffset.Z);
                Vector3 position = Vector3.Zero;

                position.X = instancePosition.X + MathF.Floor((cameraPosition.X - instancePosition.X + Environment.RainParticleSeparation / 2) / Environment.RainParticleSeparation) * Environment.RainParticleSeparation;
                position.Z = instancePosition.Z + MathF.Floor((cameraPosition.Z - instancePosition.Z + Environment.RainParticleSeparation / 2) / Environment.RainParticleSeparation) * Environment.RainParticleSeparation;

                Matrix billboard = Matrix.CreateConstrainedBillboard(position, cameraPosition, Vector3.Up, cameraWorld.Forward, Vector3.Forward);
                Instances[i].BillboardMatrix = billboard;
            }

            // Set the instace data to the instanceBuffer.
            InstanceBuffer = new VertexBuffer(GraphicsDevice, InstanceVertexDeclaration, Environment.RainParticles, BufferUsage.None);
            InstanceBuffer.SetData(Instances);
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

                Instances[i].Index = (float)i;
                Instances[i].PositionOffset = offset;
            }
        }

        private void InitializeVertexDeclaration()
        {
            InstanceVertexDeclaration = new VertexDeclaration(
                // Index
                new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.BlendIndices, 0),
                // Position Offset
                new VertexElement(sizeof(float), VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                // Matrix
                new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
                new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
                new VertexElement(sizeof(float) * 13, VertexElementFormat.Vector4, VertexElementUsage.Position, 4),
                new VertexElement(sizeof(float) * 17, VertexElementFormat.Vector4, VertexElementUsage.Position, 5)
            );
        }

        private void InitializeGeometry()
        {
            // Creo vertices en base al GridWidth y GridHeight
            VertexPosition[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(GraphicsDevice, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            uint[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Crea una grilla de vertices que representa un plano para enviar al VertexBuffer
        /// </summary>
        private VertexPosition[] CalculateVertices()
        {
            var vertices = new VertexPosition[4];

            int vertIndex = 0;
            for (float y = 0; y < 2; ++y)
            {
                var rand = new Random();
                var rotated = rand.Next(0, 2);

                for (float x = 0; x < 2; ++x)
                {
                    var position = new Vector3(x / 2 * Environment.RainParticleWidth, y / 2 * Environment.RainParticleHeight, 0f);
                    vertices[vertIndex++] = new VertexPosition(position);
                }
            }

            return vertices;
        }
        /// <summary>
        /// Crea los indices de los quads (2 triangulos) para enviarlos al IndexBuffer
        /// </summary>
        private uint[] CalculateIndices()
        {
            var indices = new uint[(1) * (1) * 6];

            int indicesIndex = 0;
            for (int y = 0; y < 2 - 1; ++y)
            {
                for (int x = 0; x < 2 - 1; ++x)
                {
                    int start = y * 2 + x;
                    indices[indicesIndex++] = (uint)start;
                    indices[indicesIndex++] = (uint)(start + 1);
                    indices[indicesIndex++] = (uint)(start + 2);
                    indices[indicesIndex++] = (uint)(start + 1);
                    indices[indicesIndex++] = (uint)(start + 1 + 2);
                    indices[indicesIndex++] = (uint)(start + 2);
                }
            }

            return indices;
        }
    }
}
