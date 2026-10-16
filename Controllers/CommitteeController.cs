using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ChurchFlow.api.Models;

[ApiController]
[Route("api/[controller]")]
public class CommitteeController : ControllerBase
{
    private readonly ChurchFlowDbContext _context;
    private readonly PasswordHasher<Committee> _passwordHasher;

    public CommitteeController(ChurchFlowDbContext context, PasswordHasher<Committee> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Committee newCommittee)
    {
        newCommittee.Password = _passwordHasher.HashPassword(newCommittee, newCommittee.Password);
        _context.Committees.Add(newCommittee);
        await _context.SaveChangesAsync();
        return Ok(newCommittee);
    }
    //Get all Mambers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var _members = await _context.Committees.ToListAsync();
        return Ok(_members);

    }
    //Get committee member by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var _member = await _context.Committees.FindAsync(id);
        if (_member == null)
            return NotFound("Memeber not found");
        return Ok(_member);
    }
    //Update Member details
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Committee committee)
    {
        var _member = await _context.Committees.FindAsync(id);
        if (_member == null)
            return NotFound("committe Member does not exist");
        _member.MemberId = committee.MemberId;
        _member.Name = committee.Name;
        _member.Position = committee.Position;
        _member.Email = committee.Email;
        if (!string.IsNullOrWhiteSpace(committee.Password))
        {
            _member.Password = _passwordHasher.HashPassword(_member, committee.Password);
        }
        await _context.SaveChangesAsync();
        return Ok(_member);
    }
    //Delete Memeber
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var _member = await _context.Committees.FindAsync(id);
        if (_member == null)
            return NotFound("Committee member does not exist");
        try
        {
            _context.Committees.Remove(_member);
            await _context.SaveChangesAsync();
            return Ok("Memeber" + id + "has been removed");
        }
        catch (DbUpdateException)
        {
            return BadRequest("Cannot delete this committee member because it has existing contributions linked to it.");
        }
    }
}