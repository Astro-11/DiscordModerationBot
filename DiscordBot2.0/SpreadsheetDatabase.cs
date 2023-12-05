using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot2._0
{
    public interface Database
    {
        public List<User> refreshUsers();
        public User refreshUser(User user);
        public List<User> getUsers();
        public User getUser(decimal userId);
        public void addUser(string userName, decimal userIndex);
        public void deleteUser(User user);
        public void commitUserChanges(User user, string author);
        public void commitUserOffencesChanges(User user, OffenceInterface offence, string author);
        public void commitUserOffencesChanges(User user, string author);
    }

    internal class SpreadsheetDatabase : Database
    {
        private int linesFromTopToSkip = 3;
        private List<User> userList = new List<User>();
        private Dictionary<User, int> userIndexInDatabase = new Dictionary<User, int>();

        public SpreadsheetDatabase()
        {
            userList = refreshUsers();
        }

        public List<User> getUsers()
        {
            return userList;
        }

        public User getUser(decimal userId)
        {
            Console.WriteLine("Wanted user ID: " + userId);
            //foreach (User user in userList) Console.WriteLine(user.name + " " + user.offencesRecord.inGameOffence.getOffenceLevel());
            User? wantedUser = userList.Find(user => user.id == userId);
            if (wantedUser == null) { throw new CouldNotFindUserException(); }
            else return wantedUser;
        }

        public void addUser(string userName, decimal userIndex)
        {
            Spreadsheet.CreateUser(userName, userIndex.ToString());
            refreshUsers();
        }

        public void deleteUser(User user)
        {
            string userIndexString = userIndexInDatabase[user].ToString();
            Spreadsheet.DeleteEntry($"A{userIndexString}", $"L{userIndexString}");
            refreshUsers();
        }

        public void commitUserChanges(User user, string author)
        {
            string userIndexString = userIndexInDatabase[user].ToString();

            Spreadsheet.UpdateEntry("A", userIndexString, user.name);
            Spreadsheet.UpdateEntry("B", userIndexString, user.id.ToString());
        }

        public void commitUserOffencesChanges(User user, OffenceInterface offence, string author)
        {
            Console.WriteLine("Commit changes requested for " + user.name + " with " + offence.getOffenceLevel() + " " + offence.getOffenceName());
            if (!user.offencesRecord.contains(offence)) throw new CouldNotFindUserException();
            Console.WriteLine("Offence present");
            User oldUser = refreshUser(user);
            Console.WriteLine("Old user found: " + oldUser.name + " with " + oldUser.offencesRecord.textMute.getOffenceLevel() + " text mutes");
            string userIndexString = userIndexInDatabase[user].ToString();
            bool hasIncreased = false;

            switch (offence.getOffenceName())
            {
                case "Mute Testuale":
                    if (user.offencesRecord.textMute.Equals(oldUser.offencesRecord.textMute)) return;
                    Console.WriteLine("Offence was internally updated");
                    if (user.offencesRecord.textMute.getOffenceLevel() > oldUser.offencesRecord.textMute.getOffenceLevel()) hasIncreased = true;
                    Spreadsheet.UpdateEntry("C", userIndexString, createOffenceEntry(user.offencesRecord.textMute));
                    if (!hasIncreased) Spreadsheet.UpdateEntry("I", userIndexString, user.offencesRecord.textMute.getLastDecayedOffenceRemovalDate().ToString("dd/MM/yyyy"));
                    break;
                case "Mute Vocale":
                    if (user.offencesRecord.voiceMute.Equals(oldUser.offencesRecord.voiceMute)) return;
                    Console.WriteLine("Offence was internally updated");
                    if (user.offencesRecord.voiceMute.getOffenceLevel() > oldUser.offencesRecord.voiceMute.getOffenceLevel()) hasIncreased = true;
                    Spreadsheet.UpdateEntry("D", userIndexString, createOffenceEntry(user.offencesRecord.voiceMute));
                    if (!hasIncreased) Spreadsheet.UpdateEntry("J", userIndexString, user.offencesRecord.voiceMute.getLastDecayedOffenceRemovalDate().ToString("dd/MM/yyyy"));
                    break;
                case "Infrazione In-Game":
                    if (user.offencesRecord.inGameOffence.Equals(oldUser.offencesRecord.inGameOffence)) return;
                    Console.WriteLine("Offence was internally updated");
                    if (user.offencesRecord.inGameOffence.getOffenceLevel() > oldUser.offencesRecord.inGameOffence.getOffenceLevel()) hasIncreased = true;
                    Spreadsheet.UpdateEntry("E", userIndexString, createOffenceEntry(user.offencesRecord.inGameOffence));
                    if (!hasIncreased) Spreadsheet.UpdateEntry("K", userIndexString, user.offencesRecord.inGameOffence.getLastDecayedOffenceRemovalDate().ToString("dd/MM/yyyy"));
                    break;
                case "Infrazione Disciplinare":
                    if (user.offencesRecord.disciplinaryOffence.Equals(oldUser.offencesRecord.disciplinaryOffence)) return;
                    Console.WriteLine("Offence was internally updated");
                    if (user.offencesRecord.disciplinaryOffence.getOffenceLevel() > oldUser.offencesRecord.disciplinaryOffence.getOffenceLevel()) hasIncreased = true;
                    Spreadsheet.UpdateEntry("F", userIndexString, createOffenceEntry(user.offencesRecord.disciplinaryOffence));
                    if (!hasIncreased) Spreadsheet.UpdateEntry("L", userIndexString, user.offencesRecord.disciplinaryOffence.getLastDecayedOffenceRemovalDate().ToString("dd/MM/yyyy"));
                    break;
                default:
                    throw new CouldNotFindUserException();
            }
            Console.WriteLine("Offence sent to DB");

            if (hasIncreased) Spreadsheet.UpdateEntry("G", userIndexString, DateTime.Today.ToString("dd/MM/yyyy"));

            if (hasIncreased) Spreadsheet.CreateLog($"{author} ha aggiunto {offence.getOffenceName()} {offence.getOffenceLevel()} a {user.name}#{user.id} in data {DateTime.Today.ToString("dd/MM/yyyy")}");
            else Spreadsheet.CreateLog($"{author} ha rimosso {offence.getOffenceName()} {offence.getOffenceLevel() + 1} a {user.name}#{user.id} in data {offence.getLastDecayedOffenceRemovalDate().ToString("dd/MM/yyyy")}");

            string createOffenceEntry(OffenceInterface offence)
            {
                string entry = "/";

                for (int i = 0; i < offence.getOffenceLevel(); i++)
                {
                    entry = entry + "X";
                }

                return entry;
            }
        }

        public void commitUserOffencesChanges(User user, string author)
        {
            commitUserOffencesChanges(user, user.offencesRecord.textMute, author);
            commitUserOffencesChanges(user, user.offencesRecord.voiceMute, author);
            commitUserOffencesChanges(user, user.offencesRecord.inGameOffence, author);
            commitUserOffencesChanges(user, user.offencesRecord.disciplinaryOffence, author);
        }

        public User refreshUser(User user)
        {
            string userIndexString = userIndexInDatabase[user].ToString();

            string name = Spreadsheet.ReadEntry($"A{userIndexString}", $"A{userIndexString}").First();
            decimal id = Spreadsheet.ReadEntry($"B{userIndexString}", $"B{userIndexString}").Select(id => decimal.Parse(id)).First();
            int inGameOffenceLevel = Spreadsheet.ReadEntry($"E{userIndexString}", $"E{userIndexString}").Select(inGameOffence => toNumber(inGameOffence)).First();
            int disciplinaryOffenceLevel = Spreadsheet.ReadEntry($"F{userIndexString}", $"F{userIndexString}").Select(disciplinaryOffence => toNumber(disciplinaryOffence)).First();
            int textMuteLevel = Spreadsheet.ReadEntry($"C{userIndexString}", $"C{userIndexString}").Select(textMute => toNumber(textMute)).First();
            int voiceMuteLevel = Spreadsheet.ReadEntry($"D{userIndexString}", $"D{userIndexString}").Select(voiceMutes => toNumber(voiceMutes)).First();
            DateTime lastOffenceDate = Spreadsheet.ReadEntry($"G{userIndexString}", $"G{userIndexString}").Select(lastOffenceDate => toDate(lastOffenceDate)).First();
            DateTime lastTextMuteRemovalDate = Spreadsheet.ReadEntry($"I{userIndexString}", $"I{userIndexString}").Select(lastTextMuteRemovalDate => toDate(lastTextMuteRemovalDate)).First();
            DateTime lastVoiceMuteRemovalDate = Spreadsheet.ReadEntry($"J{userIndexString}", $"J{userIndexString}").Select(lastVoiceMuteRemovalDate => toDate(lastVoiceMuteRemovalDate)).First();
            DateTime lastInGameOffenceRemovalDate = Spreadsheet.ReadEntry($"K{userIndexString}", $"K{userIndexString}").Select(lastInGameOffenceRemovalDate => toDate(lastInGameOffenceRemovalDate)).First();
            DateTime lastDisciplinaryOffenceRemovalDate = Spreadsheet.ReadEntry($"L{userIndexString}", $"L{userIndexString}").Select(lastDisciplinaryOffenceRemovalDate => toDate(lastDisciplinaryOffenceRemovalDate)).First();

            Offence inGameOffence = new Offence(OffenceTypes.inGameOffence(), inGameOffenceLevel, lastOffenceDate, lastInGameOffenceRemovalDate);
            Offence disciplinarOffence = new Offence(OffenceTypes.disciplinaryOffence(), disciplinaryOffenceLevel, lastOffenceDate, lastDisciplinaryOffenceRemovalDate);
            Offence textMute = new Offence(OffenceTypes.textMute(), textMuteLevel, lastOffenceDate, lastTextMuteRemovalDate);
            Offence voiceMute = new Offence(OffenceTypes.voiceMute(), voiceMuteLevel, lastOffenceDate, lastVoiceMuteRemovalDate);
            OffencesRecord offenceRecord = new OffencesRecord(inGameOffence, disciplinarOffence, textMute, voiceMute);
            User newUser = new User(name, id, offenceRecord);
            user = newUser;

            return user;
        }

        public List<User> refreshUsers()
        {
            userList.Clear();

            string[] names = Spreadsheet.ReadEntry($"A{linesFromTopToSkip}", "A").ToArray();
            decimal[] iDs = Spreadsheet.ReadEntry($"B{linesFromTopToSkip}", "B").Select(id => decimal.Parse(id)).ToArray();
            int[] inGameOffencesLevel = Spreadsheet.ReadEntry($"E{linesFromTopToSkip}", "E").Select(inGameOffence => toNumber(inGameOffence)).ToArray();
            int[] disciplinaryOffencesLevel = Spreadsheet.ReadEntry($"F{linesFromTopToSkip}", "F").Select(disciplinaryOffence => toNumber(disciplinaryOffence)).ToArray();
            int[] textMutesLevel = Spreadsheet.ReadEntry($"C{linesFromTopToSkip}", "C").Select(textMute => toNumber(textMute)).ToArray();
            int[] voiceMutesLevel = Spreadsheet.ReadEntry($"D{linesFromTopToSkip}", "D").Select(voiceMutes => toNumber(voiceMutes)).ToArray();
            DateTime[] lastOffenceDates = Spreadsheet.ReadEntry($"G{linesFromTopToSkip}", "G").Select(lastOffenceDate => toDate(lastOffenceDate)).ToArray();
            DateTime[] lastTextMuteRemovalDates = Spreadsheet.ReadEntry($"I{linesFromTopToSkip}", "I").Select(lastTextMuteRemovalDate => toDate(lastTextMuteRemovalDate)).ToArray();
            DateTime[] lastVoiceMuteRemovalDates = Spreadsheet.ReadEntry($"J{linesFromTopToSkip}", "J").Select(lastVoiceMuteRemovalDate => toDate(lastVoiceMuteRemovalDate)).ToArray();
            DateTime[] lastInGameOffenceRemovalDates = Spreadsheet.ReadEntry($"K{linesFromTopToSkip}", "K").Select(lastInGameOffenceRemovalDate => toDate(lastInGameOffenceRemovalDate)).ToArray();
            DateTime[] lastDisciplinaryOffenceRemovalDates = Spreadsheet.ReadEntry($"L{linesFromTopToSkip}", "L").Select(lastDisciplinaryOffenceRemovalDate => toDate(lastDisciplinaryOffenceRemovalDate)).ToArray();

            for (int i = 0; i < names.Length; i++)
            {
                Offence inGameOffence = new Offence(OffenceTypes.inGameOffence(), inGameOffencesLevel[i], lastOffenceDates[i], lastInGameOffenceRemovalDates[i]);
                Offence disciplinarOffence = new Offence(OffenceTypes.disciplinaryOffence(), disciplinaryOffencesLevel[i], lastOffenceDates[i], lastDisciplinaryOffenceRemovalDates[i]);
                Offence textMute = new Offence(OffenceTypes.textMute(), textMutesLevel[i], lastOffenceDates[i], lastTextMuteRemovalDates[i]);
                Offence voiceMute = new Offence(OffenceTypes.voiceMute(), voiceMutesLevel[i], lastOffenceDates[i], lastVoiceMuteRemovalDates[i]);
                OffencesRecord offenceRecord = new OffencesRecord(inGameOffence, disciplinarOffence, textMute, voiceMute);
                User user = new User(names[i], iDs[i], offenceRecord);
                userList.Add(user);
                userIndexInDatabase.Add(user, i + linesFromTopToSkip);
            }

            return userList;
        }

        private int toNumber(string offenceString)
        {
            int n = 0;
            foreach (char c in offenceString) if (c == 'X') n++;
            return n;
        }

        private DateTime toDate(string dateString)
        {
            if (dateString != "Data ultima punizione" && dateString != "00/00/00" && dateString != "/")
            {
                return (DateTime.ParseExact(dateString, "dd/MM/yyyy", null));
            }
            else
            {
                return (DateTime.Parse("01/01/2001"));
            }
        }

        [Serializable]
        internal class CouldNotFindUserException : Exception
        {
            public CouldNotFindUserException() : base("User could not be found") { }
        }
    }
}
