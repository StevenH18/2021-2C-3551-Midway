using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Environment
{
    /// <summary>
    ///     Handles all of the aspects of working with a SkyBox.
    /// </summary>
    public class SkyBox
    { 
        private float Size { get; }
        private Effect Effect { get; }
        public Model Model { get; set; }
        private TextureCube RainTexture;
        private TextureCube StormTexture;
        private MapEnvironment Environment;
        /// <summary>
        ///     Creates a new SkyBox
        /// </summary>
        /// <param name="model">The geometry to use for SkyBox.</param>
        /// <param name="texture">The SkyBox texture to use.</param>
        public SkyBox(GraphicsDevice Graphics, ContentManager Content, MapEnvironment environment)
        {
            RainTexture = Content.Load<TextureCube>(TGCGame.ContentFolderTextures + "/SkyBoxes/StormySky");
            StormTexture = Content.Load<TextureCube>(TGCGame.ContentFolderTextures + "/SkyBoxes/StormySky");
            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "SkyBox/cube");
            Environment = environment;
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "SkyBox"); ;
            Size = 50000;
        }

        /// <summary>
        ///     Does the actual drawing of the SkyBox with our SkyBox effect.
        ///     There is no world matrix, because we're assuming the SkyBox won't
        ///     be moved around.  The size of the SkyBox can be changed with the size
        ///     variable.
        /// </summary>
        /// <param name="view">The view matrix for the effect</param>
        /// <param name="projection">The projection matrix for the effect</param>
        /// <param name="cameraPosition">The position of the camera</param>
        public void Draw(Matrix view, Matrix projection, Matrix world)
        {
            // Go through each pass in the effect, but we know there is only one...
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Draw all of the components of the mesh, but we know the cube really
                // only has one mesh
                foreach (var mesh in Model.Meshes)
                {
                    // Assign the appropriate values to each of the parameters
                    foreach (var part in mesh.MeshParts)
                    {
                        part.Effect = Effect;
                        part.Effect.Parameters["World"].SetValue(
                            Matrix.CreateScale(Size) * world);
                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["CameraPosition"].SetValue(world.Translation);
                        part.Effect.Parameters["SkyBoxTextureRain"]?.SetValue(RainTexture);
                        part.Effect.Parameters["SkyBoxTextureStorm"]?.SetValue(StormTexture);
                        part.Effect.Parameters["SkyProgress"]?.SetValue(Environment.RainProgress);
                    }

                    // Draw the mesh with the SkyBox effect
                    mesh.Draw();
                }
            }
        }
    }
}