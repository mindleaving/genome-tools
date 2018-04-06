using System.Linq;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;

namespace ChemistryLibrary.Simulation
{
    public class ChargeRedistributor
    {
        public void RedistributeCharges(Molecule aminoAcid)
        {
            var newCharges = aminoAcid.MoleculeStructure.Vertices
                .ToDictionary(vId => vId.Key, vertex => vertex.Value.Object.EffectiveCharge);
            double chargeChange;
            do
            {
                var currentCharges = newCharges;
                var vertices = aminoAcid.MoleculeStructure.Vertices
                    .Select(v => v.Value);
                foreach (var vertex in vertices)
                {
                    var neighbors = GraphAlgorithms.GetAdjacentVertices(aminoAcid.MoleculeStructure, vertex)
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
                    .ToDictionary(vId => vId.Key, vertex => vertex.Value.Object.EffectiveCharge);
                chargeChange = aminoAcid.MoleculeStructure.Vertices.Keys
                    .Select(vId => (newCharges[vId] - currentCharges[vId]).Abs().In(Unit.ElementaryCharge))
                    .Max();

                //var effectiveCharges = aminoAcid.Atoms.OrderBy(atom => atom.EffectiveCharge)
                //    .Select(atom => new { Atom = atom, Charge = atom.EffectiveCharge.In(Unit.ElementaryCharge) })
                //    .ToList();
            } while (chargeChange > 0.01);
        }
    }
}
