using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.Codeland;
using DataTemple.Matching;

// An environment is a sequence of words and rules--
//   some words can be variables, which can be looked up in the rules
namespace DataTemple.AgentEvaluate
{
    public class Context
    {
        protected Context parent;
        
        protected Coderack coderack;
        protected double weight;

        protected List<IContent> contents;
        protected Dictionary<string, object> map;
        protected List<Codelet> sequence;   // the sequence to this point

        public Context(Coderack coderack)
        {
            parent = null;

            weight = 1.0;
            this.coderack = coderack;
            
            contents = new List<IContent>();
            map = new Dictionary<string, object>();
            sequence = new List<Codelet>();
        }

        public Context(Context parent, List<IContent> contents)
            : this(parent, contents, parent.weight)
        {
        }

        public Context(Context parent, List<IContent> contents, double weight)
        {
            this.parent = parent;

            this.weight = weight;
            this.coderack = parent.coderack;

            if (contents == null)
                contents = new List<IContent>();
            this.contents = contents;
            map = new Dictionary<string, object>();
            sequence = new List<Codelet>();
        }

        public int Size
        {
            get
            {
                return 4 + map.Count * 8 + contents.Count * 8 + sequence.Count * 4;
            }
        }

        public Coderack Coderack
        {
            get
            {
                return coderack;
            }
        }

        public double Weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }

        public List<IContent> Contents
        {
            get
            {
                return contents;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return contents == null || contents.Count == 0;
            }
        }

        public Dictionary<string, object> Map
        {
            get
            {
                return map;
            }
        }

        public Context Parent
        {
            get
            {
                return parent;
            }
        }

        public List<Codelet> FullSequence
        {
            get
            {
                List<Codelet> result = new List<Codelet>();
                for (Context check = this; check != null; check = check.parent)
                    result.InsertRange(0, check.sequence);
                return result;
            }
        }

        public Dictionary<string, object> AllMap()
        {
            Dictionary<string, object> allmap = new Dictionary<string,object>();
            for (Context append = this; append != null; append = append.parent)
                foreach (KeyValuePair<string, object> kvp in append.map)
                    if (!allmap.ContainsKey(kvp.Key))
                        allmap.Add(kvp.Key, kvp.Value);

            return allmap;
        }

        public void AddToSequence(Codelet codelet)
        {
            sequence.Add(codelet);
        }

        public void AddMappings(Context mappings)
        {
            // Add in the mappings, up until the common parent
            for (Context append = mappings; append != null; append = append.parent)
            {
                // Is this a common element?
                for (Context look = parent; look != null; look = look.parent)
                    if (look == append)
                        return; // we're all done!

                foreach (KeyValuePair<string, object> kvp in append.map)
	                if (!map.ContainsKey(kvp.Key))
	                    map.Add(kvp.Key, kvp.Value);
            }
        }

        public void Unset(string name)
        {
            for (Context context = this; context != null; context = context.parent)
                context.map.Remove(name);
        }

        public object LookupSimple(string name)
        {
            object result = null;
            if (map.TryGetValue(name, out result))
                return result;

            if (parent == null)
                throw new Exception(name + " not found");

            return parent.LookupSimple(name);
        }

        // Aware of suffixed digits
        public object Lookup(string name)
        {
            // First try the whole thing
            object value = LookupDefaulted<object>(name, null);
            if (value != null)
                return value;

            if (!name.StartsWith("%"))
                throw new Exception(name + " not found");
			
            // Does this have suffixes?
			string[] parts = name.Split(':');
			if (parts.Length == 1)
                throw new Exception(name + " not found");
			
            string front = parts[0];
            Variable variable = LookupDefaulted<Variable>(front, null);
            if (variable == null)
                throw new Exception(front + " not found");
			
			StringBuilder declinedName = new StringBuilder();
			declinedName.Append(front);
			for (int ii = 1; ii < parts.Length; ii++) {
	            IDeclination declination = LookupDefaulted<IDeclination>(":" + parts[ii], null);
	            if (declination == null)
	                throw new Exception(":" + parts[ii] + " not found");
			
				declinedName.Append(":" + parts[ii]);
				variable = new DeclinedVariable(declinedName.ToString(), variable, declination);
			}
			
			return variable;
        }

        public T LookupDefaulted<T>(string name, T defval)
        {
            object result = null;
            if (map.TryGetValue(name, out result))
                return (T) result;

            if (parent == null)
                return defval;

            return parent.LookupDefaulted<T>(name, defval);
        }

        public T LookupAndAdd<T>(string name, T defval)
        {
            T before = LookupDefaulted<T>(name, defval);
            map[name] = before;

            return before;
        }
		
		public Context ChildRange(int startsat) {
			if (contents.Count > startsat)
				return new Context(this, contents.GetRange(startsat, contents.Count - startsat));
			return new Context(this, new List<IContent>());
		}
		
        public string ContentsCode()
        {
            StringBuilder result = new StringBuilder();
            foreach (IContent content in contents)
            {
                string name = content.Name;

                if (name[0] == ' ')
                    result.Append(name.Substring(1));
                else
                {
                    if (result.Length > 0)
                        result.Append(" ");
                    result.Append(name);
                }
            }

            return result.ToString();
        }
    }
}
