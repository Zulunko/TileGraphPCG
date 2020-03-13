using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AFitnessVariable {
    VarDescriber vd;

    protected AFitnessVariable(VarDescriber vd) {
        this.vd = vd;
    }

    // What is the value of this variable for a specific member?
    public abstract float Value(IMemberContainer member);

    // Is this a positive or negative variable?
    public bool IsPositive() {
        return vd.isPositive;
    }
    
    public bool IsInConstraints(float val) {
        return ((vd.min == -1 || val >= vd.min) && (vd.max == -1 || val <= vd.max));
    }

    // Simply the distance to the constraint.
    // Zero if this member is in constraints.
    public float DistanceToConstraint(float val) {
        if (IsInConstraints(val)) return 0;
        if (vd.isPositive) {
            if (val < vd.min) return float.MaxValue;
            return val - vd.max;
        } else {
            if (val > vd.max) return float.MaxValue;
            return vd.min - val;
        }
    }
}
