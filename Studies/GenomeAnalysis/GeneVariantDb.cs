using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class GeneVariantDb
    {
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<GeneVariant> geneVariantsCollection;
        private readonly IMongoCollection<GeneVariantStatistics> geneVariantStatisticsCollection;
        private readonly IMongoCollection<AggregatedGeneVariantStatistics> aggregatedGeneVariantStatisticsCollection;


        public GeneVariantDb(string databaseName)
        {
            var client = new MongoClient("mongodb://localhost");
            database = client.GetDatabase(databaseName);
            geneVariantsCollection = database.GetCollection<GeneVariant>(nameof(GeneVariant));
            geneVariantStatisticsCollection = database.GetCollection<GeneVariantStatistics>(nameof(GeneVariantStatistics));
            aggregatedGeneVariantStatisticsCollection = database.GetCollection<AggregatedGeneVariantStatistics>(nameof(AggregatedGeneVariantStatistics));
        }

        public Task<List<GeneVariantStatistics>> GetGeneVariantStatistics(string geneSymbol)
        {
            return geneVariantStatisticsCollection.Find(x => x.GeneSymbol == geneSymbol).ToListAsync();
        }

        public Task<List<GeneVariantStatistics>> GetGeneVariantStatistics(Expression<Func<GeneVariantStatistics, bool>> filter)
        {
            return geneVariantStatisticsCollection.Find(filter).ToListAsync();
        }

        public async Task Store(GeneVariantStatistics geneStatistics)
        {
            await geneVariantStatisticsCollection.ReplaceOneAsync(
                x => x.GeneSymbol == geneStatistics.GeneSymbol && x.PersonId == geneStatistics.PersonId && x.ParentalOrigin == geneStatistics.ParentalOrigin, 
                geneStatistics, 
                new ReplaceOptions { IsUpsert = true });
        }

        public Task<AggregatedGeneVariantStatistics> GetAggregatedGeneVariantStatistics(string geneSymbol)
        {
            return aggregatedGeneVariantStatisticsCollection.Find(x => x.GeneSymbol == geneSymbol).FirstOrDefaultAsync();
        }

        public async Task Store(AggregatedGeneVariantStatistics geneStatistics)
        {
            await aggregatedGeneVariantStatisticsCollection.ReplaceOneAsync(x => x.GeneSymbol == geneStatistics.GeneSymbol, geneStatistics, new ReplaceOptions { IsUpsert = true });
        }

        public Task<List<GeneVariant>> GetGeneVariants(string geneSymbol)
        {
            return geneVariantsCollection.Find(x => x.GeneSymbol == geneSymbol).ToListAsync();
        }

        public async Task Store(GeneVariant geneVariant)
        {
            await geneVariantsCollection.InsertOneAsync(geneVariant);
        }
    }
}
