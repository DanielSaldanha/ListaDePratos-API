using ListaDePratos.Data;
using ListaDePratos.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ListaDePratos.Controllers
{
    public class FoodController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoodController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ver cardapio")]
        public async Task<ActionResult> LPget()
        {
            var Vget = await _context.cardapio.ToListAsync();
            return Ok(Vget);
        }
        [HttpPost("colocar novo prato na lista")]
        public async Task<ActionResult> LPpost([FromBody] ApiModel Vpost)
        {
            if (Vpost == null) { return NotFound("valores nulos não serão aceitados"); }
            await _context.cardapio.AddAsync(Vpost);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(LPpost), new { id = Vpost.Id }, Vpost);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> LPput(int id,[FromBody] ApiModel Vput)
        {
            if (id != Vput.Id) { return NotFound("nao encontrado"); }
            _context.Entry(Vput).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> LPDel(int id)
        {
            var Vdel = await _context.cardapio.FindAsync(id);
            if (Vdel == null) { return NotFound("nao encontrado"); }           
            _context.cardapio.Remove(Vdel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
