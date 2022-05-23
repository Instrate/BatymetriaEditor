﻿using System;
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
        public bool isMoved = false;

        Vector2 _lastPos;

        //public int viewStyle = 0;

        public CameraControl(Vector2i size, bool isOrtogonal = false)
        {
            _camSize = size;
            cam = new Camera(Vector3.UnitZ * 4, size.X / (float)size.Y, isOrtogonal);

            //cam.Yaw = 0;

            _firstMove = true;
            grabedMouse = false;
        }

        
        public void OnMouseMove(System.Windows.Input.MouseEventArgs mouse, Size size, Point pointGL, Point pointScreen)
        {
            Vector2 point = new((float)pointGL.X, (float)pointGL.Y);
            float deltaX = 0;
            float deltaY = 0;
            if (mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (_firstMove)
                {
                    _firstMove = false;
                    _lastPos = point;
                    return;
                }
                
                deltaX = point.X - _lastPos.X;
                deltaY = point.Y - _lastPos.Y;

                cam.Yaw -= deltaX * sensitivity;
                cam.Pitch -= deltaY * sensitivity;
            }
            else
            {
                _firstMove = true;
                return;
            }
            if(deltaX != 0 || deltaY != 0)
            {
                Vector2 newPos = new Vector2
                {
                    X = _lastPos.X + deltaX,
                    Y = _lastPos.Y + deltaY
                };
                _lastPos = newPos;
            }
            

            //Console.WriteLine("lastPos: ");
            //Console.WriteLine(_lastPos);
            //Console.WriteLine();
        }

        public void OnMouseLeave()
        {
            grabedMouse = false;
            _firstMove = true;
            //_lastPos = Vector2.Zero;
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
