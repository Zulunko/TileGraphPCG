using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMemberContainer {
    // Creates the initial random version of this member.
    //   SHOULD ONLY BE CALLED ONCE AT THE START OF THE GA.
    void Generate();
    // Should mutate this member in-place
    void Mutate();
    void GeneratePhenotype();
    void FinalRender(Vector3 loc, List<float> fitVals, GameObject parent);
    void DeleteRender();
    // Returns exactly two children based on this member and the given parent
    //   NOTE: DO NOT CALL THIS ON THE OTHER PARENT; THIS DOES BOTH SIMULTANEOUSLY
    IMemberContainer[] Crossover(IMemberContainer otherParent);
    // Only copies the Genotype.
    IMemberContainer CopyGenotype();
}
