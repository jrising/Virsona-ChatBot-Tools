using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using PluggerBase;
using GenericTools;

namespace InOutTools
{
    public class Unilog : IMessageReceiver
    {
        public struct LogEntry {
            public DateTime when;
            public double level;
            public object source;
            public string stackTrace;
            public int parent;
            public string message;
            public object[] args;

            public LogEntry(DateTime when, double level, object source, string stackTrace, int parent, string message, object[] args)
            {
                this.when = when;
                this.level = level;
                this.source = source;
                this.stackTrace = stackTrace;
                this.parent = parent;
                this.message = message;
                this.args = args;
            }
        }

        public static double levelNotice = 0.0;
        public static double levelWarning = 1.0;
        public static double levelRecoverable = 5.0;
        public static double levelUnrecoverable = 40.0;

        protected static ExpiringDictionary<object, double> maxLevels = new ExpiringDictionary<object,double>();
        protected static ExpiringDictionary<object, double> errorLevels = new ExpiringDictionary<object, double>();

        protected static int maxEntryId = 0;
        protected static ExpiringDictionary<int, LogEntry> entries = new ExpiringDictionary<int, LogEntry>();
		
		public static bool HasEntries {
			get {
				return entries.Count > 0;
			}
		}
		
        public static int Log(double salience, string source, string message, params object[] args)
        {
            return Error(salience, source, null, message, args);
        }

        public static int Exception(Exception ex, object source, string message, params object[] args)
        {
            return Error(levelRecoverable, source, ex.StackTrace, message + ": " + ex.Message, args);
        }
        
        public static int Notice(object source, string message, params object[] args)
        {
            return Error(levelNotice, source, Environment.StackTrace, message, args);
        }

        public static int Warning(object source, string message, params object[] args)
        {
            return Error(levelWarning, source, Environment.StackTrace, message, args);
        }

        public static int Recoverable(object source, string message, params object[] args)
        {
            return Error(levelRecoverable, source, Environment.StackTrace, message, args);
        }

        public static int Unrecoverable(object source, string message, params object[] args)
        {
            return Error(levelUnrecoverable, source, Environment.StackTrace, message, args);
        }

        public static int Error(double level, object source, string stackTrace, string message, object[] args)
        {
            if (!(source is string))
            {
                lock (errorLevels)
                {
                    double prevlev = 0;
                    errorLevels.TryGetValue(source, out prevlev);
                    errorLevels[source] = prevlev + level;

                    double maxlev = levelUnrecoverable;
                    if (maxLevels.TryGetValue(source, out maxlev))
                    {
                        string fullmsg = string.Format(message, args);

                        if (prevlev + level > maxlev)
                            throw new Exception(fullmsg);
                    }
                }
            }

            lock (entries)
            {
                entries[++maxEntryId] = new LogEntry(DateTime.Now, level, source, stackTrace, 0, message, args);
            }

            return maxEntryId;
        }

        public static void FlushToNull()
        {
            lock (entries)
            {
                entries.Clear();
                maxEntryId = 0;
            }
        }

        public static void FlushToFile(string filename) {
            lock (entries)
            {
                StreamWriter logfile = File.AppendText(filename);
                foreach (KeyValuePair<int, LogEntry> kvp in entries)
                {
                    LogEntry entry = kvp.Value;
                    if (entry.source is string)
                        logfile.WriteLine(kvp.Key + "," + entry.when + "," + entry.source + "," + string.Format(entry.message, entry.args));
                    else if (entry.parent == 0)
                        logfile.WriteLine(kvp.Key + "," + entry.when + "," + entry.source + "," + string.Format(entry.message, entry.args) + "," + entry.stackTrace);
                    else
                        logfile.WriteLine(kvp.Key + "," + entry.when + "," + entry.source + "," + string.Format(entry.message, entry.args) + "," + entry.stackTrace + "," + entry.parent);
                }
                logfile.Close();

                FlushToNull();
            }
        }

        /*public static void FlushToDatabase(DbQuery dbquery)
        {
            // Ignore-- not implemented yet!
            FlushToNull();
        }*/

        public static void FlushToXml(XmlTextWriter xml)
        {
            lock (entries)
            {
                foreach (KeyValuePair<int, LogEntry> kvp in entries)
                {
                    LogEntry entry = kvp.Value;
                    xml.WriteStartElement("entry");

                    xml.WriteAttributeString("id", kvp.Key.ToString());
                    xml.WriteElementString("when", entry.when.ToString());
                    xml.WriteElementString("src", entry.source.ToString());
                    xml.WriteElementString("msg", string.Format(entry.message, entry.args));
                    if (entry.stackTrace != null)
                        xml.WriteElementString("trace", entry.stackTrace);
                    if (entry.parent != 0)
                        xml.WriteElementString("parent", entry.parent.ToString());

                    xml.WriteEndElement();
                }

                FlushToNull();
            }
        }
		
		public static string FlushToStringShort() {
            lock (entries)
            {
                StringBuilder result = new StringBuilder();
                foreach (KeyValuePair<int, LogEntry> kvp in entries)
                {
                    LogEntry entry = kvp.Value;
                    result.Append(entry.source.ToString());
                    result.Append(": ");
                    result.Append(string.Format(entry.message, entry.args));
                    result.Append("\n");
                }

                FlushToNull();

                return result.ToString();
            }
		}
		
        public static string FlushToString(string partdelim, string entrydelim)
        {
            lock (entries)
            {
                StringBuilder result = new StringBuilder();
                foreach (KeyValuePair<int, LogEntry> kvp in entries)
                {
                    LogEntry entry = kvp.Value;
                    result.Append(kvp.Key);
                    result.Append(partdelim);
                    result.Append(entry.when.ToString());
                    result.Append(partdelim);
                    result.Append(entry.source.ToString());
                    result.Append(partdelim);
                    result.Append(string.Format(entry.message, entry.args));
                    if (entry.stackTrace != null)
                    {
                        result.Append(partdelim);
                        result.Append(entry.stackTrace);
                    }
                    if (entry.parent != 0)
                    {
                        result.Append(partdelim);
                        result.Append(entry.parent);
                    }
                    result.Append(entrydelim);
                }

                FlushToNull();

                return result.ToString();
            }
        }
		
		public static void DropBelow(double level) {
            lock (entries)
            {
				ExpiringDictionary<int, LogEntry> afterward = new ExpiringDictionary<int, LogEntry>();
				foreach (KeyValuePair<int, LogEntry> kvp in entries)
					if (kvp.Value.level >= level)
						afterward.Add(kvp);
				
				entries = afterward;
            }
		}
		
        /// An instance of this class, used as an IMessageReceiver

        protected List<object> objectIgnores;
        protected List<string> stringIgnores;

        public Unilog(List<object> objectIgnores, List<string> stringIgnores)
        {
            this.objectIgnores = objectIgnores;
            this.stringIgnores = stringIgnores;
        }

        #region IMessageReceiver Members

        public bool Receive(string message, object reference)
        {
            if (objectIgnores.Contains(reference) || stringIgnores.Contains(message))
                return false;

            Notice(reference, message);
            return true;
        }

        #endregion
    }
}
