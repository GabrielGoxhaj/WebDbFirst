using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebDbFirst.Models;

namespace WebDbFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BnBController : ControllerBase
    {
        private IMongoCollection<BsonDocument> _bnbCollection;

        public BnBController(IOptions<BnbMDBConfig> bnbMDBConfig)
        {
            var mongoClient = new MongoClient(bnbMDBConfig.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(bnbMDBConfig.Value.DatabaseName);
            _bnbCollection = mongoDatabase.GetCollection<BsonDocument>(bnbMDBConfig.Value.BnBCollectionName);
        }

        [HttpGet]
        public async Task<IActionResult> GetBnBs()
        {
            var bnbs = await _bnbCollection.Find(new BsonDocument()).ToListAsync();
            var mdbJson = new List<object>();
           
            foreach (var bnb in bnbs)
            {
                mdbJson.Add(bnb.ToJson());
            }

            return Ok(new { data = mdbJson });
        }

        [HttpGet("{City},{pageSize}")]
        public async Task<IEnumerable<object>> GetBnbByCity(string City, int pageNum, int pageSize=10)
        {
            var filter = Builders<BsonDocument>
                .Filter
                .Regex(BnB => BnB["neighbourhood"], new BsonRegularExpression($"^{City}"));

            var bnbs = await _bnbCollection.Find(filter)
                .Skip((pageNum - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            
            var mdbJson = new List<object>();
            
            foreach (var bnb in bnbs)
            {
                mdbJson.Add(bnb.ToJson());
            }
            
            return mdbJson;
        }
    }
}
