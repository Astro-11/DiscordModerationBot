using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;

namespace DiscordBot2._0
{
    public class CommandModule : BaseCommandModule
    {
        private Database db = new SpreadsheetDatabase();
        private List<User> dbUsersList = new List<User>();
        static private DiscordRole textMute = null;
        static private DiscordRole voiceMute = null;
        static private Dictionary<int, DiscordRole> inGameWarnRoles = null;
        static private Dictionary<int, DiscordRole> disciplinaryWarnRoles = null;
        private bool rolesInitialized = false;

        private void FindRoles(CommandContext ctx)
        {
            rolesInitialized = true;
            textMute = ctx.Guild.GetRole(698653530625540178);
            voiceMute = ctx.Guild.GetRole(672109597884022794);

            DiscordRole disciplinaryWarn = ctx.Guild.GetRole(828573087728795679);
            DiscordRole disciplinaryWarn2 = ctx.Guild.GetRole(828573116389261312);
            DiscordRole inGameWarn = ctx.Guild.GetRole(596612543598952460);
            DiscordRole inGameWarn2 = ctx.Guild.GetRole(828573042426380299);
            DiscordRole inGameWarn3 = ctx.Guild.GetRole(905845233739235471);

            inGameWarnRoles = new Dictionary<int, DiscordRole>() { { 0, null }, { 1, inGameWarn }, { 2, inGameWarn2 }, {3, inGameWarn3 }, { 4, null } };
            disciplinaryWarnRoles = new Dictionary<int, DiscordRole>() { { 0, null }, { 1, disciplinaryWarn }, { 2, disciplinaryWarn2 }, { 3, null } };
        }

        private User FindDiscordUserInDatabase(DiscordUser discordUser)
        {
            Console.WriteLine("ID: " + discordUser.Id);
            return db.getUser(discordUser.Id);
        }

        private List<User> GetAllUsers(CommandContext ctx)
        {
            if (dbUsersList.Count == 0) dbUsersList = new List<User>(db.getUsers().Where(user => ctx.Guild.Members.ContainsKey((ulong)user.id)));
            return dbUsersList;
        }


