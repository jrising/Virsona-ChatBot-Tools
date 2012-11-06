using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.AgentEvaluate
{
    public class Word : IContent
    {
        protected string name;

        public Word(string name)
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
    }
}
