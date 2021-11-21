using System.Threading.Tasks;
using MongoDB.Driver;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class GeneStatisticsDb
    {
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<GeneVariantStatistics> geneVariantStatisticsCollection;

        public GeneStatisticsDb(string databaseName)
        {
            var client = new MongoClient("mongodb://localhost");
            database = client.GetDatabase(databaseName);
            geneVariantStatisticsCollection = database.GetCollection<GeneVariantStatistics>(nameof(GeneVariantStatistics));
        }

        public async Task<GeneVariantStatistics> GetGeneStatistics(string geneSymbol)
        {
            return await geneVariantStatisticsCollection.Find(x => x.GeneSymbol == geneSymbol).FirstOrDefaultAsync();
        }

        public async Task StoreGeneStatistics(GeneVariantStatistics geneStatistics)
        {
            await geneVariantStatisticsCollection.ReplaceOneAsync(x => x.Id == geneStatistics.Id, geneStatistics, new ReplaceOptions { IsUpsert = true });
        }
    }
}
