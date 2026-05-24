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
    public class TeamsController : Controller
    {
        private readonly Custom_Tournaments_Context _context;

        public TeamsController(Custom_Tournaments_Context context)
        {
            _context = context;
        }

        // GET: Teams
        public async Task<IActionResult> Index(string? searchName, string? searchMember)
        {
            var query = _context.Teams
                .Include(t => t.Owner)
                .Include(t => t.Teammembers)
                    .ThenInclude(tm => tm.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(t => t.Name.ToLower().Contains(searchName.ToLower()));

            if (!string.IsNullOrWhiteSpace(searchMember))
                query = query.Where(t => t.Teammembers
                    .Any(tm => tm.User.Username.ToLower().Contains(searchMember.ToLower())));

            ViewData["searchName"] = searchName;
            ViewData["searchMember"] = searchMember;

            return View(await query.ToListAsync());
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var team = await _context.Teams
                .Include(t => t.Owner)
                .Include(t => t.Teammembers)
                    .ThenInclude(tm => tm.User)
                .Include(t => t.Teammatchresults)
                .Include(t => t.Tournamentparticipants)
                    .ThenInclude(tp => tp.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (team == null) return NotFound();

            var memberUserIds = team.Teammembers
                .Select(tm => tm.Userid)
                .ToHashSet();

            var availableUsers = await _context.Users
                .Where(u => !memberUserIds.Contains(u.Id))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                })
                .ToListAsync();

            ViewBag.AvailableUsers = availableUsers;

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            ViewData["Ownerid"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ownerid,Name,Logourl,Id")] Team team)
        {
            ModelState.Remove("Owner");
            ModelState.Remove("Teammembers");
            ModelState.Remove("Teammatchresults");
            ModelState.Remove("Tournamentparticipants");

            bool nameExists = await _context.Teams
                .AnyAsync(t => t.Name.ToLower() == team.Name.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError("Name", "Команда з такою назвою вже існує");
            }

            if (ModelState.IsValid)
            {
                team.Createdat = DateTime.Now;
                team.Updatedat = DateTime.Now;
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Ownerid"] = new SelectList(_context.Users, "Id", "Username", team.Ownerid);
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            ViewData["Ownerid"] = new SelectList(_context.Users, "Id", "Username", team.Ownerid);
            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Ownerid,Name,Logourl,Createdat,Id")] Team team)
        {
            if (id != team.Id) return NotFound();

            ModelState.Remove("Owner");
            ModelState.Remove("Teammembers");
            ModelState.Remove("Teammatchresults");
            ModelState.Remove("Tournamentparticipants");

            bool nameExists = await _context.Teams
                .AnyAsync(t => t.Name.ToLower() == team.Name.ToLower() && t.Id != team.Id);

            if (nameExists)
            {
                ModelState.AddModelError("Name", "Команда з такою назвою вже існує");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    team.Updatedat = DateTime.Now;
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Ownerid"] = new SelectList(_context.Users, "Id", "Username", team.Ownerid);
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var team = await _context.Teams
                .Include(t => t.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (team == null) return NotFound();

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Teams/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int teamId, int userId, string? role)
        {
            var team = await _context.Teams.FindAsync(teamId);
            var user = await _context.Users.FindAsync(userId);

            if (team == null || user == null) return NotFound();

            bool alreadyMember = await _context.Teammembers
                .AnyAsync(tm => tm.Teamid == teamId && tm.Userid == userId);

            if (!alreadyMember)
            {
                var member = new Teammember
                {
                    Teamid = teamId,
                    Userid = userId,
                    Role = string.IsNullOrWhiteSpace(role) ? "Player" : role,
                    Joinedat = DateTime.Now
                };
                _context.Teammembers.Add(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        // POST: Teams/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, int memberId)
        {
            var member = await _context.Teammembers.FindAsync(memberId);
            if (member != null)
            {
                _context.Teammembers.Remove(member);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
}
