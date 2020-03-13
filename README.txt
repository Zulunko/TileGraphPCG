This is Eric Lang's Multiobjective Evolutionary Tile-Based PCG project. This corresponds to the first part of this DC submission: https://www.aaai.org/ojs/index.php/AIIDE/article/view/5247

A quick note on words: PCG is "Procedural Content Generation" and GA is "Genetic Algorithm".

It is a Unity project. It was originally created in Unity version 2018.1.0f2, but it should work in similar versions.

All PCG-specific code is in the Assets/Scripts/PCG folder, while all GA (generalized) code is in the Assets/Scripts/GA folder. The tiles are in Assets/Tiles. If you would like to add more tiles, make them a prefab, follow the format of the current tiles, and add them to the Tiles list on the "PCGRunner" GameObject.

To run:
1. Open the project in Unity
2. Set any variables that are desired on the "PCGRunner" GameObject.
3. Run the project.
4. Once the algorithm is complete (Unity is no longer frozen), hit A to render the maximum and minimum samples for each variable or hit S to render the entire Pareto front found.
