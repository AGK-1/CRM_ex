using CRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrmController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CrmController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            // Retrieve all users from the database
            var users = await _context.user.ToListAsync();

            // Select only the desired properties (excluding Id)
            var response = users.Select(u => new
            {
                u.name,
                u.email,
                u.role
            }).ToList();

            return Ok(response);
        }

        [HttpGet("get-users-2")]
        public async Task<IActionResult> GetUserrs()
        {
            var users = await _context.user.ToListAsync();

            // Return the list of users directly, instead of wrapping it
            return Ok(users);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            // Fetch all companies from the database
            var companies = await _context.customer.ToListAsync();

            // Shape the response to match your desired format
            var response = companies.Select(company => new
            {
                companyName = company.CompanyName,       // Assuming Name corresponds to CompanyName
                contactName = company.ContactName, // Assuming ContactName is a property of your company model
                phone = company.Phone   // Assuming PhoneNumber is a property of your company model
            });

            return Ok(response);
        }

        // POST: api/customers
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Customers>> PostCustomer(Customers customer)
        {
            _context.customer.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCustomers), new { id = customer.Id }, customer);
        }

       
    }
}
