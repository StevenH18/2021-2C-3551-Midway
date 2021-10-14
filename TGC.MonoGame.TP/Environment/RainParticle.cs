﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace TGC.MonoGame.TP
{
    public class RainParticle
    {
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;
        protected Effect Effect;
        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;

        public float Width = 0;
        public float Height = 0;
        public Vector3 Offset = Vector3.Zero;
        public float TimeOffset = 0;

        private Vector3 Position = Vector3.Zero;
        private Matrix Billboard = Matrix.Identity;

        public RainParticle(GraphicsDevice graphics, ContentManager content, float width, float height, Vector3 offset, float timeOffset)
        {
            this.GraphicsDevice = graphics;
            this.Content = content;
            this.Width = width;
            this.Height = height;
            this.Offset = offset;
            this.TimeOffset = timeOffset;
        }
        public void Load()
        {
            // Generate Rain Mesh
            GenerateParticleMesh();

            // Load Shader
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "RainShader");
        }
        public void Draw(Matrix view, Matrix proj, Matrix cameraWorld, float particleSeparation, float heightStart, float heightEnd, float speed, bool skip, GameTime gameTime)
        {
            if (skip)
                return;

            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            Vector3 cameraPosition = cameraWorld.Translation;

            // Hacer que las particulas formen un cubo donde la camara esta en el centro
            Position.X = Offset.X + MathF.Floor((cameraPosition.X - Offset.X + particleSeparation / 2) / particleSeparation) * particleSeparation;
            Position.Z = Offset.Z + MathF.Floor((cameraPosition.Z - Offset.Z + particleSeparation / 2) / particleSeparation) * particleSeparation;

            Billboard = Matrix.CreateConstrainedBillboard(Position, cameraPosition, Vector3.Up, cameraWorld.Forward, Vector3.Forward);

            Effect.Parameters["World"]?.SetValue(Billboard);
            Effect.Parameters["View"]?.SetValue(view);
            Effect.Parameters["Projection"]?.SetValue(proj);
            Effect.Parameters["Time"]?.SetValue(time + TimeOffset);
            Effect.Parameters["HeightStart"]?.SetValue(heightStart);
            Effect.Parameters["HeightEnd"]?.SetValue(heightEnd);
            Effect.Parameters["Speed"]?.SetValue(speed);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = 2;

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles);
            }
        }

        public void GenerateParticleMesh()
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
                
                for (float x = 0; x < 2; ++x)
                {
                    var position = new Vector3(x / 2 * Width, y / 2 * Height, 0f);
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
