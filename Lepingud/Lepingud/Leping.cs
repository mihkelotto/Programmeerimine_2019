using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lepingud
{
    [Serializable]
    public class Leping
    {
        private string nimi;
        private string amet;
        private string kuup2ev;
        private string kestvus;
        private double palk;
        public Leping() { }
        public Leping(string Nimi, string Amet, string Kuup2ev, string Kestvus, double Palk)
        {
            nimi = Nimi;
            amet = Amet;
            kuup2ev = Kuup2ev;
            kestvus = Kestvus;
            palk = Palk;
        }
        public string Nimi
        {
            get { return nimi; }
            set { nimi = value; }
        }

        public string Amet
        {
            get { return amet; }
            set { amet = value; }
        }

        public string Kuup2ev
        {

            get {
                DateTime start;
                if (DateTime.TryParseExact(kuup2ev, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
                {
                    return kuup2ev;
                }

                else
                {
                    throw new Exception();
                }
            }
            set
            {
                DateTime start;
                if (DateTime.TryParseExact(value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
                {
                    kuup2ev = value;
                }

                else
                {
                    throw new Exception();
                }
            }
        }

        public string Kestvus
        {
            get {
                DateTime start;
                if (DateTime.TryParseExact(kestvus, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
                {
                    return kestvus;
                }

                else
                {
                    throw new Exception();
                }
            }
            set
            {
                DateTime start;
                if (DateTime.TryParseExact(value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
                {
                    kestvus = value;
                }

                else
                {
                    throw new Exception();
                }
            }
        }

        public double Palk
        {
            get { return palk; }
            set { palk = value; }
        }
    }
}
