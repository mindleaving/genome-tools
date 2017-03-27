import sys;
import subprocess;
import math;

class Atom:
    Symbol = "";
    AminoAcidAtomName = "";
    AminoAcidName = "";
    AminoAcidIdx = -1;
    Position = (0,0,0);
    AtomIdx = -1;

    def __init__(self, symbol, name, atomIdx, x, y, z):
        self.Symbol = symbol;
        self.AminoAcidAtomName = name;
        self.Position = (x,y,z);
        self.AtomIdx = atomIdx;
    def __repr__(self):
        return self.AminoAcidAtomName;
    def __str__(self):
        return self.AminoAcidAtomName;

def magnitude(v):
    return math.sqrt(sum(v[i]*v[i] for i in range(len(v))))

def add(u, v):
    return [ u[i]+v[i] for i in range(len(u)) ]

def subtract(u, v):
    return [ u[i]-v[i] for i in range(len(u)) ]

def scalarMultiply(scalar, v):
    return [ scalar*v[i] for i in range(len(v)) ]

def dot(u, v):
    return sum(u[i]*v[i] for i in range(len(u)))

def normalize(v):
    vmag = magnitude(v)
    return [ v[i]/vmag  for i in range(len(v)) ]

def addAminoAcidToPdb(inputFileName, outputFileName, aminoAcidCode):
    if len(aminoAcidCode) == 1:
        aminoAcidCode = mapOneLetterToThreeLetterAminoAcidCode(aminoAcidCode);
    inFile = open(inputFileName,"r");
    outFile = open(outputFileName,"w");
    lastLine = "";
    lastBackboneNitrogen = None;
    lastBackboneCarbon = None;
    newPeptideLength = None;
    for line in inFile:
        if lastLine.startswith("SEQRES"):
            [ aminoAcids, peptideLength, lineIdx ] = parseSequence(lastLine);
            if newPeptideLength is None:
                newPeptideLength = peptideLength+1;
            if line.startswith("SEQRES"):
                seqresLine = generateSeqresLine(aminoAcids, newPeptideLength, lineIdx);
                outFile.write(seqresLine);
            else:
                if len(aminoAcids) < 13:
                    aminoAcids.append(aminoAcidCode);
                    seqresLine = generateSeqresLine(aminoAcids, newPeptideLength, lineIdx);
                    outFile.write(seqresLine);
                else:
                    seqresLine = generateSeqresLine(aminoAcids, newPeptideLength, lineIdx);
                    outputFile.write(lastLine);
                    seqresLine = generateSeqresLine([aminoAcidCode], newPeptideLength, lineIdx+1);
                    outFile.write(seqresLine);
            outFile.write(line);
        elif line.startswith("TER"):
            lastAtom = parseAtom(lastLine);
            lastAtomIdx = lastAtom.AtomIdx;
            outFile.write(lastLine);
            aminoAcidAtoms = getAminoAcidAtoms(aminoAcidCode, newPeptideLength);
            positionAminoAcid(aminoAcidAtoms, lastBackboneNitrogen, lastBackboneCarbon);
            atomLines = generateAtomLines(aminoAcidAtoms, lastAtomIdx+1);
            for i in range(len(atomLines)):
                outFile.write(atomLines[i]);
            terminalLine = generateTerminalLine();
            outFile.write(terminalLine);
        elif not lastLine.startswith("TER"):
            outFile.write(lastLine);
        
        if line.startswith("ATOM"):
            atom = parseAtom(line);
            if atom.AminoAcidAtomName == "N":
                lastBackboneNitrogen = atom;
            elif atom.AminoAcidAtomName == "C":
                lastBackboneCarbon = atom;
        lastLine = line;
    outFile.write(lastLine);
    outFile.close();
    inFile.close();

def generateSeqresLine(aminoAcids, peptideLength, lineIdx):
    seqresLine = "SEQRES {:3} A {:4}  {}".format(lineIdx,peptideLength," ".join(aminoAcids));
    return seqresLine.ljust(80) + "\n";

def getAminoAcidAtoms(aminoAcidCode, aminoAcidIdx):
    aminoAcidFileName = "../AminoAcids/" + aminoAcidCode + ".txt";
    file = open(aminoAcidFileName,"r");
    atoms = [];
    for line in file:
        atom = parseAtom(line);
        atom.AminoAcidName = aminoAcidCode;
        atom.AminoAcidIdx = aminoAcidIdx;
        atoms.append(atom);
    return atoms;

