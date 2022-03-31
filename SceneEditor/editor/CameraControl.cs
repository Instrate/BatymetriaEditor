using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LearnOpenTK.Common;
using OpenTK.Mathematics;

using System.Windows.Forms;

namespace SceneEditor.editor
{
    public class CameraControl
    {
        public Camera cam;
        private Vector2i _camSize;
        public float cameraSpeed = 4f;
        public float sensitivity = 0.4f;

        public bool grabedMouse;
        bool _firstMove;

        Vector2 _lastPos;

        //public int viewStyle = 0;

        public CameraControl(Vector2i size, bool isOrtogonal = false)
        {
            _camSize = size;
            cam = new Camera(Vector3.UnitZ * 4, size.X / (float)size.Y, isOrtogonal);

            cam.Yaw = 0;

            _firstMove = true;
            grabedMouse = false;
        }


        ~CameraControl()
        {

        }

        public void OnRender(int handle)
        {
           //Matrix4 model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(0));

           //shader.SetMatrix4("model", model);
           //shader.SetMatrix4("view", camera.GetViewMatrix());
           //shader.SetMatrix4("projection", camera.GetProjectionMatrix());

           //cameraView();
        }

        public void OnMouseMove(System.Windows.Input.MouseEventArgs mouse, Size size, Point pointGL, Point pointScreen)
        {

            if (_firstMove)
            {
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point()
                {
                    X = (int)(pointScreen.X - (pointGL.X - size.Width / 2.0)),
                    Y = (int)(pointScreen.Y - (pointGL.Y - size.Height / 2.0))
                };
                _firstMove = false;
                if (!grabedMouse)
                {
                    _lastPos = new Vector2(0, 0);
                }               
            }
            else 
            {
                // ! fix later
                if (mouse.LeftButton == MouseButtonState.Pressed)
                {
                    // Calculate the offset of the mouse position
                    float deltaX = (float)(pointGL.X * Screen.PrimaryScreen.Bounds.Width / size.Width - _lastPos.X);

                    float deltaY = (float)(pointGL.Y * Screen.PrimaryScreen.Bounds.Height / size.Height - _lastPos.Y);

                    Vector2 newPos = new Vector2
                    {
                        X = _lastPos.X + deltaX,
                        Y = _lastPos.Y + deltaY
                    };

                    //// Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                    cam.Yaw -= deltaX * sensitivity;
                    cam.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
                    _lastPos = newPos;
                }
                else
                {
                    //_lastPos = new Vector2((float)size.Width, (float)size.Height);
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point()
                    {
                        X = (int)(pointScreen.X - (pointGL.X - size.Width / 2.0)),
                        Y = (int)(pointScreen.Y - (pointGL.Y - size.Height / 2.0))
                    };
                }
                //Console.WriteLine("lastPos");
                //Console.WriteLine(_lastPos);
            }
        }

        public void OnMouseLeave()
        {
            grabedMouse = false;
            _firstMove = true;
            _lastPos = Vector2.Zero;
            //Cursor.Show();
        }

        public void OnKeyDown(System.Windows.Input.KeyEventArgs e, float timeDelta)
        {
            var key = e.Key;

            var offset = cameraSpeed * timeDelta;

            switch (key)
            {
                case Key.W: {
                        cam.Position += cam.Front * offset;
                    };break;
                case Key.S:
                    {
                        cam.Position -= cam.Front * offset;
                    }; break;
                case Key.A:
                    {
                        cam.Position -= cam.Right * offset;
                    }; break;
                case Key.D:
                    {
                        cam.Position += cam.Right * offset;
                    }; break;
                case Key.Space:
                    {
                        cam.Position += new Vector3() { Z = offset };
                    }; break;
                case Key.LeftCtrl:
                    {
                        cam.Position += new Vector3() { Z = -offset };
                    }; break;
            }

        }

        private void cameraView()
        {
            //switch (viewStyle)
            //{
            //    case 1:
            //        {
            //            //camera.Yaw = 90;
            //            camera.Pitch = 0;
            //        }; break;
            //    case 2: { }; break;
            //    case 3: { }; break;
            //    case 4: { }; break;
            //    default: break;
            //}

        }

        public void Rotate()
        {

        }
    }
}
