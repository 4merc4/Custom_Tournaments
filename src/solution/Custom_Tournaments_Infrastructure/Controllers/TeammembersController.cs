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
    public class TeammembersController : Controller
    {
        private readonly Custom_Tournaments_Context _context;

        public TeammembersController(Custom_Tournaments_Context context)
        {
            _context = context;
        }

        // GET: Teammembers
        public async Task<IActionResult> Index()
        {
            var custom_Tournaments_Context = _context.Teammembers.Include(t => t.Team).Include(t => t.User);
            return View(await custom_Tournaments_Context.ToListAsync());
        }

        // GET: Teammembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teammember = await _context.Teammembers
                .Include(t => t.Team)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teammember == null)
            {
                return NotFound();
            }

            return View(teammember);
        }

        // GET: Teammembers/Create
        public IActionResult Create()
        {
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name");
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Teammembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Teamid,Userid,Role,Joinedat,Id")] Teammember teammember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teammember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", teammember.Teamid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Email", teammember.Userid);
            return View(teammember);
        }

        // GET: Teammembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teammember = await _context.Teammembers.FindAsync(id);
            if (teammember == null)
            {
                return NotFound();
            }
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", teammember.Teamid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Email", teammember.Userid);
            return View(teammember);
        }

        // POST: Teammembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Teamid,Userid,Role,Joinedat,Id")] Teammember teammember)
        {
            if (id != teammember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teammember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeammemberExists(teammember.Id))
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
            ViewData["Teamid"] = new SelectList(_context.Teams, "Id", "Name", teammember.Teamid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Email", teammember.Userid);
            return View(teammember);
        }

        // GET: Teammembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teammember = await _context.Teammembers
                .Include(t => t.Team)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teammember == null)
            {
                return NotFound();
            }

            return View(teammember);
        }

        // POST: Teammembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teammember = await _context.Teammembers.FindAsync(id);
            if (teammember != null)
            {
                _context.Teammembers.Remove(teammember);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeammemberExists(int id)
        {
            return _context.Teammembers.Any(e => e.Id == id);
        }
    }
}
