using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Custom_Tournaments.Domain.Model;
using Custom_Tournaments_Infrastructure;

namespace Custom_Tournaments_Infrastructure.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly Custom_Tournaments_Context _context;
        private readonly UserManager<AppUser> _userManager;

        public TournamentsController(
            Custom_Tournaments_Context context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return null;
            return await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityUserId == appUser.Id);
        }

        // GET: Tournaments
        // Пункт 5: фільтруємо завершені
        public async Task<IActionResult> Index(string? searchTitle, string? searchGame,
            decimal? minPrize, decimal? maxPrize, bool showFinished = false)
        {
            var query = _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Tournamentparticipants)
                .AsQueryable();

            // Пункт 5: за замовчуванням не показуємо завершені
            if (!showFinished)
                query = query.Where(t => t.Status != "finished");

            if (!string.IsNullOrWhiteSpace(searchTitle))
                query = query.Where(t => t.Title.ToLower().Contains(searchTitle.ToLower()));

            if (!string.IsNullOrWhiteSpace(searchGame))
                query = query.Where(t => t.Gamename != null && t.Gamename.ToLower().Contains(searchGame.ToLower()));

            if (minPrize.HasValue)
                query = query.Where(t => t.Prizepool >= minPrize.Value);

            if (maxPrize.HasValue)
                query = query.Where(t => t.Prizepool <= maxPrize.Value);

            ViewData["searchTitle"] = searchTitle;
            ViewData["searchGame"] = searchGame;
            ViewData["minPrize"] = minPrize;
            ViewData["maxPrize"] = maxPrize;
            ViewData["showFinished"] = showFinished;

            return View(await query.ToListAsync());
        }

        // GET: Tournaments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Tournamentparticipants)
                    .ThenInclude(tp => tp.Team)
                        .ThenInclude(team => team.Teammembers)
                            .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tournament == null) return NotFound();

            var currentUser = await GetCurrentUserAsync();
            ViewBag.CurrentUserId = currentUser?.Id;
            ViewBag.IsOrganizer = currentUser != null && tournament.Organizerid == currentUser.Id;

            if (tournament.Status == "pending")
            {
                var participantTeamIds = tournament.Tournamentparticipants
                    .Select(tp => tp.Teamid)
                    .ToHashSet();

                var availableTeams = await _context.Teams
                    .Where(t => !participantTeamIds.Contains(t.Id))
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                    .ToListAsync();

                ViewBag.AvailableTeams = availableTeams;
            }

            return View(tournament);
        }

        // GET: Tournaments/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["Organizerid"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Tournaments/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Organizerid,Title,Gamename,Prizepool,Rules,Isprivate,Format,ScoringEnabled,ScoringWin,ScoringDraw,ScoringLoss,MaxRounds,Id")]
            Tournament tournament)
        {
            ModelState.Remove("Organizer");
            ModelState.Remove("Tournamentparticipants");
            ModelState.Remove("Tournamentmatches");
            ModelState.Remove("Teammatchresults");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                ViewBag.DebugErrors = string.Join(" | ", errors);
                ViewData["Organizerid"] = new SelectList(_context.Users, "Id", "Username", tournament.Organizerid);
                return View(tournament);
            }

            tournament.Status = "pending";
            tournament.Createdat = DateTime.Now;
            tournament.Updatedat = DateTime.Now;
            _context.Add(tournament);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = tournament.Id });
        }

        // GET: Tournaments/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || tournament.Organizerid != currentUser.Id)
                return Forbid();

            if (tournament.Status != "pending")
            {
                TempData["Error"] = "Не можна редагувати турнір який вже розпочався";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewData["Organizerid"] = new SelectList(_context.Users, "Id", "Username", tournament.Organizerid);
            return View(tournament);
        }

        // POST: Tournaments/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Organizerid,Title,Gamename,Prizepool,Rules,Isprivate,Format,ScoringEnabled,ScoringWin,ScoringDraw,ScoringLoss,MaxRounds,Createdat,Id")]
            Tournament tournament)
        {
            if (id != tournament.Id) return NotFound();

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || tournament.Organizerid != currentUser.Id)
                return Forbid();

            ModelState.Remove("Organizer");
            ModelState.Remove("Tournamentparticipants");
            ModelState.Remove("Tournamentmatches");
            ModelState.Remove("Teammatchresults");

            if (ModelState.IsValid)
            {
                try
                {
                    tournament.Status = "pending";
                    tournament.Updatedat = DateTime.Now;
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewData["Organizerid"] = new SelectList(_context.Users, "Id", "Username", tournament.Organizerid);
            return View(tournament);
        }

        // GET: Tournaments/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tournament == null) return NotFound();

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || tournament.Organizerid != currentUser.Id)
                return Forbid();

            return View(tournament);
        }

        // POST: Tournaments/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || tournament.Organizerid != currentUser.Id)
                return Forbid();

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Tournaments/AddTeam
        // Пункт 3: перевірка що гравці команди не в інших командах цього турніру
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeam(int tournamentId, int teamId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Tournamentparticipants)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);
            var team = await _context.Teams
                .Include(t => t.Teammembers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (tournament == null || team == null) return NotFound();

            if (tournament.Status != "pending")
            {
                TempData["Error"] = "Не можна додавати команди до турніру що вже почався";
                return RedirectToAction(nameof(Details), new { id = tournamentId });
            }

            bool alreadyParticipant = tournament.Tournamentparticipants
                .Any(tp => tp.Teamid == teamId);

            if (alreadyParticipant)
            {
                TempData["Error"] = "Ця команда вже є учасником турніру";
                return RedirectToAction(nameof(Details), new { id = tournamentId });
            }

            // Пункт 3: перевіряємо чи гравці нової команди вже є в командах цього турніру
            var existingTeamIds = tournament.Tournamentparticipants
                .Select(tp => tp.Teamid)
                .ToHashSet();

            var newTeamMemberIds = team.Teammembers
                .Select(tm => tm.Userid)
                .ToHashSet();

            // Знаходимо всіх гравців існуючих команд
            var existingMemberIds = await _context.Teammembers
                .Where(tm => existingTeamIds.Contains(tm.Teamid))
                .Select(tm => tm.Userid)
                .ToListAsync();

            var conflictMemberIds = newTeamMemberIds
                .Intersect(existingMemberIds)
                .ToList();

            if (conflictMemberIds.Any())
            {
                var conflictNames = await _context.Users
                    .Where(u => conflictMemberIds.Contains(u.Id))
                    .Select(u => u.Username)
                    .ToListAsync();

                TempData["Error"] = $"Неможливо додати команду: гравці {string.Join(", ", conflictNames)} вже беруть участь в інших командах цього турніру";
                return RedirectToAction(nameof(Details), new { id = tournamentId });
            }

            var participant = new Tournamentparticipant
            {
                Tournamentid = tournamentId,
                Teamid = teamId,
                Joinedat = DateTime.Now
            };
            _context.Tournamentparticipants.Add(participant);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = tournamentId });
        }

        // POST: Tournaments/RemoveTeam
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTeam(int tournamentId, int participantId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return NotFound();

            if (tournament.Status != "pending")
            {
                TempData["Error"] = "Не можна видаляти команди з турніру що вже почався";
                return RedirectToAction(nameof(Details), new { id = tournamentId });
            }

            var participant = await _context.Tournamentparticipants.FindAsync(participantId);
            if (participant != null)
            {
                _context.Tournamentparticipants.Remove(participant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = tournamentId });
        }

        private bool TournamentExists(int id)
        {
            return _context.Tournaments.Any(e => e.Id == id);
        }
    }
}