        [Command("status"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task StatusCommand(CommandContext ctx, DiscordMember offendingMember)
        {
            Console.WriteLine("Comando chiamato");
            User user = FindDiscordUserInDatabase(offendingMember);
            Console.WriteLine("Utente trovato: " + user.name);

            await ctx.RespondAsync("Nome: " + user.name +
                                   "\nID: " + user.id +
                                   "\nMute testuali: " + user.offencesRecord.textMute.getOffenceLevel() +
                                   "\nMute vocali: " + user.offencesRecord.voiceMute.getOffenceLevel() +
                                   "\nInfrazioni in-game: " + user.offencesRecord.inGameOffence.getOffenceLevel() +
                                   "\nInfrazioni disciplinari: " + user.offencesRecord.disciplinaryOffence.getOffenceLevel());
        }

        [Command("add"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddUser(CommandContext ctx, DiscordMember offendingMember)
        {
            db.addUser(offendingMember.Username, offendingMember.Id);
            await ctx.RespondAsync("Utente aggiunto alla NaughtyList");
        }

        [Command("edit"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task EditUser(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            user.name = offendingMember.Username;
            db.refreshUser(user);
            await ctx.RespondAsync("Username modificato");
        }

        [Command("delete"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task DeleteUser(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            db.deleteUser(user);
            await ctx.RespondAsync("Utente rimosso dalla NaughtyList");
        }

        [Command("disciplinare"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddDisciplinaryOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            string moderatorName = ctx.Member.Username;

            try
            {
                user.offencesRecord.disciplinaryOffence.increaseOffence();
            }
            catch (OffenceLevelOverTheCapException)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di warn");
                return;
            }

            db.commitUserOffencesChanges(user, user.offencesRecord.disciplinaryOffence, moderatorName);

            try
            {
                if (!rolesInitialized) FindRoles(ctx);
                if (user.offencesRecord.inGameOffence.getOffenceLevel() > 1)
                {
                    DiscordRole oldWarnRole = offendingMember.Roles.First(role => disciplinaryWarnRoles.ContainsValue(role));
                    await offendingMember.RevokeRoleAsync(oldWarnRole);
                }
                await offendingMember.GrantRoleAsync(disciplinaryWarnRoles[user.offencesRecord.disciplinaryOffence.getOffenceLevel()]);
            }
            catch (Exception)
            {
                await ctx.RespondAsync("Errore nell'assegnamento dei ruoli");
                return;
            }

            await ctx.RespondAsync("Warn disciplinare aggiunto");
        }

        [Command("ingame"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddInGameOffenceOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            string moderatorName = ctx.Member.Username;

            try
            {
                user.offencesRecord.inGameOffence.increaseOffence();
            }
            catch (OffenceLevelOverTheCapException)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di warn");
                return;
            }

            db.commitUserOffencesChanges(user, user.offencesRecord.inGameOffence, moderatorName);

            try
            {
                if (!rolesInitialized) FindRoles(ctx);
                if (user.offencesRecord.inGameOffence.getOffenceLevel() > 1)
                {
                    DiscordRole oldWarnRole = offendingMember.Roles.First(role => inGameWarnRoles.ContainsValue(role));
                    await offendingMember.RevokeRoleAsync(oldWarnRole);
                }
                await offendingMember.GrantRoleAsync(inGameWarnRoles[user.offencesRecord.inGameOffence.getOffenceLevel()]);
            }
            catch (Exception)
            {
                await ctx.RespondAsync("Errore nell'assegnamento dei ruoli");
                return;
            }

            string answer = "Warn in-game aggiunto";
            switch (user.offencesRecord.inGameOffence.getOffenceLevel())
            {
                case 2:
                    answer += ", l'utente deve ricevere un gameban di due settimane";
                    break;
                case 3:
                    answer += ", l'utente deve ricevere un gameban di due mesi";
                    break;
                case 4:
                    answer += ", l'utente deve ricevere un gameban permanente";
                    break;
            }
            await ctx.RespondAsync(answer);
        }

        [Command("testuale"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task AddTextMuteOffence(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            string moderatorName = ctx.Member.Username;

            try
            {
                user.offencesRecord.textMute.increaseOffence();
            }
            catch (OffenceLevelOverTheCapException)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di mute testuali");
                return;
            }

            db.commitUserOffencesChanges(user, user.offencesRecord.textMute, moderatorName);

            switch (user.offencesRecord.textMute.getOffenceLevel())
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
            User user = FindDiscordUserInDatabase(offendingMember);
            string moderatorName = ctx.Member.Username;

            try
            {
                user.offencesRecord.voiceMute.increaseOffence();
            }
            catch (OffenceLevelOverTheCapException)
            {
                await ctx.RespondAsync("L'utente ha raggiunto il numero massimo di mute vocali");
                return;
            }

            db.commitUserOffencesChanges(user, user.offencesRecord.voiceMute, moderatorName);

            switch (user.offencesRecord.voiceMute.getOffenceLevel())
            {
                case 1:
                    await ctx.RespondAsync("Mute vocale aggiunto, l'utente deve essere mutato per 1 giorno");
                    break;
                case 2:
                    user.offencesRecord.disciplinaryOffence.increaseOffence();
                    db.commitUserOffencesChanges(user, user.offencesRecord.disciplinaryOffence, moderatorName);
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
            User user = FindDiscordUserInDatabase(offendingMember);
            OffenceInterface offenceToDecrease = null;
            string moderatorName = ctx.Member.Username;
            Dictionary<int, DiscordRole> warnRolesDictionaryToUse = null;
            if (rolesInitialized == false) FindRoles(ctx);

            switch (offence)
            {
                case "testuale":
                    offenceToDecrease = user.offencesRecord.textMute;
                    break;
                case "vocale":
                    offenceToDecrease = user.offencesRecord.voiceMute;
                    break;
                case "ingame":
                    offenceToDecrease = user.offencesRecord.inGameOffence;
                    warnRolesDictionaryToUse = inGameWarnRoles;
                    break;
                case "disciplinare":
                    offenceToDecrease = user.offencesRecord.disciplinaryOffence;
                    warnRolesDictionaryToUse = disciplinaryWarnRoles;
                    break;
                default:
                    await ctx.RespondAsync("Tipo di infrazione non valido");
                    return;
            }

            try
            {
                offenceToDecrease.decreaseOffence();
            }
            catch (OffenceLevelZeroException)
            {
                await ctx.RespondAsync("L'utente non ha infrazioni da rimuovere");
                return;
            }

            db.commitUserOffencesChanges(user, moderatorName);

            if (offenceToDecrease == user.offencesRecord.disciplinaryOffence || offenceToDecrease == user.offencesRecord.inGameOffence)
            {
                try
                {
                    if (!rolesInitialized) FindRoles(ctx);
                    DiscordRole oldWarnRole = offendingMember.Roles.First(role => warnRolesDictionaryToUse.ContainsValue(role));
                    await offendingMember.RevokeRoleAsync(oldWarnRole);
                    if (offenceToDecrease.getOffenceLevel() != 0) await offendingMember.GrantRoleAsync(warnRolesDictionaryToUse[offenceToDecrease.getOffenceLevel()]);
                }
                catch (Exception)
                {
                    await ctx.RespondAsync("Errore nell'assegnamento dei ruoli");
                    return;
                }
            }

            await ctx.RespondAsync($"{offenceToDecrease.getOffenceName()} rimosso");
        }

        [Command("scaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task checkExpiredOffences(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            Console.WriteLine("User found: " + user.name);
            Dictionary<OffenceInterface, int> numberOfDecayedLevels = user.offencesRecord.getOffencesNumberOfDecayedLevels();
            Console.WriteLine("2");

            if (numberOfDecayedLevels.All(o => o.Value == 0)) await ctx.RespondAsync($"{user.name}#{user.id} non ha infrazioni scadute");
            else await ctx.RespondAsync($"Infrazioni scadute per {user.name}#{user.id}: " +
                $"{numberOfDecayedLevels[user.offencesRecord.textMute]} mute testuali, " +
                $"{numberOfDecayedLevels[user.offencesRecord.voiceMute]} mute vocali, " +
                $"{numberOfDecayedLevels[user.offencesRecord.inGameOffence]} warn in-game, " +
                $"{numberOfDecayedLevels[user.offencesRecord.disciplinaryOffence]} warn disciplinari");
        }

        [Command("scaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task checkExpiredOffences(CommandContext ctx)
        {
            string str = "Infrazioni scadute per utente:";

            foreach (User user in GetAllUsers(ctx))
            {
                Dictionary<OffenceInterface, int> numberOfDecayedLevels = user.offencesRecord.getOffencesNumberOfDecayedLevels();

                if (!numberOfDecayedLevels.All(o => o.Value == 0))
                    str += $"\n{user.name}#{user.id}: " +
                           $"{numberOfDecayedLevels[user.offencesRecord.textMute]} mute testuali, " +
                           $"{numberOfDecayedLevels[user.offencesRecord.voiceMute]} mute vocali, " +
                           $"{numberOfDecayedLevels[user.offencesRecord.inGameOffence]} warn in-game, " +
                           $"{numberOfDecayedLevels[user.offencesRecord.disciplinaryOffence]} warn disciplinari";
            }

            if (str == "Infrazioni scadute per utente:") str = "Non ci sono utenti con infrazioni scadute";
            await ctx.RespondAsync(str);
        }

        [Command("rimuoviScaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task removeExpiredOffences(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            Dictionary<OffenceInterface, int> numberOfDecayedLevels = user.offencesRecord.getOffencesNumberOfDecayedLevels();
            string moderatorName = ctx.Member.Username;

            if (numberOfDecayedLevels.All(o => o.Value == 0))
            {
                await ctx.RespondAsync($"{user.name}#{user.id} non ha infrazioni scadute");
                return;
            }

            user.offencesRecord.removeExpiredOffences();
            db.commitUserOffencesChanges(user, moderatorName);

            await ctx.RespondAsync($"Sono stati rimossi " +
                $"{numberOfDecayedLevels[user.offencesRecord.textMute]} mute testuali, " +
                $"{numberOfDecayedLevels[user.offencesRecord.voiceMute]} mute vocali, " +
                $"{numberOfDecayedLevels[user.offencesRecord.inGameOffence]} warn in-game, " +
                $"{numberOfDecayedLevels[user.offencesRecord.disciplinaryOffence]} warn disciplinari " +
                $"per {user.name}#{user.id}");

            if (rolesInitialized == false) FindRoles(ctx);
            await FixRoles(offendingMember, user);
        }

        [Command("rimuoviScaduti"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task removeExpiredOffences(CommandContext ctx)
        {
            string moderatorName = ctx.Member.Username;
            int[] totalNumberOfRemovedOffences = new int[4];

            foreach (User user in GetAllUsers(ctx))
            {
                Dictionary<OffenceInterface, int> numberOfDecayedLevels = user.offencesRecord.getOffencesNumberOfDecayedLevels();
                if (numberOfDecayedLevels.All(o => o.Value == 0)) continue;

                totalNumberOfRemovedOffences = Enumerable.Zip(totalNumberOfRemovedOffences, numberOfDecayedLevels.Values, (x, y) => x + y).ToArray();
                user.offencesRecord.removeExpiredOffences();
                db.commitUserOffencesChanges(user, moderatorName);

                if (rolesInitialized == false) FindRoles(ctx);
                await FixRoles(ctx.Guild.Members[(ulong)user.id], user);
            }

            await ctx.RespondAsync($"In totale sono stati rimossi: {totalNumberOfRemovedOffences[0]} mute testuali, {totalNumberOfRemovedOffences[1]} mute vocali, {totalNumberOfRemovedOffences[2]} warn in-game, {totalNumberOfRemovedOffences[3]} warn disciplinari");
        }

        [Command("sistemaRuoli"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task fixRolesCommand(CommandContext ctx, DiscordMember offendingMember)
        {
            User user = FindDiscordUserInDatabase(offendingMember);
            Console.WriteLine("Utente trovato");
            if (rolesInitialized == false) FindRoles(ctx);
            Console.WriteLine("Ruoli inizializzati");

            if (!CheckForUnwantedRoles(offendingMember, user))
            {
                await ctx.RespondAsync("L'utente non ha ruoli da sistemare");
                return;
            }
            else
            {
                Console.WriteLine("Ruoli da sistemare trovati");
                await FixRoles(ctx.Guild.Members[(ulong)user.id], user);
                Console.WriteLine("Ruoli sistemati");
                await ctx.RespondAsync("Ruoli sistemati");
            }
        }

        [Command("sistemaRuoli"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task fixRoleCommand(CommandContext ctx)
        {
            Console.WriteLine("Comando chiamato");
            if (rolesInitialized == false) FindRoles(ctx);
            var membersList = await ctx.Guild.GetAllMembersAsync();
            Console.WriteLine("Membri trovati");
            int fixedRolesNumber = 0;

            foreach (User user in GetAllUsers(ctx))
            {
                DiscordMember offendingMember = ctx.Guild.Members[(ulong)user.id];
                Console.WriteLine("Membro singolo trovato: " + offendingMember.DisplayName + " " + user.name);
                if (!CheckForUnwantedRoles(offendingMember, user)) continue;
                else
                {
                    Console.WriteLine("Richiesta sistemazione ruoli per membro singolo");
                    await FixRoles(offendingMember, user);
                    Console.WriteLine("Ruoli sistemati per membro singolo");
                    fixedRolesNumber++;
                }
            }

            if (fixedRolesNumber == 0) await ctx.RespondAsync("Non sono stati trovati utenti con ruoli da sistemare");
            else await ctx.RespondAsync($"In totale sono stati sistemati i ruoli di {fixedRolesNumber} utenti");
        }

        public async Task FixRoles(DiscordMember targetMember, User targetUser)
        {
            if (!CheckForUnwantedRoles(targetMember, targetUser)) return;

            List<DiscordRole> expectedRoles = new List<DiscordRole>() { inGameWarnRoles[targetUser.offencesRecord.inGameOffence.getOffenceLevel()], disciplinaryWarnRoles[targetUser.offencesRecord.disciplinaryOffence.getOffenceLevel()] };
            expectedRoles.RemoveAll(role => role == null);
            List<DiscordRole> targetMemberWarnRoles = targetMember.Roles.Where(role => inGameWarnRoles.ContainsValue(role) || disciplinaryWarnRoles.ContainsValue(role)).ToList();
            Console.WriteLine("Expected roles: ");
            expectedRoles.ForEach(o => Console.WriteLine(o));
            Console.WriteLine("Target member warn roles: ");
            targetMemberWarnRoles.ForEach(o => Console.WriteLine(o));
            foreach (DiscordRole unwatedRole in targetMemberWarnRoles.Except(expectedRoles)) { await targetMember.RevokeRoleAsync(unwatedRole); }
            Console.WriteLine("2");
            foreach (DiscordRole wantedRole in expectedRoles.Except(targetMemberWarnRoles)) { await targetMember.GrantRoleAsync(wantedRole); }
            Console.WriteLine("3");

            //targetMemberWarnRoles.Where(role => !expectedRoles.Contains(role)).ToList().ForEach(async unwantedRole => await targetMember.RevokeRoleAsync(unwantedRole));
            //expectedRoles.Where(expectedRole => !targetMemberWarnRoles.Contains(expectedRole)).ToList().ForEach(async wantedRole => await targetMember.GrantRoleAsync(wantedRole));
        }

        public bool CheckForUnwantedRoles(DiscordMember targetMember, User targetUser)
        {
            //Console.WriteLine(targetUser.offencesRecord.inGameOffence.getOffenceLevel());
            //Console.WriteLine(inGameWarnRoles[targetUser.offencesRecord.inGameOffence.getOffenceLevel()]);
            //Console.WriteLine(disciplinaryWarnRoles[targetUser.offencesRecord.disciplinaryOffence.getOffenceLevel()]);
            List<DiscordRole> expectedRoles = new List<DiscordRole>() { inGameWarnRoles[targetUser.offencesRecord.inGameOffence.getOffenceLevel()], disciplinaryWarnRoles[targetUser.offencesRecord.disciplinaryOffence.getOffenceLevel()] };
            expectedRoles.RemoveAll(role => role == null);
            //Console.WriteLine("2");
            List<DiscordRole> targetMemberWarnRoles = targetMember.Roles.Where(role => inGameWarnRoles.ContainsValue(role) || disciplinaryWarnRoles.ContainsValue(role)).ToList();
            //Console.WriteLine("3");

            //Console.WriteLine(expectedRoles.All(targetMemberWarnRoles.Contains));
            //Console.WriteLine(targetMemberWarnRoles.Count == expectedRoles.Count);

            return !(expectedRoles.All(targetMemberWarnRoles.Contains) && targetMemberWarnRoles.Count == expectedRoles.Count);
        }

        /*public async Task removeWarnRoles(DiscordMember targetMember)
        {

            targetMember.Roles.Where(role => inGameWarnRoles.ContainsValue(role) || disciplinaryWarnRoles.ContainsValue(role)).ToList().ForEach(async role => await targetMember.RevokeRoleAsync(role));
            await Task.Delay(100);
        }*/

        /*[Command("sistemaId"), RequirePermissionsAttribute(DSharpPlus.Permissions.MuteMembers)]
        public async Task fixUserIDs(CommandContext ctx)
        {
            var membersList = await ctx.Guild.GetAllMembersAsync();
            String modifiedUserNamesList = "";
            int fixedIdsNumber = 0;

            foreach (User user in userList)
            {
                DiscordMember searchedMember = membersList.FirstOrDefault(member => member.DisplayName == user.name);
                if (searchedMember == null) searchedMember = membersList.FirstOrDefault(member => member.Discriminator == user.iD);
                if (searchedMember == null) continue;

                Spreadsheet.UpdateEntry("B", $"{user.userIndex}", searchedMember.Id.ToString());
                modifiedUserNamesList += user.name + ", ";
                fixedIdsNumber++;
                //RefreshSpreadsheet();
            }

            if (fixedIdsNumber == 0) await ctx.RespondAsync("Non sono stati trovati utenti con ID da sistemare");
            else await ctx.RespondAsync($"In totale sono stati sistemati gli ID di {fixedIdsNumber} utenti: " + modifiedUserNamesList);
        }*/
    }
}
