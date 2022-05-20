using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Moveable
    {
        public Vector3 position = Vector3.Zero;

        private protected Matrix4 transform = Matrix4.Identity;
        private protected Matrix4 originShift = Matrix4.Identity;

        public virtual void Move(Vector3 shifts)
        {
            originShift = originShift * Matrix4.CreateTranslation(shifts);

            TransformCombiner();
        }

        public virtual void TransformCombiner()
        {
            transform *= originShift;
        }

        public virtual void TransformClean()
        {
            transform = originShift = Matrix4.Identity;
        }
    }
}
