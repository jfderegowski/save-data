using System;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Serializable
{
    [Serializable]
    public struct SerializeTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public SerializeTransform(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }
        
        public SerializeTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    public static class SerializeTransformExtensions
    {
        public static SerializeTransform ToSerializeTransform(this Transform transform) => new(transform);

        public static void FromSerializeTransform(this Transform transform, SerializeTransform serializeTransform)
        {
            transform.position = serializeTransform.position;
            transform.rotation = serializeTransform.rotation;
            transform.localScale = serializeTransform.scale;
        }
    }
}