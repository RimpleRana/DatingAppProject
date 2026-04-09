using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")] // localhost:5000/api/members
    [ApiController]
    public class MembersController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            var members = await context.Users.ToListAsync();
            return Ok(members);
        }

        [HttpGet("{id}")] // localhost:5001/api/members/bob-id
        public async Task<IActionResult> GetMember(string id)
        {
            var member = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }
    }
}
