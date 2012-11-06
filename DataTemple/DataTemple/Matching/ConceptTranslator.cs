using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace DataTemple.Matching
{
    public class ConceptTranslator
    {
        protected Memory memory;
        protected bool firstsecond;

        public ConceptTranslator(Memory memory, bool firstsecond)
        {
            this.memory = memory;
            this.firstsecond = firstsecond;
        }

        public Concept GetConcept(IParsedPhrase phrase)
        {
            string name = Nouns.AsSubject(phrase.Text);

            if (firstsecond)
            {
                if (name == "I")
                    return memory.you;
                else if (name == "you")
                    return memory.self;
            }
            else
            {
                if (name == "I")
                    return memory.self;
                else if (name == "you")
                    return memory.you;
            }

            return memory.NewConcept(phrase);
        }
    }
}
