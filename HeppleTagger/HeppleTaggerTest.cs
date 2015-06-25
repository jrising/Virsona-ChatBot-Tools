/******************************************************************\
 *      Class Name:     HeppleTaggerTest
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A collection of tests for the various plugin actions
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PluggerBase;
using LanguageNet.Grammarian;

namespace HeppleTagger
{
    [TestFixture]
    public class HeppleTaggerTest : IMessageReceiver
    {
        protected PluginEnvironment plugenv;

        public HeppleTaggerTest()
        {
            plugenv = new PluginEnvironment(this);

            string plugbase = "C:\\Documents and Settings\\All Users\\Application Data\\Virsona\\plugins\\";
            plugenv.Initialize(plugbase + "config.xml", null);
        }

        [Test]
        public void StringTagTest()
        {
            object result = plugenv.ImmediateConvertTo("This is a test.",
                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 1, 1000);
            Assert.IsInstanceOfType(typeof(IEnumerable<string>), result);

            CollectionAssert.AreEqual(new string[] { "DT", "VBZ", "DT", "NN", "." }, (IEnumerable<string>)result);
        }

        [Test]
        public void EnumerableTagTest()
        {
            object result = plugenv.ImmediateConvertTo(new string[] { "How", "did", "I", "do", " ?" },
                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 1, 1000);
            Assert.IsInstanceOfType(typeof(IEnumerable<string>), result);

            CollectionAssert.AreEqual(new string[] { "WRB", "VBD", "PRP", "VB", "." }, (IEnumerable<string>)result);
        }

        [Test]
        public void PhraseResolveTest()
        {
            // Note that Mary is intentionally mis-tagged, and should not change.
            IParsedPhrase[] phrases = new IParsedPhrase[] {
                new WordPhrase("Mary", "JJ"), new GroupPhrase("VP", new IParsedPhrase[] {
                    new WordPhrase("had", "VBD"), new GroupPhrase("NP", new IParsedPhrase[] {
                        new WordPhrase("a", "DT"), new GroupPhrase("NP", new IParsedPhrase[] {
                            new WordPhrase("little", "??"), new WordPhrase("lamb", "NN")})})})};

            object result = plugenv.ImmediateConvertTo(phrases,
                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 1, 1000);
            Assert.IsInstanceOfType(typeof(IEnumerable<string>), result);

            CollectionAssert.AreEqual(new string[] { "JJ", "VBD", "DT", "JJ", "NN" }, (IEnumerable<string>)result);
        }

        [Test]
        public void PartsSourceTest()
        {
            IDataSource<string, string[]> result = plugenv.GetDataSource<string, string[]>(
                LanguageNet.Grammarian.POSTagger.PartsSourceName);
            Assert.IsNotNull(result);

            string[] types = null;
            Assert.IsTrue(result.TryGetValue("police", out types));
            CollectionAssert.AreEqual(new string[] { "NN", "NNS", "VB" }, types);
        }

        #region IMessageReceiver Members

        public bool Receive(string message, object reference)
        {
            // Print it out, whatever it is!
            Console.WriteLine(message);
            return true;
        }

        #endregion
    }
}
