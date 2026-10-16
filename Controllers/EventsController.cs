using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ChurchFlow.api.Models;


[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ChurchFlowDbContext _context;

    public EventsController(ChurchFlowDbContext context)
    {
        _context = context;
    }
    //Get all events
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events
            .Include(e => e.Contributions)
                .ThenInclude(c => c.Member)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.EventId)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Type,
                e.Location,
                e.Status,
                e.Date,
                e.ContributionAmount,
                e.Poster,
                e.InvitationLetter,
                Contributions = e.Contributions.Select(c => new
                {
                    c.ContrId,
                    c.Status,
                    c.AmountPaid,
                    c.TotalAmount,
                    c.LastPaidDate,
                    MemberName = c.Member != null ? c.Member.Name + " " + c.Member.Surname : "Unknown"
                })
            })
            .ToListAsync();

        return Ok(events);
    }
    //Get events by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // FindAsync only works with the primary key and doesn't support
        // .Include(), so i switch to a regular query here instead —
        // same pattern as GetAll, just filtered down to one event.
        var eventEntity = await _context.Events
            .Include(e => e.Contributions)
                .ThenInclude(c => c.Member)
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Type,
                e.Location,
                e.Status,
                e.Date,
                e.ContributionAmount,
                e.Poster,
                e.InvitationLetter,
                Contributions = e.Contributions
    .GroupBy(c => c.MemberId)
    .Select(group => new
    {
        MemberId = group.Key,   // <-- add this if it's missing
        MemberName = group.First().Member != null
            ? group.First().Member!.Name + " " + group.First().Member!.Surname
            : "Unknown",
        AmountPaid = group.Sum(c => c.AmountPaid),
        TotalAmount = group.Sum(c => c.TotalAmount),
        LastPaidDate = group.Max(c => c.LastPaidDate),
        Status = group.Any(c => c.Status == "Pending") ? "Pending" : "Accepted"
    })
            })
            // FirstOrDefaultAsync returns the first matching item, or null
            // if none matched — same idea as FindAsync, but works after
            // a Select/Where chain like this one.
            .FirstOrDefaultAsync();

        if (eventEntity == null)
            return NotFound($"Event {id} does not exist");

        return Ok(eventEntity);
    }
    //Add new Event
    [Authorize(Roles = "Chairperson")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Event newEvent)
    {
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return Ok(newEvent);
    }
    //Update Existing event
    [Authorize(Roles = "Chairperson")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Event newEvent)
    {
        var _event = await _context.Events.FindAsync(id);
        if (_event == null)
            return NotFound("Event does not exists");
        _event.Name = newEvent.Name;
        _event.Status = newEvent.Status;
        _event.Location = newEvent.Location;
        _event.Date = newEvent.Date;
        _event.ContributionAmount = newEvent.ContributionAmount;
        _event.Type = newEvent.Type;
        _event.Poster = newEvent.Poster;
        _event.InvitationLetter = newEvent.InvitationLetter;
        //update database
        await _context.SaveChangesAsync();
        return Ok(_event);
    }
    //Delete an Event
    [Authorize(Roles = "Chairperson")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingEvent = await _context.Events.FindAsync(id);
        if (existingEvent == null)
            return NotFound($"Event {id} does not exist");

        try
        {
            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();
            return Ok($"Event {id} has been deleted");
        }
        catch (DbUpdateException)
        {
            return BadRequest("Cannot delete this event because it has existing contributions linked to it.");
        }
    }
}
