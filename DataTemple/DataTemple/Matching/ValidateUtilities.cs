using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.Matching
{
    public class ValidateUtilities
    {
        public static ProbableStrength SeemsTime(string datetime)
        {
            return SeemsTimeOfDay(datetime).Better(SeemsSingleDay(datetime).Better(SeemsSingleMonth(datetime).Better(SeemsSingleYear(datetime))));
        }

        public static ProbableStrength SeemsTimeOfDay(string datetime)
        {
            if (datetime.Contains(":") || datetime.ToLower().Contains("am") || datetime.ToLower().Contains("pm") || datetime.ToLower().Contains("o'clock"))
                return ProbableStrength.Full;

            // Are we all numeric?
            if (datetime.Length < 3)
                return SeemsNumeric(datetime);

            return ProbableStrength.Zero;
        }

        public static ProbableStrength SeemsSingleDay(string datetime)
        {
            datetime = datetime.ToLower();
            string[] elements = datetime.Split(new char[] { ' ', ',' });
            if (elements[0] == "today" || elements[0] == "tomorrow" || elements[0] == "yesterday" || elements[0] == "monday" || elements[0] == "tuesday" || elements[0] == "wednesday" || elements[0] == "thursday" || elements[0] == "friday" || elements[0] == "saturday" || elements[0] == "sunday")
            {
                if (elements.Length == 1)
                    return ProbableStrength.Full;
                return SeemsSingleMonth(string.Join(" ", elements, 1, elements.Length - 1));
            }

            return ProbableStrength.Zero;
        }

        public static ProbableStrength SeemsSingleMonth(string datetime)
        {
            datetime = datetime.ToLower();
            string[] elements = datetime.Split(new char[] { ' ', ',' });
            if (elements[0] == "january" || elements[0] == "february" || elements[0] == "march" || elements[0] == "april" || elements[0] == "may" || elements[0] == "june" || elements[0] == "july" || elements[0] == "august" || elements[0] == "september" || elements[0] == "october" || elements[0] == "november" || elements[0] == "december")
            {
                if (elements.Length == 1)
                    return ProbableStrength.Full;
                return SeemsSingleYear(string.Join(" ", elements, 1, elements.Length - 1));
            }

            return ProbableStrength.Zero;
        }

        public static ProbableStrength SeemsSingleYear(string datetime)
        {
            if (datetime.Length == 4)
                return SeemsNumeric(datetime);

            if (datetime[0] == '\'' && datetime.Length == 3)
                return SeemsNumeric(datetime.Substring(1));

            if (datetime.Length == 2)
                return SeemsNumeric(datetime).Relative(ProbableStrength.Half);

            return ProbableStrength.Zero;
        }

        public static ProbableStrength SeemsNumeric(string input)
        {
            foreach (char cn in input)
                if (!char.IsNumber(cn))
                    return ProbableStrength.Zero;

            return ProbableStrength.Full;
        }
    }
}
