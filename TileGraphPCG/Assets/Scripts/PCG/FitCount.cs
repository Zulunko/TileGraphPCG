using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitCount : AFitnessVariable {
    public FitCount(VarDescriber vd) : base(vd) { }

    public override float Value(IMemberContainer member) {
        return ((MemberTilePCG)member).graph.nodes.Count;
    }
}
