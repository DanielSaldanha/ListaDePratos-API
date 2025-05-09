using ListaDePratos.Data;
using ListaDePratos.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace ListaDePratos.Controllers
{
    public class FoodController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public FoodController(IMemoryCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
        }
        [HttpGet("ver cardapio sem cache")]
        public async Task<ActionResult> LPGetN()
        {
            Stopwatch stopwatch = new Stopwatch(); // Inicia o cronômetro
            stopwatch.Start();

            var Vget = _context.cardapio.ToListAsync();

            stopwatch.Stop(); // Para o cronômetro
            var elapsedTime = stopwatch.ElapsedMilliseconds; // Tempo em milissegundos

            return Ok(new
            {
                Data = Vget,//sem o contador ele ficaria só com o Vget
                ExecucaoSemCache = elapsedTime // Retorna o tempo de execução
            });
        }

        [HttpGet("ver cardapio com cache")]
        public async Task<ActionResult> LPget()
        {
          

            string cacheKey = "cardapioCache"; // A chave do cache
                                               
            Stopwatch stopwatch = new Stopwatch(); // Inicia o cronômetro
            stopwatch.Start();

            // Tenta obter dados do cache
            if (!_cache.TryGetValue(cacheKey, out List<ApiModel> cachedCardapio))
            {
                // Se não estiver no cache, obtém do banco de dados
                cachedCardapio = await _context.cardapio.ToListAsync();

                // Configuração das opções do cache
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30), // Expira após 30 minutos
                    SlidingExpiration = TimeSpan.FromMinutes(10) // Expiração deslizante de 10 minutos
                };

                // Armazena os dados no cache
                _cache.Set(cacheKey, cachedCardapio, cacheOptions);
            }
            stopwatch.Stop(); // Para o cronômetro
            var elapsedTime = stopwatch.ElapsedMilliseconds; // Tempo em milissegundos
            return Ok(new
            {
                Data = cachedCardapio,//sem o contador ele ficaria só com o cachedCardapio
                ExecucaoComCache = elapsedTime // Retorna o tempo de execução
            });
        }

        [HttpPost("colocar novo prato na lista")]
        public async Task<ActionResult> LPpost([FromBody] ApiModel Vpost)
        {
            if (Vpost == null) { return NotFound("valores nulos não serão aceitados"); }
            await _context.cardapio.AddAsync(Vpost);
            await _context.SaveChangesAsync();

            // Invalida o cache
            string cacheKey = "cardapioCache";
            _cache.Remove(cacheKey); // Remove a entrada do cache da lista de cardápios

            return CreatedAtAction(nameof(LPpost), new { id = Vpost.Id }, Vpost);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> LPput(int id,[FromBody] ApiModel Vput)
        {
            // Verifica se o id do Cardapio recebido no corpo está correto
            if (id != Vput.Id)
            {
                return BadRequest();
            }
            // Atualiza o cardápio no banco de dados
            _context.Entry(Vput).State = EntityState.Modified;   
            await _context.SaveChangesAsync();

            // Invalida o cache
            string cacheKey = "cardapioCache";
            _cache.Remove(cacheKey); // Remove o item do cache para que a próxima requisição busque do banco.

            return NoContent(); // Retorna 204 No Content após a atualização bem-sucedida
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> LPDel(int id)
        {
            var Vdel = await _context.cardapio.FindAsync(id);
            if (Vdel == null) { return NotFound("nao encontrado"); }           
            _context.cardapio.Remove(Vdel);
            await _context.SaveChangesAsync();

            // Invalida o cache
            string cacheKey = "cardapioCache";
            _cache.Remove(cacheKey); // Remove o item do cache para que a próxima requisição busque do banco.
            return NoContent();
        }

       
    }
}
