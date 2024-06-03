using apbd19_cwiczenia9.Data;
using apbd19_cwiczenia9.DTO;
using apbd19_cwiczenia9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd19_cwiczenia9.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripsController(ApbdContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _context.Trips.Select(e => new
        {
            Name = e.Name,
            Countries = e.IdCountries.Select(c => new {
                Name = c.Name
            })
        }).ToListAsync();
        var TripsInclude = await _context.Trips
            .Include(e => e.IdCountries)
            .ToListAsync();
        return Ok(TripsInclude.Select(e => new
        {
            Name = e.Name,
            Country = e.IdCountries.Select(c => new
            {
                Name = c.Name
            })
        }));
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
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientTripDTO request)
    {
        var existingClient = await _context.Clients.FirstOrDefaultAsync(client => client.Pesel == request.Pesel);
        if (existingClient != null)
        {
            return BadRequest("Client with this PESEL exists!");
        }
        var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.IdTrip == idTrip);
        if (trip == null)
        {
            return NotFound("Trip not found!");
        }
        if (trip.DateFrom <= DateTime.Now)
        {
            return BadRequest("Trip already started!");
        }
        var newClient = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Telephone = request.Telephone,
            Pesel = request.Pesel
        };
        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        var clientTrip = new ClientTrip
        {
            IdClient = newClient.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = request.PaymentDate
        };
        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();
        return Ok();
    }
}