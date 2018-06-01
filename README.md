# genome-tools

A repositories with code written for exploration and analysis of molecules, especially proteins, and genomics.
NOTE: The code is only loosely structured and makes undocumented assumptions. E.g. the molecule simulation makes very rough approximations of how atoms move relative to each other. The movement is unrealistic.

Why release this code at all?
Some of the code in ChemistryLibrary can be useful for others.
Some useful classes:
- PdbReader, for reading PDB-files
- Atom, Molecule, Peptide: Classes for representing and building chemical structures (using a graph)
- AminoAcidAngleMeasurer, for measuring dihedral angles of amino acids (and in extension generate Ramachandran plots)
- AminoAcidLibrary, builds (human) amino acids, which can be connected to form a peptide.
- UnitValue, Graph and many others in Commons-submodule
Feel free to browse the code and copy whatever is of help to you (see license).

Other repositories:
This repository includes my personal Commons-repository, which is much more structured and provides a wide range of helpful (and tested) classes. Check it out.
