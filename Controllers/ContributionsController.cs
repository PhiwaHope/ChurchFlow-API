using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ChurchFlow.api.Models;

[ApiController]
[Route("api/[controller]")]

public class ContributionController : ControllerBase
{
    private readonly ChurchFlowDbContext _context;

    public ContributionController(ChurchFlowDbContext context)
    {
        _context = context;
    }

    //Get all Contributions
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var _contr = await _context.Contributions.ToListAsync();
        return Ok(_contr);

    }
    //Get Contribution by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var _contr = await _context.Contributions.FindAsync(id);
        if (_contr == null)
            return NotFound("contribution not found");
        return Ok(_contr);
    }
    //Add new Contribution
    [Authorize(Roles = "Treasurer")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentRequest payment)
    {
        // 1. Look for an existing Contribution for this member+event combo
        var existing = await _context.Contributions
            .FirstOrDefaultAsync(c => c.EventId == payment.EventId && c.MemberId == payment.MemberId);

        Contribution contribution;

        var _member = await _context.Members.FindAsync(payment.MemberId);
        var _event = await _context.Events.FindAsync(payment.EventId);

        if (_member == null || _event == null)
            return NotFound("Member or Event not found");

        if (existing != null)
        {
            // 2a. Found one — accumulate the payment onto the running total
            existing.AmountPaid += payment.Amount;
            existing.LastPaidDate = DateTime.UtcNow;
            existing.Status = existing.AmountPaid >= existing.TotalAmount ? "Accepted" : "Pending";

            contribution = existing;
        }
        else
        {
            // 2b. Not found — this is their first payment for this event
            contribution = new Contribution
            {
                EventId = payment.EventId,
                MemberId = payment.MemberId,
                AmountPaid = payment.Amount,
                TotalAmount = _event.ContributionAmount,
                LastPaidDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Contributions.Add(contribution);
        }

        // 3. Save now so a NEW Contribution gets a real ContrId assigned
        //    by the database — we need that id below for the Transaction's
        //    RefId. (If it was an existing Contribution, this just saves
        //    the updated AmountPaid/LastPaidDate/Status.)
        await _context.SaveChangesAsync();

        // 4. Log this specific payment as a Transaction

        var transaction = new Transaction
        {
            Description = $"{_member.Name} {_member.Surname} - {_event.Name}",
            Type = TransactionType.Income,
            Amount = payment.Amount,
            Date = DateTime.UtcNow,
            PaymentMethod = payment.PaymentMethod,
            RefId = contribution.ContrId,
            RefType = RefType.Contribution
        };

        _context.Transactions.Add(transaction);

        // 5. Save the Transaction, then return both pieces so the frontend
        //    has everything it needs without a follow-up request
        await _context.SaveChangesAsync();

        return Ok(new { contribution, transaction });
    }
    //Update Contribution details
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContr(int id, [FromBody] Contribution contr)
    {
        var _contr = await _context.Contributions.FindAsync(id);
        if (_contr == null)
            return NotFound("Contribution does not exist");
        _contr.EventId = contr.EventId;
        _contr.MemberId = contr.MemberId;
        _contr.Status = contr.Status;
        _contr.LastPaidDate = contr.LastPaidDate;
        _contr.AmountPaid = contr.AmountPaid;
        _contr.TotalAmount = contr.TotalAmount;

        await _context.SaveChangesAsync();
        return Ok(_contr);
    }
    //Delete Contribution
    [Authorize(Roles = "Treasurer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContr(int id)
    {
        var _contr = await _context.Contributions.FindAsync(id);
        if (_contr == null)
            return NotFound("Contribution does not exist");
        try
        {
            _context.Contributions.Remove(_contr);
            await _context.SaveChangesAsync();
            return Ok("contribution" + id + "has been removed");
        }
        catch (DbUpdateException)
        {
            return BadRequest("Bad request my G");
        }
    }
}