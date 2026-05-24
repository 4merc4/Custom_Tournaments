using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Custom_Tournaments_Infrastructure;

namespace Custom_Tournaments_Infrastructure.Controllers
{
    public class HomeController : Controller
    {
        private readonly Custom_Tournaments_Context _context;

        public HomeController(Custom_Tournaments_Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Tournamentparticipants)
                .Where(t => t.Isprivate == false || t.Isprivate == null)
                .OrderByDescending(t => t.Createdat)
                .Take(6)
                .ToListAsync();

            return View(tournaments);
        }
    }
}