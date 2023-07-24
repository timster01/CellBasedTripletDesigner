[![DOI](https://zenodo.org/badge/670157817.svg)](https://zenodo.org/badge/latestdoi/670157817)

# Usage
Open the Unity project using Unity(2021.3.19f1)

# libraries
- SimpleFileBrowser(v1.5.9)
- Clipper2 library(1.2.0)
- Ear Clipping Triangulation scripts by Sebastian Langue(commit 7828bde on master)

License files can be found in the folders where the libraries or packages are used, respectively:
- Assets/Plugins/SimpleFileBrowser
- Assets/Libs
- Assets/Scripts/External/Ear-Clipping-Triangulation (Git submodule)

# Controls
## General
Left-Click - Activate button or change 2D grid cells active shape

## Camera
C - Switch Camera view
Hold Right-Mouse - Allows camera yaw and pitch by moving the mouse
R - Camera position up
F - Camera position down
Q - Camera position up relative to current view direction
E - Camera position down relative to current view direction

## Buttons
Buttons that apply to the adjacent grid
Blue floppy icon next to 2D grid - Save the current shape to a file
White floppy with arrow icon next to 2D grid - Load a shape from a file
Curved arrow - Rotate the shape clockwise
Arrows pointing at each other - Mirroring in the direction shown by the arrows

Buttons next to the 3D trip-let
Blue floppy icon next to 2D grid - Save the current trip-let to a file
White floppy with arrow icon next to 2D grid - Load a batch of shapes from a folder and test all combinations, result is written to a file
Curved arrow - Test all combinations of rotation and mirror operations on the current shapes in the grid and returns the most connected valid result

# Gridsize
Gridsize can be adjusted. 
all sizes of files will still work for the tripletbuilder, 
but can be cut off or in the bottom left corner. 
The batch runner only handles files of the currently set size and breaks if not all files in the dataset are the current size.