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
        public float cameraSpeed = 4f;
        public float sensitivity = 0.4f;

        public bool grabedMouse;
        bool _firstMove;
        public bool isMoved = false;

        Vector2 _lastPos;

        public CameraControl(Vector2i size, bool isOrtogonal = false)
        {
            //_camSize = size;
            cam = new Camera(Vector3.UnitZ * 4, size.X / (float)size.Y, isOrtogonal);
            _firstMove = true;
            grabedMouse = false;
        }

        
        public void OnMouseMove(System.Windows.Input.MouseEventArgs mouse, Point pointGL)
        {
            if (mouse.LeftButton == MouseButtonState.Pressed)
            {
                Vector2 point = new((float)pointGL.X, (float)pointGL.Y);
                if (_firstMove)
                {
                    _firstMove = false;
                    _lastPos = point;
                    return;
                }
                float deltaX = point.X - _lastPos.X;
                float deltaY = point.Y - _lastPos.Y;

                cam.Yaw -= deltaX * sensitivity;
                cam.Pitch -= deltaY * sensitivity;

                if (deltaX != 0 || deltaY != 0)
                {
                    Vector2 newPos = new Vector2
                    {
                        X = _lastPos.X + deltaX,
                        Y = _lastPos.Y + deltaY
                    };
                    _lastPos = newPos;
                }
            }
            else
            {
                _firstMove = true;
            }
        }

        public void OnMouseLeave()
        {
            grabedMouse = false;
            _firstMove = true;
        }

        public void OnMouseEnter()
        {
            grabedMouse = true;
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

            isMoved = true;
        }
    }
}
