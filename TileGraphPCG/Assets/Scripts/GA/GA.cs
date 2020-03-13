using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GA {
    static List<IMemberContainer> population;

    // All categories are mutually exclusive until outBoundary is selected (becomes a subset of outPareto).
    // These categories are listed in order of selection priority.
    static Dictionary<IMemberContainer, List<float>> inPareto; // In constraints, in Pareto
    static Dictionary<IMemberContainer, List<float>> outBoundary; // Closest to constraints (outside) in Pareto set. Empty until Select() is called.
    static List<IMemberContainer> inConstraints; // Inside constraints
    static Dictionary<IMemberContainer, List<float>> outPareto; // Outside constraints, in Pareto
    static List<IMemberContainer> outConstraints; // Outside constraints;


    static int popSize;
    static Dictionary<IMemberContainer, List<float>> finalCandidates;
    static AFitnessVariable[] variables;
    static StreamWriter writer;

    // Randomly generate population (0 generation)
    public static void Initialize<T>(int popSize, AFitnessVariable[] variables) where T : IMemberContainer, new() {
        GA.variables = variables;
        GA.popSize = popSize;
        population = new List<IMemberContainer>();
        inPareto = new Dictionary<IMemberContainer, List<float>>();
        outBoundary = new Dictionary<IMemberContainer, List<float>>();
        inConstraints = new List<IMemberContainer>();
        outPareto = new Dictionary<IMemberContainer, List<float>>();
        outConstraints = new List<IMemberContainer>();
        finalCandidates = new Dictionary<IMemberContainer, List<float>>();
        for (int i = 0; i < popSize; i++) {
            T member = new T();
            member.Generate();
            population.Add(member);
        }
        writer = new StreamWriter("Assets/output.txt", false);
    }

    // Run GA for numGens:
    // 1. Fitness calculation
    // 2. Selection (top 50%)
    // 3. Mutation of selected members
    // 4. Crossover of selected members to make children
    public static Dictionary<IMemberContainer, List<float>> Run<T>(int numGens) where T : IMemberContainer, new() {
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < numGens; i++) {
            StartNewGen();

            foreach (IMemberContainer member in population) {
                member.GeneratePhenotype();
                CheckStoreParetoMember(member);
            }

            Select();

            // Mutation. Should be probabilistic, but I'm going to put that in the Mutate method itself.
            foreach (IMemberContainer member in population) {
                member.Mutate();
            }
            List<IMemberContainer> newChildren = new List<IMemberContainer>();
            for (int j = 0; j < population.Count / 2; j++) {
                newChildren.AddRange(population[j * 2].Crossover(population[j * 2 + 1]));
            }
            population.AddRange(newChildren);
        }

        StartNewGen();

        // Final evaluation for returned member
        foreach (IMemberContainer member in population) {
            member.GeneratePhenotype();
            CheckStoreParetoMember(member);
        }
        /*writer.Write(writer.NewLine);
        foreach (KeyValuePair<IMemberContainer, List<float>> kv in paretoFront) {
            foreach (float val in kv.Value) {
                writer.Write(val + "\t");
            }
            writer.Write(writer.NewLine);
        }*/
        writer.Close();

        Dictionary<IMemberContainer, List<float>> validMembers = new Dictionary<IMemberContainer, List<float>>();
        // XXX Only return results within constraints.
        foreach (KeyValuePair<IMemberContainer, List<float>> kvp in finalCandidates) {
            bool isValid = true;
            for (int i = 0; i < variables.Length; i++) {
                if (!variables[i].IsInConstraints(kvp.Value[i]))
                    isValid = false;
            }
            if (isValid) {
                validMembers.Add(kvp.Key, kvp.Value);
            }
        }

        Debug.Log("Runtime " + (Time.realtimeSinceStartup - startTime) + " secs.");
        return validMembers;
    }

    private static void StartNewGen() {
        Debug.Log("Clearing...");
        inPareto.Clear();
        outBoundary.Clear();
        inConstraints.Clear();
        outPareto.Clear();
        outConstraints.Clear();
    }

    private static void Select() {
        // First, need to calculate outBoundary from outPareto.
        foreach (KeyValuePair<IMemberContainer, List<float>> mvp in outPareto) {
            List<float> vals = new List<float>();
            for (int i = 0; i < variables.Length; i++) {
                vals.Add(variables[i].DistanceToConstraint(mvp.Value[i]));
            }
            List<IMemberContainer> removeList = new List<IMemberContainer>();
            bool dominated = false;
            foreach (KeyValuePair<IMemberContainer, List<float>> kvp in outBoundary) {
                switch (CompareMembers(mvp.Key, vals, kvp.Key, kvp.Value)) {
                    case MemberComparison.DOMINATED:
                        dominated = true;
                        break;
                    case MemberComparison.DOMINATES:
                        removeList.Add(kvp.Key);
                        break;
                    case MemberComparison.NEUTRAL:
                        break;
                }
                if (dominated) break;
            }
            if (dominated)
                continue;
            foreach (IMemberContainer rm in removeList) {
                outBoundary.Remove(rm);
            }
            outBoundary.Add(mvp.Key, vals);
        }

        int inP = 0, outB = 0, inC = 0, outP = 0, outC = 0;
        population.Clear();
        // First, add members that are in constraints and in the Pareto front.
        // These are the members we want to base our next generation on.
        foreach (IMemberContainer m in inPareto.Keys) {
            if (population.Count == popSize / 2) break;
            inP++;
            population.Add(m);
        }
        // Next, add members that are closest to constraints and in the Pareto front.
        // This will ensure we explore the edge of feasibility.
        foreach (IMemberContainer m in outBoundary.Keys) {
            if (population.Count == popSize / 2) break;
            outB++;
            population.Add(m);
        }
        // Next, add members that are in constraints but not in the Pareto front.
        // This pushes our feasible members toward the front.
        foreach (IMemberContainer m in inConstraints) {
            if (population.Count == popSize / 2) break;
            inC++;
            population.Add(m);
        }
        // Next, add members that are out of constraints but in the Pareto front.
        // We're running out of useful things to add here, so this is just being hopeful.
        foreach (IMemberContainer m in outPareto.Keys) {
            if (population.Count == popSize / 2) break;
            if (population.Contains(m)) continue;
            outP++;
            population.Add(m);
        }
        // Finally, add literally anything else if we still haven't managed to get half the population size.
        // Hopefully this won't be used often.
        foreach (IMemberContainer m in outConstraints) {
            if (population.Count == popSize / 2) break;
            outC++;
            population.Add(m);
        }
        // Generate new pop members?
        //while (population.Count < popSize / 2) {
        //}

        Debug.Log("Selected: " + inP + " " + outB + " " + inC + " " + outP + " " + outC);
    }

    // Stores members in the correct list, EXCEPT FOR OUTBOUNDARY.
    // outBoundary will be calculated in Select, as it's trivial to calculate once you have the full set.
    // outBoundary members are in outPareto with the outPareto set once this function is over.
    private static void CheckStoreParetoMember(IMemberContainer m) {
        bool isInConstraints = true; ;
        List<float> vals = new List<float>();
        foreach (AFitnessVariable fv in variables) {
            float val = fv.Value(m);
            // If any value is out of the constraints, immediately reject it.
            // XXX Disregard invalid state.
            if (!fv.IsInConstraints(val))
                isInConstraints = false;
            vals.Add(fv.Value(m));
        }
        /*foreach (float val in vals) {
            writer.Write(val + "\t");
        }
        writer.Write(writer.NewLine);*/
        List<IMemberContainer> membersToRemove = new List<IMemberContainer>();
        Dictionary<IMemberContainer, List<float>> candidateList;
        if (isInConstraints)
            candidateList = inPareto;
        else
            candidateList = outPareto;
        // Check if we're in the current Pareto front
        foreach (KeyValuePair<IMemberContainer, List<float>> ckv in candidateList) {
            switch (CompareMembers(m, vals, ckv.Key, ckv.Value)) {
                case MemberComparison.DOMINATED:
                    if (!isInConstraints)
                        outConstraints.Add(m);
                    else
                        inConstraints.Add(m);
                    return;
                case MemberComparison.DOMINATES:
                    membersToRemove.Add(ckv.Key);
                    break;
                case MemberComparison.NEUTRAL:
                    break;
            }
        }
        foreach (IMemberContainer rm in membersToRemove) {
            candidateList.Remove(rm);
        }
        candidateList.Add(m, vals);
        if (isInConstraints) {
            CheckAddFinalCandidate(m, vals);
        }
        return;
    }

    private static void CheckAddFinalCandidate(IMemberContainer m, List<float> vals) {
        List<IMemberContainer> membersToRemove = new List<IMemberContainer>();
        foreach (KeyValuePair<IMemberContainer, List<float>> kvp in finalCandidates) {
            switch (CompareMembers(m, vals, kvp.Key, kvp.Value)) {
                case MemberComparison.DOMINATED:
                    return;
                case MemberComparison.DOMINATES:
                    membersToRemove.Add(kvp.Key);
                    break;
                case MemberComparison.NEUTRAL:
                    break;
            }
        }
        foreach (IMemberContainer rm in membersToRemove) {
            finalCandidates.Remove(rm);
        }
        finalCandidates.Add(m.CopyGenotype(), vals);
    }

    enum MemberComparison {
        DOMINATES,
        DOMINATED,
        NEUTRAL
    }

    private static MemberComparison CompareMembers(IMemberContainer m1, List<float> v1, IMemberContainer m2, List<float> v2) {
        bool isGreaterInOneDimension = false;
        bool isLessInOneDimension = false;
        for (int i = 0; i < v1.Count; i++) {
            if (variables[i].IsPositive()) {
                if (v1[i] > v2[i]) {
                    isGreaterInOneDimension = true;
                } else {
                    isLessInOneDimension = true;
                }
            } else {
                if (v1[i] < v2[i]) {
                    isGreaterInOneDimension = true;
                } else {
                    isLessInOneDimension = true;
                }
            }
        }
        // The new solution is dominated.
        if (!isGreaterInOneDimension) {
            return MemberComparison.DOMINATED;
        }
        // This existing solution is dominated by the new solution.
        if (!isLessInOneDimension) {
            return MemberComparison.DOMINATES;
        }
        return MemberComparison.NEUTRAL;
    }
}
