using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebDbFirst.Models;

namespace WebDbFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private IMongoCollection<BsonDocument> _mongoCollection;
        public CustomersController(AdventureWorksLt2019Context context,IOptions<BnbMDBConfig> bnbMDBConfig)
        {
            _context = context;
            var mongoClient = new MongoClient(bnbMDBConfig.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(bnbMDBConfig.Value.DatabaseName);
            _mongoCollection = mongoDatabase.GetCollection<BsonDocument>(bnbMDBConfig.Value.BnBCollectionName);
        }

        [Authorize(Policy = "UserPolicy")]
        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            if (User.IsInRole("Admin"))
            {
                Console.WriteLine($"Eseguo il metodo GetCustomers(), sono il customer: {User.FindFirstValue("CustomerId")} ");
            }

            var custs = await _context.Customers
                .Include(c => c.SalesOrderHeaders)
                .Include(c => c.CustomerAddresses)
                .ToListAsync();

            return Ok(new { status = 200, message = "TUTTO BENONE", totalCustomers = custs.Count, data = custs });
        }

        // GET: api/Customers
        [HttpGet("GetCustomersDto")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersDto()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    NameStyle = c.NameStyle,
                    Title = c.Title,
                    FirstName = c.FirstName,
                    MiddleName = c.MiddleName,
                    LastName = c.LastName,
                    Suffix = c.Suffix,
                    CompanyName = c.CompanyName,
                    SalesPerson = c.SalesPerson,
                    CustomerAddresses = c.CustomerAddresses,
                    SalesOrderHeaders = c.SalesOrderHeaders
                }).ToListAsync();

            return customers;
        }

        [HttpGet("GetEmployeesDtoStream")]
        public async IAsyncEnumerable<CustomerDto> GetCustomersDtoStream()
        {
            var customers = _context.Customers
                .AsNoTracking()
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    NameStyle = c.NameStyle,
                    Title = c.Title,
                    FirstName = c.FirstName,
                    MiddleName = c.MiddleName,
                    LastName = c.LastName,
                    Suffix = c.Suffix,
                    CompanyName = c.CompanyName,
                    SalesPerson = c.SalesPerson,
                    CustomerAddresses = c.CustomerAddresses,
                    SalesOrderHeaders = c.SalesOrderHeaders
                }).AsAsyncEnumerable();
            await foreach (var customer in customers)
            {
                yield return customer;
            }
        }


        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }


        [Route("GetCustomerMDB/{id}")]
        [HttpGet]
        public async Task<ActionResult<CustomerMDB>> GetCustomerMDB(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

           
            if (customer == null)
            {
                return NotFound();  
            }

            CustomerMDB customerMDB = new();

            customerMDB.CustomerId = customer.CustomerId;
            customerMDB.NameStyle = customer.NameStyle;
            customerMDB.Title = customer.Title;
            customerMDB.FirstName = customer.FirstName;
            
            // ... copia gli altri campi necessari ...
            // oppure crea un costruttore in CustomerMDB che accetta un Customer come parametro

            string idString = "63c6a69dd3d19531921ac195";
            ObjectId objectId = ObjectId.Parse(idString);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var resultMDB = await _mongoCollection.Find(filter).FirstOrDefaultAsync();
            
            customerMDB.LocationHotel = resultMDB["host_location"].AsString;
            customerMDB.Price = resultMDB["price"].AsString;

            return customerMDB;
        }

        [Authorize]
        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [Authorize]
        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        [Authorize]
        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
