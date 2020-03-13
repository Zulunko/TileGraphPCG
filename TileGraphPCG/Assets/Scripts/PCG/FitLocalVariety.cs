using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitLocalVariety : AFitnessVariable {
    public FitLocalVariety(VarDescriber vd) : base(vd) { }

    public override float Value(IMemberContainer member) {
        List<TileNode> nodes = ((MemberTilePCG)member).graph.nodes;
        float val = 0;
        foreach (TileNode node in nodes) {
            foreach (TileNode c in node.connectedNodes) {
                if (node.type != c.type) val += 1;
            }
        }
        return val;
    }
}
