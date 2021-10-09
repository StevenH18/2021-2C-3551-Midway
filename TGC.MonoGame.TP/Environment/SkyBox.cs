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

        /// <summary>
        ///     Creates a new SkyBox
        /// </summary>
        /// <param name="model">The geometry to use for SkyBox.</param>
        /// <param name="texture">The SkyBox texture to use.</param>
        public SkyBox(GraphicsDevice Graphics, ContentManager Content)
        {
            var skyBox = Content.Load<Model>(TGCGame.ContentFolder3D + "SkyBox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(TGCGame.ContentFolderTextures + "/SkyBoxes/StormSky");
            var skyBoxEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "SkyBox");

            Model = skyBox;
            Texture = skyBoxTexture;
            Effect = skyBoxEffect;
            Size = 10000;
        }

        /// <summary>
        ///     The size of the cube, used so that we can resize the box
        ///     for different sized environments.
        /// </summary>
        private float Size { get; }

        /// <summary>
        ///     The effect file that the SkyBox will use to render
        /// </summary>
        private Effect Effect { get; }

        /// <summary>
        ///     The actual SkyBox texture
        /// </summary>
        private TextureCube Texture { get; }

        /// <summary>
        ///     The SkyBox model, which will just be a cube
        /// </summary>
        public Model Model { get; set; }

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
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(Texture);
                        part.Effect.Parameters["CameraPosition"].SetValue(world.Translation);
                    }

                    // Draw the mesh with the SkyBox effect
                    mesh.Draw();
                }
            }
        }
    }
}