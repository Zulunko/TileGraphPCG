using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileBoundingBox {
    public Vector3 center;
    public Vector3 halfExtents;
    public float rotation;

    public bool IsPlaceable(Vector3 location, float yRot) {
        return !Physics.CheckBox(location + Quaternion.Euler(0, rotation + yRot, 0)*center, halfExtents, Quaternion.Euler(0, rotation + yRot, 0));//, ~LayerMask.GetMask("floor"));
    }
}
