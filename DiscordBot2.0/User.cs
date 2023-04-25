using DiscordBot2._0;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class User
    {
        public string name;
        public string iD;
        public int userIndex;
        public int textMute;
        public int voiceMute;
        public int inGameOffence;
        public int disciplinaryOffence;
        public DateTime lastOffenceDate;
        public string lastTextMuteRemovalDate;
        public string lastVoiceMuteRemovalDate;
        public string lastInGameOffenceRemovalDate;
        public string lastDisciplinaryOffenceRemovalDate;
        private int[] expiredOffences = null;

        public bool? AddOffence(string offenceType, string moderatorName)
        {
            DateTime todayDate = DateTime.Today;
            int numberOfPreviousPunishments = 0;
            string column = "";
            string updatedEntry = "";
            string offenceToLog = "";
            int i = 0;

            switch (offenceType)
            {
                case "textMute":
                    offenceToLog = "Mute Testuale";
                    column = "C";
                    numberOfPreviousPunishments = textMute;
                    if (numberOfPreviousPunishments >= 4) return false;
                    createEntry();
                    textMute++;
                    break;
                case "voiceMute":
                    offenceToLog = "Mute Vocale";
                    column = "D";
                    numberOfPreviousPunishments = voiceMute;
                    if (numberOfPreviousPunishments >= 3) return false;
                    createEntry();
                    voiceMute++;
                    break;
                case "inGameOffence":
                    offenceToLog = "Infrazione In-Game";
                    column = "E";
                    numberOfPreviousPunishments = inGameOffence;
                    if (numberOfPreviousPunishments >= 4) return false;
                    createEntry();
                    inGameOffence++;
                    break;
                case "disciplinaryOffence":
                    offenceToLog = "Infrazione Disciplinare";
                    column = "F";
                    numberOfPreviousPunishments = disciplinaryOffence;
                    if (numberOfPreviousPunishments >= 3) return false;
                    createEntry();
                    disciplinaryOffence++;
                    break;
            }

            void createEntry()
            {
                while (i <= numberOfPreviousPunishments)
                {
                    updatedEntry = updatedEntry + "X";
                    i++;
                }
            }

            Spreadsheet.UpdateEntry(column, userIndex.ToString(), updatedEntry);
            Spreadsheet.UpdateEntry("G", userIndex.ToString(), todayDate.ToString("dd/MM/yyyy"));
            Spreadsheet.CreateLog($"{moderatorName} ha aggiunto {offenceToLog} {i} a {name}#{iD} in data {todayDate.ToString("dd/MM/yyyy")} per: ");
            return true;
        }

        public bool? RemoveOffence(string offenceType, string moderatorName)
        {
            DateTime todayDate = DateTime.Today;
            int numberOfPreviousPunishments = 0;
            string column = "";
            string lastOffenceRemovalColumn = "";
            string updatedEntry = "";
            string offenceToLog = "";
            int i = 0;

            switch (offenceType)
            {
                case "testuale":
                    offenceToLog = "Mute Testuale";
                    column = "C";
                    lastOffenceRemovalColumn = "I";
                    numberOfPreviousPunishments = textMute;
                    if (numberOfPreviousPunishments == 0) return false;
                    textMute--;
                    break;
                case "vocale":
                    offenceToLog = "Mute Vocale";
                    column = "D";
                    lastOffenceRemovalColumn = "J";
                    numberOfPreviousPunishments = voiceMute;
                    if (numberOfPreviousPunishments == 0) return false;
                    voiceMute--;
                    break;
                case "ingame":
                    offenceToLog = "Infrazione In-Game";
                    column = "E";
                    lastOffenceRemovalColumn = "L";
                    numberOfPreviousPunishments = inGameOffence;
                    if (numberOfPreviousPunishments == 0) return false;
                    inGameOffence--;
                    break;
                case "disciplinare":
                    offenceToLog = "Infrazione Disciplinare";
                    column = "F";
                    lastOffenceRemovalColumn = "L";
                    numberOfPreviousPunishments = disciplinaryOffence;
                    if (numberOfPreviousPunishments == 0) return false;
                    disciplinaryOffence--;
                    break;
            }

            if (numberOfPreviousPunishments != 0)
            {
                while (i + 1 < numberOfPreviousPunishments)
                {
                    updatedEntry = updatedEntry + "X";
                    i++;
                }
                if (string.IsNullOrEmpty(updatedEntry))
                {
                    updatedEntry = "/";
                }

                Spreadsheet.UpdateEntry(column, userIndex.ToString(), updatedEntry);
                Spreadsheet.UpdateEntry(lastOffenceRemovalColumn, userIndex.ToString(), todayDate.ToString("dd/MM/yyyy"));
                Spreadsheet.CreateLog($"{moderatorName} ha rimosso {offenceToLog} {i + 1} a {name}#{iD} in data {todayDate.ToString("dd/MM/yyyy")}");
            }

            return true;
        }

        private DateTime ParseToDateTime(string str)
        {
            if (str != "00/00/00" && str != "/") return (DateTime.ParseExact(str, "dd/MM/yyyy", null));
            else return DateTime.Now;
        }

        public int[] GetOffences()
        {
            return new int[] { textMute, voiceMute, inGameOffence, disciplinaryOffence };
        }

        public int[] CheckExpiredOffences()
        {
            if (expiredOffences != null) return expiredOffences;
            expiredOffences = new int[4];

            if (textMute > 0)
            {
                int tempTextMuteOffences = textMute;
                if (DateTime.Now > lastOffenceDate.AddMonths(3))
                {
                    DateTime lastOffenceRemovalDate;

                    if (lastTextMuteRemovalDate != "/") lastOffenceRemovalDate = ParseToDateTime(lastTextMuteRemovalDate);
                    else lastOffenceRemovalDate = lastOffenceDate;

                    while (DateTime.Now > lastOffenceRemovalDate.AddMonths(3) && tempTextMuteOffences != 0)
                    {
                        lastOffenceRemovalDate = lastOffenceRemovalDate.AddMonths(3);
                        expiredOffences[0] += 1;
                        tempTextMuteOffences--;
                    }
                }
            }

            if (voiceMute > 0)
            {
                int tempInVoiceMuteOffences = voiceMute;
                if (DateTime.Now > lastOffenceDate.AddMonths(3))
                {
                    DateTime lastOffenceRemovalDate;

                    if (lastTextMuteRemovalDate != "/") lastOffenceRemovalDate = ParseToDateTime(lastVoiceMuteRemovalDate);
                    else lastOffenceRemovalDate = lastOffenceDate;

                    while (DateTime.Now > lastOffenceRemovalDate.AddMonths(3) && tempInVoiceMuteOffences != 0)
                    {
                        lastOffenceRemovalDate = lastOffenceRemovalDate.AddMonths(3);
                        expiredOffences[1] += 1;
                        tempInVoiceMuteOffences--;
                    }
                }
            }

            if (inGameOffence > 0)
            {
                int tempInGameOffences = inGameOffence;
                if (DateTime.Now > lastOffenceDate.AddMonths(3))
                {
                    DateTime lastOffenceRemovalDate;

                    if (lastTextMuteRemovalDate != "/") lastOffenceRemovalDate = ParseToDateTime(lastInGameOffenceRemovalDate);
                    else lastOffenceRemovalDate = lastOffenceDate;

                    while (DateTime.Now > lastOffenceRemovalDate.AddMonths(4) && tempInGameOffences != 0)
                    {
                        lastOffenceRemovalDate = lastOffenceRemovalDate.AddMonths(4);
                        expiredOffences[2] += 1;
                        tempInGameOffences--;
                    }
                }
            }

            if (disciplinaryOffence > 0)
            {
                int tempDisciplinaryOffences = disciplinaryOffence;
                if (DateTime.Now > lastOffenceDate.AddMonths(3))
                {
                    DateTime lastOffenceRemovalDate;

                    if (lastTextMuteRemovalDate != "/") lastOffenceRemovalDate = ParseToDateTime(lastDisciplinaryOffenceRemovalDate);
                    else lastOffenceRemovalDate = lastOffenceDate;

                    while (DateTime.Now > lastOffenceRemovalDate.AddMonths(6) & tempDisciplinaryOffences != 0)
                    {
                        lastOffenceRemovalDate = lastOffenceRemovalDate.AddMonths(6);
                        expiredOffences[3] += 1;
                        tempDisciplinaryOffences--;
                    }
                }
            }

            return expiredOffences;

            //Creare classe Offence con sottoclassi TextMute, VoiceMute etc contenenti il numero di infrazioni e la durata delle infrazioni per ottimizzare il codice
        }

        public bool RemoveExpiredOffences(string moderatorName)
        {
            if (expiredOffences == null) expiredOffences = CheckExpiredOffences();
            bool didRemove = false;

            while (expiredOffences[0] > 0) 
            {
                RemoveOffence("testuale", moderatorName);
                expiredOffences[0]--;
                didRemove = true;
            }
            while (expiredOffences[1] > 0)
            {
                RemoveOffence("vocale", moderatorName);
                expiredOffences[1]--;
                didRemove = true;
            }
            while (expiredOffences[2] > 0)
            {
                RemoveOffence("ingame", moderatorName);
                expiredOffences[2]--;
                didRemove = true;
            }
            while (expiredOffences[3] > 0)
            {
                RemoveOffence("disciplinare", moderatorName);
                expiredOffences[3]--;
                didRemove = true;
            }

            return didRemove;
        }
    }
}
