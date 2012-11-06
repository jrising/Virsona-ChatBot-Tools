using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.ConceptNet
{
    public class Assertion
    {
        protected int id;
        protected Notion left;
        protected Associator relation;
        protected Notion right;
        protected int score;
        protected string sentence;

        public Assertion()
        {
        }
        
        public Assertion(Notion left, Associator relation, Notion right)
        {
            id = 0;
            this.left = left;
            this.relation = relation;
            this.right = right;
            score = 1;
            sentence = relation.Express(left.Canonical, right.Canonical);
        }

        public Assertion(int id, Notion left, Associator relation, Notion right, int score, string sentence)
        {
            this.id = id;
            this.left = left;
            this.relation = relation;
            this.right = right;
            this.score = score;
            this.sentence = sentence;
        }

        public Notion Left
        {
            get
            {
                return left;
            }
        }

        public Notion Right
        {
            get
            {
                return right;
            }
        }

        public Associator Type
        {
            get
            {
                return relation;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }
        }

        public string Sentence
        {
            get
            {
                return sentence;
            }
        }
    }
}
