using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Custom_Tournaments.Domain.Model;
using Custom_Tournaments_Infrastructure;

namespace Custom_Tournaments_Infrastructure.Controllers
{
    public class TournamentparticipantsController : Controller
    {
        private readonly Custom_Tournaments_Context _context;

        public TournamentparticipantsController(Custom_Tournaments_Context context)
        {
            _context = context;
        }

        // GET: Tournamentparticipants
        public async Task<IActionResult> Index()
        {
            var custom_Tournaments_Context = _context.Tournamentparticipants.Include(t => t.Team).Include(t => t.Tournament);
            return View(await custom_Tournaments_Context.ToListAsync());
        }

        // GET: Tournamentparticipants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournamentparticipant = await _context.Tournamentparticipants
                .Include(t => t.Team)
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tournamentparticipant == null)
            {
                return NotFound();
            }

            return View(tournamentparticipant);
        }

        // GET: Tournamentparticipants/Create
        public IActionResult Create()
        {
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name");
            ViewData["Tournamentid"] = new SelectList(_context.Tournaments, "Id", "Title");
            return View();
        }

        // POST: Tournamentparticipants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tournamentid,Teamid,Joinedat,Id")] Tournamentparticipant tournamentparticipant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tournamentparticipant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", tournamentparticipant.Teamid);
            ViewData["Tournamentid"] = new SelectList(_context.Tournaments, "Id", "Title", tournamentparticipant.Tournamentid);
            return View(tournamentparticipant);
        }

        // GET: Tournamentparticipants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournamentparticipant = await _context.Tournamentparticipants.FindAsync(id);
            if (tournamentparticipant == null)
            {
                return NotFound();
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", tournamentparticipant.Teamid);
            ViewData["Tournamentid"] = new SelectList(_context.Tournaments, "Id", "Title", tournamentparticipant.Tournamentid);
            return View(tournamentparticipant);
        }

        // POST: Tournamentparticipants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Tournamentid,Teamid,Joinedat,Id")] Tournamentparticipant tournamentparticipant)
        {
            if (id != tournamentparticipant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tournamentparticipant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentparticipantExists(tournamentparticipant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", tournamentparticipant.Teamid);
            ViewData["Tournamentid"] = new SelectList(_context.Tournaments, "Id", "Title", tournamentparticipant.Tournamentid);
            return View(tournamentparticipant);
        }

        // GET: Tournamentparticipants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournamentparticipant = await _context.Tournamentparticipants
                .Include(t => t.Team)
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tournamentparticipant == null)
            {
                return NotFound();
            }

            return View(tournamentparticipant);
        }

        // POST: Tournamentparticipants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tournamentparticipant = await _context.Tournamentparticipants.FindAsync(id);
            if (tournamentparticipant != null)
            {
                _context.Tournamentparticipants.Remove(tournamentparticipant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TournamentparticipantExists(int id)
        {
            return _context.Tournamentparticipants.Any(e => e.Id == id);
        }
    }
}
