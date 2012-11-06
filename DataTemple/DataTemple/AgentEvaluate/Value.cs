using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.AgentEvaluate
{
    public class Value : IContent
    {
        protected object data;

        public Value(object data)
        {
            this.data = data;
        }

        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        #region IContent Members

        public string Name
        {
            get
            {
                return data.ToString();
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }
}
