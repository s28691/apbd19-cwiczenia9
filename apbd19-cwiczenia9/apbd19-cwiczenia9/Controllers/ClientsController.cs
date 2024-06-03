using apbd19_cwiczenia9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd19_cwiczenia9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientsController(ApbdContext context)
    {
        _context = context;
    }
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients
            .Include(client => client.ClientTrips)
            .FirstOrDefaultAsync(client => client.IdClient == idClient);
        if (client == null)
        {
            return NotFound("Client not found");
        }
        if (client.ClientTrips.Any())
        {
            return BadRequest("Cannot delete client with connected trips");
        }
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}