def positionAminoAcid(aminoAcidAtoms, lastBackboneNitrogen, lastBackboneCarbon):
    lastBackboneDirection = subtract(lastBackboneCarbon.Position, lastBackboneNitrogen.Position);
    normalizedBackboneDirection = normalize(lastBackboneDirection);
    [ rBB, thetaBB, phiBB ] = calculateSphericalCoordinates(normalizedBackboneDirection);

    aminoAcidNitrogen = None;
    aminoAcidCarbon = None;
    for i in range(len(aminoAcidAtoms)):
        atom = aminoAcidAtoms[i];
        if atom.AminoAcidAtomName == "N":
            aminoAcidNitrogen = atom;
        elif atom.AminoAcidAtomName == "C":
            aminoAcidCarbon = atom;
    aminoAcidDirection = subtract(aminoAcidCarbon.Position, aminoAcidNitrogen.Position);
    normalizedAminoAcidDirection = normalize(aminoAcidDirection);
    [ rAA, thetaAA, phiAA ] = calculateSphericalCoordinates(normalizedAminoAcidDirection);

    oldNitrogenPosition = aminoAcidNitrogen.Position;
    newNitrogenPosition = add(lastBackboneCarbon.Position,\
                           scalarMultiply(1.32,normalizedBackboneDirection));
    dTheta = thetaBB - thetaAA;
    dPhi = phiBB - phiAA;
    for i in range(len(aminoAcidAtoms)):
        atom = aminoAcidAtoms[i];
        nitrogenCenteredPosition = subtract(atom.Position, oldNitrogenPosition);
        [ r, theta, phi ] = calculateSphericalCoordinates(nitrogenCenteredPosition);
        rotatedPosition = calculateCartesianCoordinates(r,theta+dTheta,phi+dPhi);
        atom.Position = tuple(add(rotatedPosition,newNitrogenPosition));

def calculateCartesianCoordinates(r,theta,phi):
    x = r*math.sin(theta)*math.cos(phi);
    y = r*math.sin(theta)*math.sin(phi);
    z = r*math.cos(theta);
    return [ x, y, z ];

def calculateSphericalCoordinates(v3D):
    r = magnitude(v3D);
    if r == 0:
        return [ r, 0, 0 ];
    theta = math.acos(v3D[2]/r);
    phi = math.acos(v3D[0]/(r*math.sin(theta)));
    if v3D[1] < 0 and phi > 0:
        phi = -phi;
    return [ r, theta, phi ];
    

def generateAtomLines(atoms, nextAtomIdx):
    atomLines = [];
    for idx in range(len(atoms)):
        atom = atoms[idx];
        atomLine = generateAtomLine(atom, nextAtomIdx);
        atomLines.append(atomLine);
        nextAtomIdx += 1;
    return atomLines;
    
def generateAtomLine(atom, nextAtomIdx):
    return "ATOM  {:>5} {:>4} {:3} A{:4}    {:8.3f}{:8.3f}{:8.3f}{:6.2f}{:6.2f}          {:>2}  \n".format(\
        nextAtomIdx,\
        atom.AminoAcidAtomName,\
        atom.AminoAcidName,\
        atom.AminoAcidIdx,\
        atom.Position[0],\
        atom.Position[1],\
        atom.Position[2],\
        1.0,\
        0.0,\
        atom.Symbol);

def generateTerminalLine():
    return "TER".ljust(80) + "\n";

##def parsePdb(filename):
##    file = open(filename, "r");
##    atoms = [];
##    sequence = [];
##    for line in file:
##        if line.startswith("SEQRES"):
##            aminoAcids = parseSequence(line);
##            sequence.extend(aminoAcids);
##        elif line.startswith("ATOM"):
##            atom = parseAtom(line);
##            if type(atom) is Atom:
##                print(atom.Position);
##                atoms.append(atom);
##    return [ sequence, atoms ];
            

def parseSequence(line):
    "Parses PDB SEQRES line"
    if line[:6] != "SEQRES":
        raise Exception;
    lineIdx = int(line[7:10]);
    peptideLength = int(line[13:17]);
    aminoAcids = line[19:].split();
    return [ aminoAcids, peptideLength, lineIdx ];

def parseAtom(line):
    "Parses PDB ATOM line"
    if line[:4] != "ATOM":
        raise Exception;
    atomName = line[12:16].strip();
    symbol = line[76:78].strip();
    atomIdx = int(line[6:11]);
    x = float(line[30:38]);
    y = float(line[38:46]);
    z = float(line[46:54]);
    return Atom(symbol,atomName,atomIdx,x,y,z);

def mapOneLetterToThreeLetterAminoAcidCode(oneLetterCode):
    if oneLetterCode == "A":
        return "ALA";
    if oneLetterCode == "G":
        return "GLY";
    if oneLetterCode == "I":
        return "ILE";
    if oneLetterCode == "L":
        return "LEU";
    if oneLetterCode == "P":
        return "PRO";
    if oneLetterCode == "V":
        return "VAL";
    if oneLetterCode == "F":
        return "PHE";
    if oneLetterCode == "W":
        return "TRP";
    if oneLetterCode == "Y":
        return "TYR";
    if oneLetterCode == "D":
        return "ASP";
    if oneLetterCode == "E":
        return "GLU";
    if oneLetterCode == "R":
        return "ARG";
    if oneLetterCode == "H":
        return "HIS";
    if oneLetterCode == "K":
        return "LYS";
    if oneLetterCode == "S":
        return "SER";
    if oneLetterCode == "T":
        return "THR";
    if oneLetterCode == "C":
        return "CYS";
    if oneLetterCode == "M":
        return "MET";
    if oneLetterCode == "N":
        return "ASN";
    if oneLetterCode == "Q":
        return "GLN";

def main(argv):
    #filename = argv[1];
    #aminoAcidCode = argv[2];
    inputFileName = "cftr_1_2.pdb";
    outputFileName = "cftr_1_3.pdb";
    aminoAcidCode = "A";
    addAminoAcidToPdb(inputFileName, outputFileName, aminoAcidCode);

if __name__ == "__main__":
    main(sys.argv);
