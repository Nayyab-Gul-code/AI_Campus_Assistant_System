using AI_Campus_Assistant.Data;

namespace AI_Campus_Assistant.Services
{
    public class SeedDataService
    {
        private readonly MongoDbContext _db;

        public SeedDataService(MongoDbContext db) => _db = db;

        public async Task SeedAsync()
        {
            await SeedData.SeedAsync(_db);
        }
    }
}
