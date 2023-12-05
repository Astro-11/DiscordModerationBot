using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot2._0
{
    public class OffencesRecord
    {
        public OffenceInterface inGameOffence { get; private set; }
        public OffenceInterface disciplinaryOffence { get; private set; }
        public OffenceInterface textMute { get; private set; }
        public OffenceInterface voiceMute { get; private set;  }

        private List<OffenceInterface> offences;

        //public DateTime lastOffenceDate { get; private set; }

        public OffencesRecord()
        {
            inGameOffence = new Offence(OffenceTypes.inGameOffence());
            disciplinaryOffence = new Offence(OffenceTypes.disciplinaryOffence());
            textMute = new Offence(OffenceTypes.textMute());
            voiceMute = new Offence(OffenceTypes.voiceMute());
            offences = new List<OffenceInterface>() { inGameOffence, disciplinaryOffence, textMute, voiceMute };
            //lastOffenceDate = inGameOffence.getLastOffenceDate();
        }

        public OffencesRecord(Offence inGameOffence, Offence disciplinaryOffence, Offence textMute, Offence voiceMute)
        {
            this.inGameOffence = inGameOffence;
            this.disciplinaryOffence = disciplinaryOffence;
            this.textMute = textMute;
            this.voiceMute = voiceMute;
            offences = new List<OffenceInterface>() { inGameOffence, disciplinaryOffence, textMute, voiceMute };
        }

        public List<OffenceInterface> getExpiredOffences()
        {
            return offences.Where(offence => offence.isExpired()).ToList();
        }

        public Dictionary<OffenceInterface, int> getOffencesNumberOfDecayedLevels()
        {
            return offences.ToDictionary(x => x, x => x.numberOfDecayedOffenceLevels());
        }

        public void removeExpiredOffences()
        {
            foreach (OffenceInterface offence in offences)
            {
                while (offence.isExpired())
                {
                    offence.decreaseOffence();
                }
            }
        }

        public bool contains(OffenceInterface offence)
        {
            return offences.Contains(offence);
        }

        public override bool Equals(object? obj)
        {
            return obj is OffencesRecord record &&
                   EqualityComparer<OffenceInterface>.Default.Equals(inGameOffence, record.inGameOffence) &&
                   EqualityComparer<OffenceInterface>.Default.Equals(disciplinaryOffence, record.disciplinaryOffence) &&
                   EqualityComparer<OffenceInterface>.Default.Equals(textMute, record.textMute) &&
                   EqualityComparer<OffenceInterface>.Default.Equals(voiceMute, record.voiceMute) &&
                   EqualityComparer<List<OffenceInterface>>.Default.Equals(offences, record.offences);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(inGameOffence, disciplinaryOffence, textMute, voiceMute, offences);
        }


        /*public ReadOnlyCollection<OffenceInterface> getOffences() 
        {
            return offences.AsReadOnly(); 
        }*/
    }
}
