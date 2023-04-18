using UnityEngine;

namespace Utils {
    // 坐标和角度信息
    public struct MobiusRingBlock {
        public Vector3 position;
        public Quaternion rotation;
        public float angle;
    }
    public class MobiusRing {
        // 构造函数，参数有环形的格数，环形的半径，角度的0点
        public MobiusRing(int numBlocksU, float ringRadius, float angleZero = 0) {
            this.numBlocksU = numBlocksU;
            this.ringRadius = ringRadius;
        }
        
        private int numBlocksU;
        private float ringRadius;

        // 获取环形上的点
        public Vector3 GetPositionOnMobiusRing(float u)
        {
            float x = ringRadius * Mathf.Cos(u);
            float y = ringRadius * Mathf.Sin(u);
            return new Vector3(x, y, 0);
        }
        
        // 获取环形上的点的旋转
        public Quaternion GetRotationOnMobiusRing(float u)
        {
            Vector3 tangentU = new Vector3(
                -Mathf.Sin(u),
                Mathf.Cos(u),
                0
            );
    
            Vector3 tangentV = new Vector3(
                Mathf.Cos(u) * Mathf.Cos(u / 2) / 2,
                Mathf.Sin(u) * Mathf.Cos(u / 2) / 2,
                Mathf.Sin(u / 2)/ 2
            );  
            Vector3 normal = Vector3.Cross(tangentU, tangentV).normalized;
            return Quaternion.LookRotation(tangentV, normal);
        }
    }
}


        