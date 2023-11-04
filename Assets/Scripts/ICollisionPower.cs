using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    internal interface ICollisionPower
    {
        public float GetCollisionPower();
        public UnityEngine.Vector3 GetMomentum();
    }
}
