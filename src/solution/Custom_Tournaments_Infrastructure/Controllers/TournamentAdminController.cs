using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Custom_Tournaments.Domain.Model;
using Custom_Tournaments_Infrastructure;

namespace Custom_Tournaments_Infrastructure.Controllers
{
    [Authorize]
    public class TournamentAdminController : Controller
    {
        private readonly Custom_Tournaments_Context _context;
        private readonly UserManager<AppUser> _userManager;

        public TournamentAdminController(
            Custom_Tournaments_Context context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<(bool isOrganizer, Tournament? tournament, User? currentUser)>
            CheckOrganizerAsync(int tournamentId)
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return (false, null, null);

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityUserId == appUser.Id);
            if (currentUser == null) return (false, null, null);

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Tournamentparticipants)
                    .ThenInclude(tp => tp.Team)
                        .ThenInclude(t => t.Teammembers)
                            .ThenInclude(tm => tm.User)
                .Include(t => t.Tournamentmatches)
                    .ThenInclude(m => m.Matchparticipants)
                        .ThenInclude(mp => mp.Team)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null) return (false, null, currentUser);

            bool isOrganizer = tournament.Organizerid == currentUser.Id;
            return (isOrganizer, tournament, currentUser);
        }

        // GET: TournamentAdmin/Manage/5
        public async Task<IActionResult> Manage(int id)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(id);
            if (tournament == null) return NotFound();
            if (!isOrganizer) return Forbid();
            return View(tournament);
        }

        // POST: TournamentAdmin/Start/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(id);
            if (tournament == null) return NotFound();
            if (!isOrganizer) return Forbid();

            if (tournament.Status != "pending")
                return RedirectToAction(nameof(Manage), new { id });

            if (tournament.Tournamentparticipants.Count < 2)
            {
                TempData["Error"] = "Потрібно щонайменше 2 команди для початку турніру";
                return RedirectToAction(nameof(Manage), new { id });
            }

            tournament.Status = "active";
            tournament.Updatedat = DateTime.Now;
            await GenerateRoundAsync(tournament);
            _context.Update(tournament);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id });
        }

        // POST: TournamentAdmin/Finish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(int id)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(id);
            if (tournament == null) return NotFound();
            if (!isOrganizer) return Forbid();

            tournament.Status = "finished";
            tournament.Updatedat = DateTime.Now;
            await AssignFinalPlacesAsync(tournament);
            _context.Update(tournament);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Tournaments", new { id });
        }

        // POST: TournamentAdmin/GenerateNextRound/5
        // Пункт 6: перевірка MaxRounds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateNextRound(int id)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(id);
            if (tournament == null) return NotFound();
            if (!isOrganizer) return Forbid();

            int currentRound = tournament.Tournamentmatches.Any()
                ? tournament.Tournamentmatches.Max(m => m.Round)
                : 0;

            // Пункт 6: якщо досягли максимуму раундів — не генеруємо
            if (currentRound >= tournament.MaxRounds)
            {
                TempData["Error"] = $"Досягнуто максимальну кількість раундів ({tournament.MaxRounds})";
                return RedirectToAction(nameof(Manage), new { id });
            }

            bool allFinished = !tournament.Tournamentmatches
                .Where(m => m.Round == currentRound)
                .Any(m => m.Status != "finished");

            if (!allFinished)
            {
                TempData["Error"] = "Спочатку завершіть усі матчі поточного раунду";
                return RedirectToAction(nameof(Manage), new { id });
            }

            await GenerateRoundAsync(tournament);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id });
        }

        // POST: TournamentAdmin/StartMatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMatch(int matchId, int tournamentId)
        {
            var (isOrganizer, _, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();

            var match = await _context.Tournamentmatches.FindAsync(matchId);
            if (match == null) return NotFound();

            match.Status = "active";
            match.StartedAt = DateTime.Now;
            _context.Update(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // POST: TournamentAdmin/FinishMatch
        // Пункт 4: для battleroyal місця переносяться у FinalPlace
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishMatch(int matchId, int tournamentId,
            List<int> teamIds, List<string> scores, List<int> places, List<bool> winners)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();
            if (tournament == null) return NotFound();

            var match = await _context.Tournamentmatches
                .Include(m => m.Matchparticipants)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return NotFound();

            match.Status = "finished";
            match.FinishedAt = DateTime.Now;

            for (int i = 0; i < teamIds.Count; i++)
            {
                var participant = match.Matchparticipants
                    .FirstOrDefault(mp => mp.Teamid == teamIds[i]);

                if (participant != null)
                {
                    participant.Score = i < scores.Count ? scores[i] : null;
                    participant.Place = i < places.Count && places[i] > 0 ? places[i] : null;
                    participant.IsWinner = i < winners.Count && winners[i];

                    // Балова система
                    if (tournament.ScoringEnabled)
                    {
                        if (participant.IsWinner)
                            participant.Points = tournament.ScoringWin;
                        else if (!match.Matchparticipants.Any(mp => mp.IsWinner))
                            participant.Points = tournament.ScoringDraw;
                        else
                            participant.Points = tournament.ScoringLoss;
                    }

                    _context.Update(participant);

                    // Пункт 4: для battleroyal — одразу переносимо місце в tournamentparticipant
                    if (tournament.Format == "battleroyal" && participant.Place.HasValue)
                    {
                        var tp = tournament.Tournamentparticipants
                            .FirstOrDefault(p => p.Teamid == teamIds[i]);
                        if (tp != null)
                        {
                            tp.FinalPlace = participant.Place.Value;
                            _context.Update(tp);
                        }
                    }
                }
            }

            _context.Update(match);

            // Для elimination — вибуваємо команди що програли (виключення з турніру)
            if (tournament.Format == "elimination")
            {
                foreach (var mp in match.Matchparticipants.Where(mp => !mp.IsWinner))
                {
                    var tp = tournament.Tournamentparticipants
                        .FirstOrDefault(p => p.Teamid == mp.Teamid);
                    if (tp != null && !tp.IsEliminated)
                    {
                        tp.IsEliminated = true;
                        tp.EliminatedAt = DateTime.Now;
                        _context.Update(tp);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // POST: TournamentAdmin/EliminateTeam — ВИКЛЮЧИТИ команду з турніру назавжди
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminateTeam(int tournamentId, int teamId)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();
            if (tournament == null) return NotFound();

            var participant = tournament.Tournamentparticipants
                .FirstOrDefault(tp => tp.Teamid == teamId);

            if (participant != null)
            {
                participant.IsEliminated = true;
                participant.EliminatedAt = DateTime.Now;
                _context.Update(participant);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // POST: TournamentAdmin/RestoreTeam — відновити команду
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreTeam(int tournamentId, int teamId)
        {
            var (isOrganizer, tournament, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();
            if (tournament == null) return NotFound();

            var participant = tournament.Tournamentparticipants
                .FirstOrDefault(tp => tp.Teamid == teamId);

            if (participant != null)
            {
                participant.IsEliminated = false;
                participant.EliminatedAt = null;
                _context.Update(participant);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // POST: TournamentAdmin/MatchEliminateTeam — вибути команду тільки з матчу (не з турніру)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MatchEliminateTeam(int matchId, int tournamentId, int teamId)
        {
            var (isOrganizer, _, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();

            var mp = await _context.Matchparticipants
                .FirstOrDefaultAsync(m => m.Matchid == matchId && m.Teamid == teamId);

            if (mp != null)
            {
                mp.IsEliminated = true;
                _context.Update(mp);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // POST: TournamentAdmin/MatchRestoreTeam — відновити команду в матчі
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MatchRestoreTeam(int matchId, int tournamentId, int teamId)
        {
            var (isOrganizer, _, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();

            var mp = await _context.Matchparticipants
                .FirstOrDefaultAsync(m => m.Matchid == matchId && m.Teamid == teamId);

            if (mp != null)
            {
                mp.IsEliminated = false;
                _context.Update(mp);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // POST: TournamentAdmin/EliminatePlayer — вибути гравця з матчу
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminatePlayer(int tournamentId, int matchId, int memberId, bool eliminated)
        {
            var (isOrganizer, _, _) = await CheckOrganizerAsync(tournamentId);
            if (!isOrganizer) return Forbid();

            // Зберігаємо в TempData щоб View знав хто вибув
            // Простий підхід: зберігаємо в окремій таблиці або в notes матчу
            // Використовуємо matchparticipants.PlayerPoints як прапор для конкретного гравця
            // Зберігаємо список вибулих гравців в Notes матчу у форматі JSON-like
            var match = await _context.Tournamentmatches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match != null)
            {
                // Формат Notes: "eliminated_players:1,5,12"
                var notes = match.Notes ?? "";
                var tag = "eliminated_players:";
                var eliminatedIds = new List<string>();

                if (notes.Contains(tag))
                {
                    var start = notes.IndexOf(tag) + tag.Length;
                    var end = notes.IndexOf(";", start);
                    var idsStr = end > start ? notes.Substring(start, end - start) : notes.Substring(start);
                    eliminatedIds = idsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                if (eliminated)
                {
                    if (!eliminatedIds.Contains(memberId.ToString()))
                        eliminatedIds.Add(memberId.ToString());
                }
                else
                {
                    eliminatedIds.Remove(memberId.ToString());
                }

                // Зберігаємо назад
                var newTag = $"{tag}{string.Join(",", eliminatedIds)};";
                if (notes.Contains(tag))
                {
                    var start = notes.IndexOf(tag);
                    var end = notes.IndexOf(";", start);
                    notes = notes.Substring(0, start) + newTag + (end >= 0 && end + 1 < notes.Length ? notes.Substring(end + 1) : "");
                }
                else
                {
                    notes += newTag;
                }

                match.Notes = notes;
                _context.Update(match);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage), new { id = tournamentId });
        }

        // ===== Приватні методи =====

        private async Task GenerateRoundAsync(Tournament tournament)
        {
            int currentRound = tournament.Tournamentmatches.Any()
                ? tournament.Tournamentmatches.Max(m => m.Round) + 1
                : 1;

            var activeTeams = tournament.Tournamentparticipants
                .Where(tp => !tp.IsEliminated)
                .ToList();

            if (tournament.Format == "elimination")
            {
                if (currentRound == 1)
                    activeTeams = activeTeams.OrderBy(_ => Guid.NewGuid()).ToList();

                for (int i = 0; i + 1 < activeTeams.Count; i += 2)
                {
                    var match = new Tournamentmatch
                    {
                        Tournamentid = tournament.Id,
                        Round = currentRound,
                        Status = "planned"
                    };
                    _context.Tournamentmatches.Add(match);
                    await _context.SaveChangesAsync();

                    _context.Matchparticipants.Add(new Matchparticipant { Matchid = match.Id, Teamid = activeTeams[i].Teamid });
                    _context.Matchparticipants.Add(new Matchparticipant { Matchid = match.Id, Teamid = activeTeams[i + 1].Teamid });
                }

                if (activeTeams.Count % 2 == 1)
                    TempData["Info"] = $"Команда «{activeTeams.Last().Team?.Name}» отримує bye (проходить без матчу)";
            }
            else if (tournament.Format == "roundrobin")
            {
                for (int i = 0; i < activeTeams.Count; i++)
                {
                    for (int j = i + 1; j < activeTeams.Count; j++)
                    {
                        var match = new Tournamentmatch
                        {
                            Tournamentid = tournament.Id,
                            Round = currentRound,
                            Status = "planned"
                        };
                        _context.Tournamentmatches.Add(match);
                        await _context.SaveChangesAsync();

                        _context.Matchparticipants.Add(new Matchparticipant { Matchid = match.Id, Teamid = activeTeams[i].Teamid });
                        _context.Matchparticipants.Add(new Matchparticipant { Matchid = match.Id, Teamid = activeTeams[j].Teamid });
                    }
                }
            }
            else if (tournament.Format == "battleroyal")
            {
                var match = new Tournamentmatch
                {
                    Tournamentid = tournament.Id,
                    Round = currentRound,
                    Status = "planned"
                };
                _context.Tournamentmatches.Add(match);
                await _context.SaveChangesAsync();

                foreach (var team in activeTeams)
                    _context.Matchparticipants.Add(new Matchparticipant { Matchid = match.Id, Teamid = team.Teamid });
            }

            await _context.SaveChangesAsync();
        }

        // Пункт 4: для battleroyal місця вже збережені в FinishMatch
        // Для інших форматів — розраховуємо по вибуттю
        private async Task AssignFinalPlacesAsync(Tournament tournament)
        {
            if (tournament.Format == "battleroyal")
            {
                // Для battleroyal місця вже виставлені в FinishMatch
                // Якщо хтось не має місця — ставимо в кінець
                int lastPlace = tournament.Tournamentparticipants.Count;
                foreach (var tp in tournament.Tournamentparticipants.Where(p => !p.FinalPlace.HasValue))
                {
                    tp.FinalPlace = lastPlace--;
                    _context.Update(tp);
                }
            }
            else
            {
                // Для elimination і roundrobin
                int place = tournament.Tournamentparticipants.Count;
                foreach (var tp in tournament.Tournamentparticipants
                    .Where(p => p.IsEliminated)
                    .OrderBy(p => p.EliminatedAt))
                {
                    tp.FinalPlace = place--;
                    _context.Update(tp);
                }

                int activePlace = 1;
                foreach (var tp in tournament.Tournamentparticipants.Where(p => !p.IsEliminated))
                {
                    tp.FinalPlace = activePlace++;
                    _context.Update(tp);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
