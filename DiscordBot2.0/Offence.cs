using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot2._0
{
    public interface OffenceInterface
    {
        abstract Offence increaseOffence();
        abstract Offence decreaseOffence();
        abstract string getOffenceName();
        abstract int getOffenceLevel();
        abstract int getMaxOffenceLevel();
        abstract TimeSpan getOffenceDecayLengh();
        abstract DateTime getLastOffenceDate();
        abstract DateTime getLastDecayedOffenceRemovalDate();

        public bool isExpired()
        {
            if (getLastOffenceDate() > getLastDecayedOffenceRemovalDate())
            {
                return getLastOffenceDate().Add(getOffenceDecayLengh()) > DateTime.Today;
            }
            else
            {
                return getLastDecayedOffenceRemovalDate().Add(getOffenceDecayLengh()) > DateTime.Today;
            }
        }

        public int numberOfDecayedOffenceLevels()
        {
            if (!isExpired()) return 0;

            int n = 0;

            if (getLastOffenceDate() > getLastDecayedOffenceRemovalDate())
            {
                while (getLastOffenceDate().Add(getOffenceDecayLengh()) < DateTime.Today) { n++; }
                return n;
            }
            else
            {
                while (getLastDecayedOffenceRemovalDate().Add(getOffenceDecayLengh()) < DateTime.Today) { n++; }
                return n;
            }
        }
    }

    public class Offence : OffenceInterface
    {
        OffenceType offenceType;
        private int offenceLevel = 0;
        private DateTime lastOffenceDate = DateTime.Parse("01/01/2001");
        private DateTime lastDecayedOffenceRemovalDate = DateTime.Parse("01/01/2001");

        public Offence(OffenceType offenceType)
        {
            this.offenceType = offenceType;
        }

        public Offence(OffenceType offenceType, int offenceLevel, DateTime lastOffenceDate, DateTime lastDecayedOffenceRemovalDate) : this(offenceType)
        {
            this.offenceLevel = offenceLevel;
            this.lastOffenceDate = lastOffenceDate;
            this.lastDecayedOffenceRemovalDate = lastDecayedOffenceRemovalDate;
        }

        public Offence decreaseOffence()
        {
            if (offenceLevel == 0) { throw new OffenceLevelZeroException();  }
            offenceLevel--;
            lastDecayedOffenceRemovalDate = DateTime.Today;
            return this;
        }

        public Offence increaseOffence()
        {
            if (offenceLevel == getMaxOffenceLevel()) { throw new OffenceLevelOverTheCapException(); }
            offenceLevel++;
            lastOffenceDate = DateTime.Today;
            return this;
        }

        public int getOffenceLevel()
        {
            return offenceLevel;
        }

        public DateTime getLastOffenceDate()
        {
           return lastOffenceDate;
        }

        public DateTime getLastDecayedOffenceRemovalDate()
        {
            return lastDecayedOffenceRemovalDate;
        }

        public int getMaxOffenceLevel()
        {
            return offenceType.getMaxOffenceLevel();
        }

        public string getOffenceName()
        {
            return offenceType.getOffenceName();
        }

        public TimeSpan getOffenceDecayLengh()
        {
            return offenceType.getOffenceDecayLengh();
        }

        public override bool Equals(object? obj)
        {
            return obj is Offence offence &&
                   EqualityComparer<OffenceType>.Default.Equals(offenceType, offence.offenceType) &&
                   offenceLevel == offence.offenceLevel &&
                   lastOffenceDate == offence.lastOffenceDate &&
                   lastDecayedOffenceRemovalDate == offence.lastDecayedOffenceRemovalDate;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(offenceType, offenceLevel, lastOffenceDate, lastDecayedOffenceRemovalDate);
        }
    }

    public class OffenceType
    {
        readonly string offenceName;
        readonly int maxOffenceLevel;
        readonly TimeSpan offenceDecayLenght;

        public OffenceType(string offenceName, int maxOffenceLevel, TimeSpan offenceDecayLenght)
        {
            this.offenceName = offenceName;
            this.maxOffenceLevel = maxOffenceLevel;
            this.offenceDecayLenght = offenceDecayLenght;
        }

        public int getMaxOffenceLevel()
        {
            return maxOffenceLevel;
        }

        public string getOffenceName()
        {
            return offenceName;
        }

        public TimeSpan getOffenceDecayLengh()
        {
            return offenceDecayLenght;
        }

        public override bool Equals(object? obj)
        {
            return obj is OffenceType type &&
                   offenceName == type.offenceName &&
                   maxOffenceLevel == type.maxOffenceLevel &&
                   offenceDecayLenght.Equals(type.offenceDecayLenght);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(offenceName, maxOffenceLevel, offenceDecayLenght);
        }
    }

    sealed internal class OffenceTypes
    {
        public static OffenceType inGameOffence()
        {
            return new OffenceType("Infrazione In-Game", 4, new TimeSpan(120, 0, 0, 0)); //, "E", "K"
        }

        public static OffenceType disciplinaryOffence()
        {
            return new OffenceType("Infrazione Disciplinare", 4, new TimeSpan(180, 0, 0, 0)); // "F", "L"
        }

        public static OffenceType textMute()
        {
            return new OffenceType("Mute Testuale", 4, new TimeSpan(90, 0, 0, 0)); // "C", "I"
        }

        public static OffenceType voiceMute()
        {
            return new OffenceType("Mute Vocale", 3, new TimeSpan(120, 0, 0, 0)); //, "D", "J"
        }
    }

    [Serializable]
    internal class OffenceLevelZeroException : Exception
    {
        public OffenceLevelZeroException() : base("Offence level is already at 0") { }
    }

    [Serializable]
    internal class OffenceLevelOverTheCapException : Exception
    {
        public OffenceLevelOverTheCapException() : base("Offence level is already at max") { }
    }
}
