﻿using Microsoft.Xna.Framework;
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
            Effect.Parameters["ParticlesTotal"]?.SetValue(Environment.RainParticles);
            Effect.Parameters["HeightStart"]?.SetValue(Environment.RainHeightStart);
            Effect.Parameters["HeightEnd"]?.SetValue(Environment.RainHeightEnd);
            Effect.Parameters["Speed"]?.SetValue(Environment.RainSpeed);
            Effect.Parameters["Progress"]?.SetValue(Environment.RainProgress);

            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffers(new VertexBufferBinding(VertexBuffer), new VertexBufferBinding(InstanceBuffer, 0, 1));

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = 2;

                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles, Environment.RainParticles);
            }
        }

        private void UpdateInstances()
        {
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
                offset.W = (float)i;

                Instances[i].PositionOffset = offset;
            }
            UpdateInstances();
        }

        private void InitializeVertexDeclaration()
        {
            InstanceVertexDeclaration = new VertexDeclaration(
                // INDEX POSITION ???? NO LO PUEDO SACAR O SI NO SE ROMPE TODO
                // ADEMAS NO LO PUEDO LEER DESDE EL SHADER
                // ASI QUE EL INDICE DE LA PARTICULA LO MANDO POR EL offset.W del Position Offset
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                // Position Offset
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Position, 2)
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
                for (float x = 0; x < 2; ++x)
                {
                    var position = new Vector3(x / 2 * Environment.RainParticleWidth - Environment.RainParticleWidth / 2, y / 2 * Environment.RainParticleHeight - Environment.RainParticleHeight / 2, 0f);
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
