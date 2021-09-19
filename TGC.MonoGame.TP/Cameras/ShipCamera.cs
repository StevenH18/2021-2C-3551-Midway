using BepuUtilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.TP.Cameras
{
    /// <summary>
    /// Una camara que sigue objetos
    /// </summary>
    public class ShipCamera
    {
        private const float AxisDistanceToTarget = 400f;

        private const float AngleFollowSpeed = 0.030f;

        private const float AngleThreshold = 0f;

        public Matrix Projection { get; private set; }

        public Matrix View { get; private set; }

        private Vector3 CurrentBackVector { get; set; } = Vector3.Backward;

        private float BackVectorInterpolator { get; set; } = 0f;

        private Vector3 PastBackVector { get; set; } = Vector3.Backward;

        public Vector3 Position { get; set; }

        /// <summary>
        /// Crea una FollowCamera que sigue a una matriz de mundo
        /// </summary>
        /// <param name="aspectRatio"></param>
        public ShipCamera(float aspectRatio)
        {
            // Orthographic camera
            // Projection = Matrix.CreateOrthographic(screenWidth, screenHeight, 0.01f, 10000f);

            // Perspective camera
            // Uso 60° como FOV, aspect ratio, pongo las distancias a near plane y far plane en 0.1 y 100000 (mucho) respectivamente
            Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 3f, aspectRatio, 0.1f, 100000f);
        }

        /// <summary>
        /// Actualiza la Camara usando una matriz de mundo actualizada para seguirla
        /// </summary>
        /// <param name="gameTime">The Game Time to calculate framerate-independent movement</param>
        /// <param name="followedWorld">The World matrix to follow</param>
       
        public void Update(GameTime gameTime, Matrix Rotation,Matrix followedWorld,float speed) //agregue speed para poder hacer un efecto de que la camara se mueve mas lento mientras el barco arranca
        {
            // Obtengo el tiempo
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            // Obtengo la posicion de la matriz de mundo que estoy siguiendo
            var followedPosition = followedWorld.Translation;

            // Obtengo el vector atras de la matriz de mundo que estoy siguiendo

            var followedRigth = Vector3.Transform(Vector3.Backward, Rotation);

            // Si el producto escalar entre el vector Derecha anterior
            // y el actual es mas grande que un limite,
            // muevo el Interpolator (desde 0 a 1) mas cerca de 1
            if (speed !=  0)
            {
                // Incremento el Interpolator
                BackVectorInterpolator += elapsedTime * AngleFollowSpeed;

                // No permito que Interpolator pase de 1
                BackVectorInterpolator = MathF.Min(BackVectorInterpolator, 1f);

                // Calculo el vector Derecha a partir de la interpolacion
                // Esto mueve el vector Derecha para igualar al vector Derecha que sigo
                // En este caso uso la curva x^2 para hacerlo mas suave
                // Interpolator se convertira en 1 eventualmente
                CurrentBackVector = Vector3.Lerp(CurrentBackVector, followedRigth, BackVectorInterpolator * BackVectorInterpolator);
            }
            else
                // Si el angulo no pasa de cierto limite, lo pongo de nuevo en cero
                BackVectorInterpolator = 0f;
        
            // Guardo el vector Derecha para usar en la siguiente iteracion
            PastBackVector = followedRigth;

            // Calculo la posicion del a camara
            // tomo la posicion que estoy siguiendo, agrego un offset en los ejes Y y Derecha
            var offsetedPosition = followedPosition
                + CurrentBackVector * AxisDistanceToTarget
                + Vector3.Up * AxisDistanceToTarget * 0.2f;

            // Calculo el vector Arriba actualizado
            // Nota: No se puede usar el vector Arriba por defecto (0, 1, 0)
            // Como no es correcto, se calcula con este truco de producto vectorial

            // Calcular el vector Adelante haciendo la resta entre el destino y el origen
            // y luego normalizandolo (Esta operacion es cara!)
            // (La siguiente operacion necesita vectores normalizados)
            var forward = (followedPosition - offsetedPosition);
            forward.Normalize();

            // Obtengo el vector Derecha asumiendo que la camara tiene el vector Arriba apuntando hacia arriba
            // y no esta rotada en el eje X (Roll)
            var right = Vector3.Cross(forward, Vector3.Up);

            // Una vez que tengo la correcta direccion Derecha, obtengo la correcta direccion Arriba usando
            // otro producto vectorial
            var cameraCorrectUp = Vector3.Cross(right, forward);

            // Calculo la matriz de Vista de la camara usando la Posicion, La Posicion a donde esta mirando,
            // y su vector Arriba

            Position = offsetedPosition;
            View = Matrix.CreateLookAt(offsetedPosition, followedPosition, cameraCorrectUp);
        }
    }
}
