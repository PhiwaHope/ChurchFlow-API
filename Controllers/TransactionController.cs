using ChurchFlow.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ChurchFlowDbContext _context;
    public TransactionController(ChurchFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _context.Transactions.OrderByDescending(t => t.Date)
        .ThenByDescending(t => t.TransId).ToListAsync();
        if (transactions.Count == 0)
            return NotFound("No transactions found");
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var trans = await _context.Transactions.FindAsync(id);
        if (trans == null)
            return NotFound("No transaction found");
        return Ok(trans);
    }

    // GET api/Transaction/event/5 — all transactions linked to one event
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.RefType == RefType.Event && t.RefId == eventId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return Ok(transactions);
    }

    [Authorize(Roles = "Treasurer")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transaction transaction)
    {
        transaction.Date = DateTime.UtcNow;

        switch (transaction.RefType)
        {
            case RefType.Event:
                var _event = await _context.Events.FindAsync(transaction.RefId);
                if (_event == null)
                    return NotFound("Event not found");

                transaction.Description = $"{transaction.Description} - {_event.Name}";
                break;

            case RefType.Offering:
                transaction.Description = $"Offering - {transaction.Date:dd MMM yyyy} ({transaction.Description})";
                break;

            case RefType.FundRaising:
                transaction.Description = $"FundRaising - {transaction.Date:dd MMM yyyy} ({transaction.Description})";
                break;

                // Contribution, Sale, Other, or null: leave the description as typed.
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return Ok(transaction);
    }

    [Authorize(Roles = "Treasurer")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Transaction updatedTransaction)
    {
        var trans = await _context.Transactions.FindAsync(id);
        if (trans == null)
            return NotFound("No transaction found");

        // Copy over scalar fields from the incoming request onto the
        // tracked entity — same pattern as ContributionController.Update.
        trans.Description = updatedTransaction.Description;
        trans.Type = updatedTransaction.Type;
        trans.Amount = updatedTransaction.Amount;
        trans.PaymentMethod = updatedTransaction.PaymentMethod;
        trans.RefId = updatedTransaction.RefId;
        trans.RefType = updatedTransaction.RefType;

        // Deliberately NOT copying Date here — Date represents when the
        // transaction was originally created, so an edit shouldn't silently
        // change that. If you want to allow editing the date too, add it
        // above like the other fields.

        await _context.SaveChangesAsync();
        return Ok(trans);
    }

    [Authorize(Roles = "Treasurer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var trans = await _context.Transactions.FindAsync(id);
        if (trans == null)
            return NotFound("No transaction found");

        // Wrapped in try/catch, same as EventsController.Delete, in case
        // deleting a transaction ever runs into a database constraint —
        // not currently expected here since Transaction has no incoming
        // foreign keys, but it's a safe habit for anything touching
        // financial records.
        try
        {
            _context.Transactions.Remove(trans);
            await _context.SaveChangesAsync();
            return Ok($"Transaction {id} deleted");
        }
        catch (DbUpdateException)
        {
            return BadRequest("Cannot delete this transaction");
        }
    }


}