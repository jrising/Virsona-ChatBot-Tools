using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.AgentEvaluate
{
    public class Special : IContent
    {
        public static Special ArgDelimSpecial = new Special("/");
        public static Special EndDelimSpecial = new Special("#");

        protected string name;

        public Special(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
		
		public override string ToString()
		{
			return string.Format ("[Special: Name={0}]", Name);
		}
    }
}
