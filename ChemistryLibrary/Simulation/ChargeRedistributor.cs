using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class ChargeRedistributor
    {
        public void RedistributeCharges(Molecule aminoAcid)
        {
            var newCharges = aminoAcid.MoleculeStructure.Vertices
                .ToDictionary(vertex => vertex.Id, vertex => vertex.Object.EffectiveCharge);
            double chargeChange;
            do
            {
                var currentCharges = newCharges;
                var vertices = aminoAcid.MoleculeStructure.Vertices;
                foreach (var vertex in vertices)
                {
                    var neighbors = GraphAlgorithms.GetAdjacentVertices(aminoAcid.MoleculeStructure, vertex)
                        .Cast<IVertex<Atom>>()
                        .Select(v => v.Object)
                        .ToList();
                    var atoms = new[] { vertex.Object }.Concat(neighbors).ToList();
                    var chargeSum = atoms.Sum(x => x.EffectiveCharge.In(Unit.ElementaryCharge))
                                    - atoms.Count;
                    var electroNegativitySum = atoms.Sum(x => x.ElectroNegativity);
                    foreach (var atom in atoms)
                    {
                        atom.EffectiveCharge = (1 + chargeSum * (atom.ElectroNegativity / electroNegativitySum)).To(Unit.ElementaryCharge);
                    }
                }
                newCharges = aminoAcid.MoleculeStructure.Vertices
                    .ToDictionary(vertex => vertex.Id, vertex => vertex.Object.EffectiveCharge);
                chargeChange = aminoAcid.MoleculeStructure.Vertices
                    .Select(v => v.Id)
                    .Select(vId => (newCharges[vId] - currentCharges[vId]).Abs().In(Unit.ElementaryCharge))
                    .Max();

                //var effectiveCharges = aminoAcid.Atoms.OrderBy(atom => atom.EffectiveCharge)
                //    .Select(atom => new { Atom = atom, Charge = atom.EffectiveCharge.In(Unit.ElementaryCharge) })
                //    .ToList();
            } while (chargeChange > 0.01);
        }
    }
}
