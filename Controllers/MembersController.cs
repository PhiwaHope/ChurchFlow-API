using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChurchFlow.api.Models;

[ApiController]
[Route("api/[controller]")]

public class MembersController : ControllerBase
{
    private readonly ChurchFlowDbContext _context;

    public MembersController(ChurchFlowDbContext context)
    {
        _context = context;
    }

    //Get all Mambers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var _members = await _context.Members.ToListAsync();
        return Ok(_members);

    }
    //Get member by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var _member = await _context.Members.FindAsync(id);
        if (_member == null)
            return NotFound("Memeber not found");
        return Ok(_member);
    }
    //Add new member
    [HttpPost]
    public async Task<IActionResult> AddMember([FromBody] Member newMember)
    {
        _context.Members.Add(newMember);
        await _context.SaveChangesAsync();
        return Ok(newMember);
    }
    //Update Member details
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Member member)
    {
        var _member = await _context.Members.FindAsync(id);
        if (_member == null)
            return NotFound("Member does not exist");
        _member.Name = member.Name;
        _member.Surname = member.Surname;
        _member.Phone = member.Phone;
        _member.Email = member.Email;
        _member.FundingStatus = member.FundingStatus;
        _member.JoinDate = member.JoinDate;
        await _context.SaveChangesAsync();
        return Ok(_member);
    }
    //Delete Memeber
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var _member = await _context.Members.FindAsync(id);
        if (_member == null)
            return NotFound("Member does not exist");
        try
        {
            _context.Members.Remove(_member);
            await _context.SaveChangesAsync();
            return Ok("Memeber" + id + "has been removed");
        }
        catch (DbUpdateException)
        {
            return BadRequest("Cannot delete this event because it has existing contributions linked to it.");
        }
    }
}