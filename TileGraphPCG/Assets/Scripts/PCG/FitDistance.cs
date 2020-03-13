using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitDistance : AFitnessVariable {
    public FitDistance(VarDescriber vd) : base(vd) { }

    public override float Value(IMemberContainer member) {
        List<RenderedTileInfo> renderedNodes = new List<RenderedTileInfo>(((MemberTilePCG)member).renderedTiles.Values);
        float maxDistance = 0;
        foreach (RenderedTileInfo n in renderedNodes) {
            if (n.location.magnitude > maxDistance) {
                maxDistance = n.location.magnitude;
            }
        }
        return maxDistance;
    }
}
