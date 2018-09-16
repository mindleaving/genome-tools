using System;
using System.Collections.Generic;

namespace ChemistryLibrary.IO.Pdb
{
    public class PdbReaderResult : IDisposable
    {
        public PdbReaderResult(params PdbModel[] models)
        {
            Models.AddRange(models);
        }

        public List<PdbModel> Models { get; } = new List<PdbModel>();

        public void Dispose()
        {
            Models.ForEach(model => model.Dispose());
            Models.Clear();
        }
    }
}