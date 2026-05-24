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
    public class TeammatchresultsController : Controller
    {
        private readonly Custom_Tournaments_Context _context;

        public TeammatchresultsController(Custom_Tournaments_Context context)
        {
            _context = context;
        }

        // GET: Teammatchresults
        public async Task<IActionResult> Index()
        {
            var custom_Tournaments_Context = _context.Teammatchresults.Include(t => t.Team);
            return View(await custom_Tournaments_Context.ToListAsync());
        }

        // GET: Teammatchresults/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teammatchresult = await _context.Teammatchresults
                .Include(t => t.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teammatchresult == null)
            {
                return NotFound();
            }

            return View(teammatchresult);
        }

        // GET: Teammatchresults/Create
        public IActionResult Create()
        {
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name");
            return View();
        }

        // POST: Teammatchresults/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Teamid,Gamename,Opponentname,Result,Score,Duration,Gamestats,Playedat,Id")] Teammatchresult teammatchresult)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teammatchresult);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", teammatchresult.Teamid);
            return View(teammatchresult);
        }

        // GET: Teammatchresults/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teammatchresult = await _context.Teammatchresults.FindAsync(id);
            if (teammatchresult == null)
            {
                return NotFound();
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", teammatchresult.Teamid);
            return View(teammatchresult);
        }

        // POST: Teammatchresults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Teamid,Gamename,Opponentname,Result,Score,Duration,Gamestats,Playedat,Id")] Teammatchresult teammatchresult)
        {
            if (id != teammatchresult.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teammatchresult);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeammatchresultExists(teammatchresult.Id))
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
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", teammatchresult.Teamid);
            return View(teammatchresult);
        }

        // GET: Teammatchresults/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teammatchresult = await _context.Teammatchresults
                .Include(t => t.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teammatchresult == null)
            {
                return NotFound();
            }

            return View(teammatchresult);
        }

        // POST: Teammatchresults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teammatchresult = await _context.Teammatchresults.FindAsync(id);
            if (teammatchresult != null)
            {
                _context.Teammatchresults.Remove(teammatchresult);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeammatchresultExists(int id)
        {
            return _context.Teammatchresults.Any(e => e.Id == id);
        }
    }
}
