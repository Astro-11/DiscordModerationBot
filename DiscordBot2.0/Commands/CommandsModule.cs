using DiscordBot;
using DiscordBot2._0;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MyFirstBot
{
    public class MyFirstModule : BaseCommandModule
    {
        static List<User> userList;
        static string[] names;
        static string[] iDs;
        static string[] textMutes;
        static string[] voiceMutes;
        static string[] inGameOffence;
        static string[] disciplinaryOffence;
        static string[] lastTextMuteRemovalDate;
        static string[] lastVoiceMuteRemovalDate;
        static string[] lastInGameOffenceRemovalDate;
        static string[] lastDisciplinaryOffenceRemovalDate;
        static DateTime[] lastOffenceDate;

        static DiscordRole disciplinaryWarn = null;
        static DiscordRole disciplinaryWarn2 = null;
        static DiscordRole inGameWarn = null;
        static DiscordRole inGameWarn2 = null;
        static DiscordRole inGameWarn3 = null;
        static DiscordRole textMute = null;
        static DiscordRole voiceMute = null;

        public static void RefreshSpreadsheet()
        {
            names = Spreadsheet.ReadEntry("A", "A");
            iDs = Spreadsheet.ReadEntry("B", "B");
            textMutes = Spreadsheet.ReadEntry("C", "C");
            voiceMutes = Spreadsheet.ReadEntry("D", "D");
            inGameOffence = Spreadsheet.ReadEntry("E", "E");
            disciplinaryOffence = Spreadsheet.ReadEntry("F", "F");
            lastTextMuteRemovalDate = Spreadsheet.ReadEntry("I", "I");
            lastVoiceMuteRemovalDate = Spreadsheet.ReadEntry("J", "J");
            lastInGameOffenceRemovalDate = Spreadsheet.ReadEntry("K", "K");
            lastDisciplinaryOffenceRemovalDate = Spreadsheet.ReadEntry("L", "L");
            List<DateTime> dateList = new List<DateTime>();

            foreach (string dateString in Spreadsheet.ReadEntry("G", "G"))
            {
                if (dateString != "Data ultima punizione" && dateString != "00/00/00" && dateString != "/")
                {
                    dateList.Add(DateTime.ParseExact(dateString, "dd/MM/yyyy", null));
                }
                else
                {
                    dateList.Add(DateTime.Parse("01/01/2001"));
                }
            }

            lastOffenceDate = dateList.ToArray();
            CreateUsers();
        }

        static void FindRoles(CommandContext ctx)
        {
            disciplinaryWarn = ctx.Guild.GetRole(828573087728795679);
            disciplinaryWarn2 = ctx.Guild.GetRole(828573116389261312);
            inGameWarn = ctx.Guild.GetRole(596612543598952460);
            inGameWarn2 = ctx.Guild.GetRole(828573042426380299);
            inGameWarn3 = ctx.Guild.GetRole(905845233739235471);
            textMute = ctx.Guild.GetRole(698653530625540178);
            voiceMute = ctx.Guild.GetRole(672109597884022794);
        }

        static int FindUserIndex(DiscordUser offendingMember)
        {
            int index = Array.IndexOf(names, offendingMember.Username);
            if (index == -1) index = Array.IndexOf(iDs, offendingMember.Discriminator);
            return index;
        }

        static void CreateUsers()
        {
            userList = new List<User>();

            foreach (string username in names)
            {
                int index = Array.IndexOf(names, username);
                User newUser = new User();
                newUser.name = names[index];
                newUser.iD = iDs[index];
                newUser.textMute = toNumber(textMutes[index]);
                newUser.voiceMute = toNumber(voiceMutes[index]);
                newUser.inGameOffence = toNumber(inGameOffence[index]);
                newUser.disciplinaryOffence = toNumber(disciplinaryOffence[index]);
                newUser.lastTextMuteRemovalDate = lastTextMuteRemovalDate[index];
                newUser.lastVoiceMuteRemovalDate = lastVoiceMuteRemovalDate[index];
                newUser.lastInGameOffenceRemovalDate = lastInGameOffenceRemovalDate[index];
                newUser.lastDisciplinaryOffenceRemovalDate = lastDisciplinaryOffenceRemovalDate[index];
                newUser.lastOffenceDate = lastOffenceDate[index];
                newUser.userIndex = index + 1;
                userList.Add(newUser);
            }

            int toNumber(string offence)
            {
                int n = 0;
                foreach (char c in offence) if (c == 'X') n++;
                return n;
            }
        }

        [Command("status"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task StatusCommand(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            await ctx.RespondAsync("Nome: " + userList[index].name +
                                   "\nID: " + userList[index].iD +
                                   "\nMute testuali: " + userList[index].textMute +
                                   "\nMute vocali: " + userList[index].voiceMute +
                                   "\nInfrazioni in-game: " + userList[index].inGameOffence +
                                   "\nInfrazioni disciplinari: " + userList[index].disciplinaryOffence);
        }

        [Command("add"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddUser(CommandContext ctx, DiscordMember offendingMember)
        {
            Spreadsheet.CreateUser(offendingMember.Username, offendingMember.Discriminator);
            RefreshSpreadsheet();
            await ctx.RespondAsync("Utente aggiunto alla NaughtyList");
        }

        [Command("edit"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task EditUser(CommandContext ctx, string offendingMember, string newUsername, string newID)
        {
            int index = Array.IndexOf(names, offendingMember);
            if (index == -1) index = Array.IndexOf(iDs, offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            Spreadsheet.UpdateEntry("A", $"{index + 1}", newUsername);
            Spreadsheet.UpdateEntry("B", $"{index + 1}", newID);
            RefreshSpreadsheet();
            await ctx.RespondAsync("Username e/o ID modificati");
        }

        [Command("delete"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task DeleteUser(CommandContext ctx, string offendingMember)
        {
            int index = Array.IndexOf(names, offendingMember);
            if (index == -1) index = Array.IndexOf(iDs, offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            Spreadsheet.DeleteEntry($"A{index + 1}", $"L{index + 1}");
            RefreshSpreadsheet();
            await ctx.RespondAsync("Utente rimosso dalla NaughtyList");
        }

        [Command("disciplinare"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddDisciplinaryOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            string moderatorName = ctx.Member.Username;
            if (userList[index].AddOffence("disciplinaryOffence", moderatorName) == false)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di warn");
                return;
            }

            if (disciplinaryWarn == null) FindRoles(ctx);
            if (offendingMember.Roles.Any(x => x == disciplinaryWarn))
            {
                await offendingMember.GrantRoleAsync(disciplinaryWarn2);
                await offendingMember.RevokeRoleAsync(disciplinaryWarn);
            }
            else await offendingMember.GrantRoleAsync(disciplinaryWarn);

            await ctx.RespondAsync("Warn disciplinare aggiunto");
        }

        [Command("ingame"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddInGameOffenceOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            string moderatorName = ctx.Member.Username;
            if (userList[index].AddOffence("inGameOffence", moderatorName) == false)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di warn");
                return;
            }

            if (inGameWarn == null) FindRoles(ctx);
            if (offendingMember.Roles.Any(x => x == inGameWarn))
            {
                await offendingMember.GrantRoleAsync(inGameWarn2);
                await offendingMember.RevokeRoleAsync(inGameWarn);
                await ctx.RespondAsync("Warn in-game aggiunto, l'utente deve ricevere un gameban di due settimane");
            }
            else if (offendingMember.Roles.Any(x => x == inGameWarn2))
            {
                await offendingMember.GrantRoleAsync(inGameWarn3);
                await offendingMember.RevokeRoleAsync(inGameWarn2);
                await ctx.RespondAsync("Warn in-game aggiunto, l'utente deve ricevere un gameban di due mesi");
            }
            else if (offendingMember.Roles.Any(x => x == inGameWarn3))
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero di warn in-game e deve ricevere un gameban permanente");
            }
            else
            {
                await offendingMember.GrantRoleAsync(inGameWarn);
                await ctx.RespondAsync("Warn in-game aggiunto");
            }
        }

        [Command("testuale"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddTextMuteOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            string moderatorName = ctx.Member.Username;
            if (userList[index].AddOffence("textMute", moderatorName) == false)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di warn");
                return;
            }

            if (textMute == null) FindRoles(ctx);

            switch (userList[index].textMute)
            {
                case 1:
                    await ctx.RespondAsync("Mute testuale aggiunto, l'utente deve essere mutato per 1 giorno");
                    break;
                case 2:
                    await ctx.RespondAsync("Mute testuale aggiunto, l'utente deve essere mutato per una settimana");
                    break;
                case 3:
                    await ctx.RespondAsync("Mute testuale aggiunto, l'utente deve essere mutato per un mese");
                    break;
                case 4:
                    await ctx.RespondAsync("Mute testuale aggiunto, l'utente deve essere mutato permanentemente");
                    break;
            }
        }

        [Command("vocale"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddVoiceMuteOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            string moderatorName = ctx.Member.Username;
            if (userList[index].AddOffence("voiceMute", moderatorName) == false)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di warn");
                return;
            }

            if (voiceMute == null) FindRoles(ctx);

            switch (userList[index].voiceMute)
            {
                case 1:
                    await ctx.RespondAsync("Mute vocale aggiunto, l'utente deve essere mutato per 1 giorno");
                    break;
                case 2:
                    userList[index].AddOffence("disciplinaryOffence", moderatorName);
                    await ctx.RespondAsync("Mute vocale aggiunto, l'utente deve essere mutato per una settimana. All'utente è stato inoltre aggiunto un warn disciplinare");
                    break;
                case 3:
                    await ctx.RespondAsync("Mute vocale aggiunto, l'utente dovrebbe essere bannato");
                    break;
            }
        }

        [Command("remove"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task RemoveOffence(CommandContext ctx, DiscordMember offendingMember, string offence)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            string moderatorName = ctx.Member.Username;
            if (userList[index].RemoveOffence(offence, moderatorName) == false)
            {
                await ctx.RespondAsync("L'utente non ha infrazioni da rimuovere");
                return;
            }

            switch (offence)
            {
                case "testuale":
                    if (textMute == null) FindRoles(ctx);
                    //Filler
                    break;
                case "vocale":
                    if (voiceMute == null) FindRoles(ctx);
                    //Filler
                    break;
                case "ingame":
                    if (inGameWarn == null) FindRoles(ctx);
                    if (offendingMember.Roles.Any(x => x == inGameWarn2))
                    {
                        await offendingMember.RevokeRoleAsync(inGameWarn2);
                        await offendingMember.GrantRoleAsync(inGameWarn);
                    }
                    else if (offendingMember.Roles.Any(x => x == inGameWarn3))
                    {
                        await offendingMember.RevokeRoleAsync(inGameWarn3);
                        await offendingMember.GrantRoleAsync(inGameWarn2);
                    }
                    else
                    await offendingMember.RevokeRoleAsync(inGameWarn);
                    break;
                case "disciplinare":
                    if (disciplinaryWarn == null) FindRoles(ctx);
                    if (offendingMember.Roles.Any(x => x == disciplinaryWarn))
                    {
                        await offendingMember.RevokeRoleAsync(disciplinaryWarn2);
                        await offendingMember.GrantRoleAsync(disciplinaryWarn);
                    }
                    else await offendingMember.RevokeRoleAsync(disciplinaryWarn);
                    break;
                default:
                    await ctx.RespondAsync("Tipo di infrazione non valido");
                    return;
            }

            await ctx.RespondAsync($"{offence} rimosso");
        }

        [Command("scaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task checkExpiredOffences(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            int[] expiredOffences = userList[index].CheckExpiredOffences();
            if (expiredOffences.All(o => o == 0)) await ctx.RespondAsync($"{userList[index].name}#{userList[index].iD} non ha infrazioni scadute");
            else await ctx.RespondAsync($"Infrazioni scadute per {userList[index].name}#{userList[index].iD}: {expiredOffences[0]} mute testuali, {expiredOffences[1]} mute vocali, {expiredOffences[2]} warn in-game, {expiredOffences[3]} warn disciplinari");
        }

        [Command("scaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task checkExpiredOffences(CommandContext ctx)
        {
            string str = "Infrazioni scadute per utente:\n";

            foreach (User user in userList)
            {
                if (user.name == "Nome" || user.name == "Naughty boy") continue;
                int[] expiredOffences = user.CheckExpiredOffences();
                if (!expiredOffences.All(o => o == 0)) str += ($"{user.name}#{user.iD}: {expiredOffences[0]} mute testuali, {expiredOffences[1]} mute vocali, {expiredOffences[2]} warn in-game, {expiredOffences[3]} warn disciplinari\n");
            }

            if (str == "Infrazioni scadute per utente:\n") str = "Non ci sono utenti con infrazioni scadute";
            await ctx.RespondAsync(str);
        }

        [Command("rimuoviScaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task removeExpiredOffences(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            string moderatorName = ctx.Member.Username;
            int[] expiredOffences = userList[index].CheckExpiredOffences();

            if (expiredOffences.All(o => o == 0))
            {
                await ctx.RespondAsync($"{userList[index].name}#{userList[index].iD} non ha infrazioni scadute");
                return;
            }

            await ctx.RespondAsync($"Sono stati rimossi {expiredOffences[0]} mute testuali, {expiredOffences[1]} mute vocali, {expiredOffences[2]} warn in-game, {expiredOffences[3]} warn disciplinari per {userList[index].name}#{userList[index].iD}");
            userList[index].RemoveExpiredOffences(moderatorName);

            if (inGameWarn == null || disciplinaryOffence == null) FindRoles(ctx);
            await FixRoles(offendingMember, userList[index]);
        }

        [Command("rimuoviScaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task removeExpiredOffences(CommandContext ctx)
        {
            string moderatorName = ctx.Member.Username;
            var membersList = await ctx.Guild.GetAllMembersAsync();
            int[] totalNumberOfRemovedOffences = new int[4];

            foreach (User user in userList)
            {
                if (user.name == "Nome" || user.name == "Naughty boy") continue;
                int[] expiredOffences = user.CheckExpiredOffences();

                if (expiredOffences.All(o => o == 0)) continue;
                else
                {
                    totalNumberOfRemovedOffences = Enumerable.Zip(totalNumberOfRemovedOffences, expiredOffences, (x, y) => x + y).ToArray();
                    user.RemoveExpiredOffences(moderatorName);

                    if (inGameWarn == null || disciplinaryOffence == null) FindRoles(ctx);
                    DiscordMember searchedMember = membersList.FirstOrDefault(member => member.DisplayName == user.name);
                    if (searchedMember == null) searchedMember = membersList.FirstOrDefault(member => member.Discriminator == user.iD);
                    else if (searchedMember == null) continue;

                    await FixRoles(searchedMember, user);
                }
            }

            await ctx.RespondAsync($"In totale sono stati rimossi: {totalNumberOfRemovedOffences[0]} mute testuali, {totalNumberOfRemovedOffences[1]} mute vocali, {totalNumberOfRemovedOffences[2]} warn in-game, {totalNumberOfRemovedOffences[3]} warn disciplinari");
        }

        [Command("sistemaRuoli"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task fixRolesCommand(CommandContext ctx, DiscordMember offendingMember)
        {
            int index = FindUserIndex(offendingMember);
            if (index == -1)
            {
                await ctx.RespondAsync("Utente non trovato");
                return;
            }

            var membersList = await ctx.Guild.GetAllMembersAsync();
            int fixedRolesNumber = 0;

            DiscordMember searchedMember = membersList.FirstOrDefault(member => member == offendingMember);
            if (searchedMember == null)
            {
                await ctx.RespondAsync("Utente non trovato nella NaughtyList");
                return;
            }

            int[] userOffences = userList[index].GetOffences();
            if (inGameWarn == null || disciplinaryOffence == null) FindRoles(ctx);
            await removeWarnRoles(searchedMember);

            if (userOffences[2] == 0 && userOffences[3] == 0)
            {
                await ctx.RespondAsync("L'utente non ha ruoli da sistemare");
            }
            else
            {
                if (userOffences[2] == 1) await searchedMember.GrantRoleAsync(inGameWarn);
                if (userOffences[2] == 2) await searchedMember.GrantRoleAsync(inGameWarn2);
                if (userOffences[2] == 3) await searchedMember.GrantRoleAsync(inGameWarn3);
                if (userOffences[3] == 1) await searchedMember.GrantRoleAsync(disciplinaryWarn);
                if (userOffences[3] == 2) await searchedMember.GrantRoleAsync(disciplinaryWarn2);
            }

            await ctx.RespondAsync("Ruoli sistemati");
        }

        [Command("sistemaRuoli"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task fixRoleCommand(CommandContext ctx)
        {
            var membersList = await ctx.Guild.GetAllMembersAsync();
            int fixedRolesNumber = 0;

            foreach (User user in userList)
            {
                DiscordMember searchedMember = membersList.FirstOrDefault(member => member.DisplayName == user.name);
                if (searchedMember == null) searchedMember = membersList.FirstOrDefault(member => member.Discriminator == user.iD);
                if (searchedMember == null) continue;

                int[] userOffences = user.GetOffences();
                await removeWarnRoles(searchedMember);

                if (userOffences[2] == 0 && userOffences[3] == 0) continue;
                else
                {
                    fixedRolesNumber++;
                    if (userOffences[2] == 1) await searchedMember.GrantRoleAsync(inGameWarn);
                    if (userOffences[2] == 2) await searchedMember.GrantRoleAsync(inGameWarn2);
                    if (userOffences[2] == 3) await searchedMember.GrantRoleAsync(inGameWarn3);
                    if (userOffences[3] == 1) await searchedMember.GrantRoleAsync(disciplinaryWarn);
                    if (userOffences[3] == 2) await searchedMember.GrantRoleAsync(disciplinaryWarn2);
                }
            }

            if (fixedRolesNumber == 0) await ctx.RespondAsync("Non sono stati trovati utenti con ruoli da sistemare");
            else await ctx.RespondAsync($"In totale sono stati sistemati i ruoli di {fixedRolesNumber} utenti");
        }

        public async Task FixRoles(DiscordMember targetMember, User targetUser)
        {
            int[] userOffences = targetUser.GetOffences();
            await removeWarnRoles(targetMember);

            if (userOffences[2] == 1) await targetMember.GrantRoleAsync(inGameWarn);
            if (userOffences[2] == 2) await targetMember.GrantRoleAsync(inGameWarn2);
            if (userOffences[2] == 3) await targetMember.GrantRoleAsync(inGameWarn3);
            if (userOffences[3] == 1) await targetMember.GrantRoleAsync(disciplinaryWarn);
            if (userOffences[3] == 2) await targetMember.GrantRoleAsync(disciplinaryWarn2);
        }

        public async Task removeWarnRoles(DiscordMember targetMember)
        {
            await targetMember.RevokeRoleAsync(inGameWarn);
            await targetMember.RevokeRoleAsync(inGameWarn2);
            await targetMember.RevokeRoleAsync(inGameWarn2);
            await targetMember.RevokeRoleAsync(disciplinaryWarn);
            await targetMember.RevokeRoleAsync(disciplinaryWarn2);
        }
    }
}
