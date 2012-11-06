using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.ConceptNet
{
    public class Notion
    {
        protected string lang;
        protected string text;

        protected int id;
        protected string normalized;
        protected int num_assertions;
        protected string canonical_name;

        // Avoid using this-- use ConceptNetLoader.GetConcept
        public Notion(string lang, string text)
        {
            this.lang = lang;
            this.text = text;
            this.canonical_name = text;
        }

        public string Canonical
        {
            get
            {
                return canonical_name;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }
    }
}
