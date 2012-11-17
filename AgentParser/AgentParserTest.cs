/******************************************************************\
 *      Class Name:     AgentParserTest
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A collection of tests for the various plugin actions
\******************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using PluggerBase;
using LanguageNet.Grammarian;
using ExamineTools;

namespace LanguageNet.AgentParser
{
	[TestFixture()]
	public class AgentParserTest : IMessageReceiver
	{
		[Test()]
		public void TestLongParse()
		{
			PluginEnvironment plugenv = new PluginEnvironment(this);
            plugenv.Initialize("/Users/jrising/projects/virsona/github/config.xml", new NameValueCollection());
			
			POSTagger tagger = new POSTagger(plugenv);
			
			string input = @"2. GENERAL POLICIES--IMMEDIATE AND LONG-RANGE

During the war, production for civilian use was limited by war needs and available manpower. Economic stabilization required measures to spread limited supplies equitably by rationing, price controls, increased taxes, savings bond campaigns, and credit controls. Now, with the surrender of our enemies, economic stabilization requires that policies be directed toward promoting an increase in supplies at low unit prices.
We must encourage the development of resources and enterprises in all parts of the country, particularly in underdeveloped areas. For example, the establishment of new peacetime industries in the Western States and in the South would, in my judgment, add to existing production and markets rather than merely bring about a shifting of production. I am asking the Secretaries of Agriculture, Commerce, and Labor to explore jointly methods for stimulating new industries, particularly in areas with surplus agricultural labor.
We must also aid small businessmen and particularly veterans who are competent to start their own businesses. The establishment and development of efficient small business ventures, I believe, will not take away from, but rather will add to, the total business of all enterprises.
Even with maximum encouragement of production, we cannot hope to remove scarcities within a short time. The most serious deficiencies will persist in the fields of residential housing, building materials, and consumers' durable goods. The critical situation makes continued rent control, price control, and priorities, allocations, and inventory controls absolutely essential. Continued control of consumer credit will help to reduce the pressure on prices of durable goods and will also prolong the period during which the backlog demand will be effective.
While we are meeting these immediate needs we must look forward to a long-range program of security and increased standard of living.
The best protection of purchasing power is a policy of full production and full employment opportunities. Obviously, an employed worker is a better customer than an unemployed worker. There always will be, however, some frictional unemployment. In the present period of transition we must deal with such temporary unemployment as results from the fact that demobilization will proceed faster than reconversion or industrial expansion. Such temporary unemployment is probably unavoidable in a period of rapid change. The unemployed worker is a victim of conditions beyond his control. He should be enabled to maintain a reasonable standard of living for himself and his family.
The most serious difficulty in the path of reconversion and expansion is the establishment of a fair wage structure.
The ability of labor and management to work together, and the wage and price policies which they develop, are social and economic issues of first importance.
Both labor and management have a special interest. Labor's interest is very direct and personal because working conditions, wages, and prices affect the very life and happiness of the worker and his family.
Management has a no less direct interest because on management rests the responsibility for conducting a growing and prosperous business.
But management and labor have identical interests in the long run. Good wages mean good markets. Good business means more jobs and better wages. In this age of cooperation and in our highly organized economy the problems of one very soon become the problems of all.
Better human relationships are an urgent need to which organized labor and management should address themselves. No government policy can make men understand each other, agree, and get along unless they conduct themselves in a way to foster mutual respect and good will.
The Government can, however, help to develop machinery which, with the backing of public opinion, will assist labor and management to resolve their disagreements in a peaceful manner and reduce the number and duration of strikes.
All of us realize that productivity--increased output per man--is in the long run the basis of our standard of living. Management especially must realize that if labor is to work wholeheartedly for an increase in production, workers must be given a just share of increased output in higher wages.
Most industries and most companies have adequate leeway within which to grant substantial wage increases. These increases will have a direct effect in increasing consumer demand to the high levels needed. Substantial wage increases are good business for business because they assure a large market for their products; substantial wage increases are good business for labor because they increase labor's standard of living; substantial wage increases are good business for the country as a whole because capacity production means an active, healthy, friendly citizenry enjoying the benefits of democracy under our free enterprise system.
Labor and management in many industries have been operating successfully under the Government's wage-price policy. Upward revisions of wage scales have been made in thousands of establishments throughout the Nation since VJ-day. It is estimated that about 6 million workers, or more than 20 percent of all employees in nonagricultural and nongovernmental establishments, have received wage increases since August 18, 1945. The amounts of increases given by individual employers concentrate between 10 and 15 percent, but range from less than 5 percent to over 30 percent.
The United States Conciliation Service since VJ-day has settled over 3,000 disputes affecting over 1,300,000 workers without a strike threat and has assisted in settling about 1,300 disputes where strikes were threatened which involved about 500,000 workers. Only workers directly involved, and not those in related industries who might have been indirectly affected, are included in these estimates.
Many of these adjustments have occurred in key industries and would have seemed to us major crises if they had not been settled peaceably.
Within the framework of the wage-price policy there has been definite success, and it is to be expected that this success will continue in a vast majority of the cases arising in the months ahead.
However, everyone who realizes the extreme need for a swift and orderly reconversion must feel a deep concern about the number of major strikes now in progress. If long continued, these strikes could put a heavy brake on our program.
I have already made recommendations to the Congress as to the procedure best adapted to meeting the threat of work stoppages in Nation-wide industries without sacrificing the fundamental rights of labor to bargain collectively and ultimately to strike in support of their position.
If we manage our economy properly, the future will see us on a level of production half again as high as anything we have ever accomplished in peacetime. Business can in the future pay higher wages and sell for lower prices than ever before. This is not true now for all companies, nor will it ever be true for all, but for business generally it is true.
We are relying on all concerned to develop, through collective bargaining, wage structures that are fair to labor, allow for necessary business incentives, and conform with a policy designed to ""hold the line"" on prices.
Production and more production was the byword during the war and still is during the transition from war to peace. However, when deferred demand slackens, we shall once again face the deflationary dangers which beset this and other countries during the 1930's. Prosperity can be assured only by a high level of demand supported by high current income; it cannot be sustained by deferred needs and use of accumulated savings.
If we take the right steps in time we can certainly avoid the disastrous excesses of runaway booms and headlong depressions. We must not let a year or two of prosperity lull us into a false feeling of security and a repetition of the mistakes of the 1920's that culminated in the crash of 1929.
During the year ahead the Government will be called upon to act in many important fields of economic policy from taxation and foreign trade to social security and housing. In every case there will be alternatives. We must choose the alternatives which will best measure up to our need for maintaining production and employment in the future. We must never lose sight of our long-term objectives: the broadening of markets--the maintenance of steadily rising demand. This demand can come from only three sources: consumers, businesses, or government.
In this country the job of production and distribution is in the hands of businessmen, farmers, workers, and professional people -- in the hands of our citizens. We want to keep it that way. However, it is the Government's responsibility to help business, labor, and farmers do their jobs.
There is no question in my mind that the Government, acting on behalf of all the people, must assume the ultimate responsibility for the economic health of the Nation. There is no other agency that can. No other organization has the scope or the authority, nor is any other agency accountable, to all the people. This does not mean that the Government has the sole responsibility, nor that it can do the job alone, nor that it can do the job directly.
All of the policies of the Federal Government must be geared to the objective of sustained full production and full employment--to raise consumer purchasing power and to encourage business investment. The programs we adopt this year and from now on will determine our ability to achieve our objectives. We must continue to pay particular attention to our fiscal, monetary, and tax policy, programs to aid business--especially small business--and transportation, labor-management relations and wage-price policy, social security and health, education, the farm program, public works, housing and resource development, and economic foreign policy.
For example, the kinds of tax measures we have at different times--whether we raise our revenue in a way to encourage consumer spending and business investment or to discourage it--have a vital bearing on this question. It is affected also by regular notions on consumer credit and by the money market, which is strongly influenced by the rate of interest on Government securities. It is affected by almost every step we take.
In short, the way we handle the proper functions of government, the way we time the exercise of our traditional and legitimate governmental functions, has a vital bearing on the economic health of the Nation.
These policies are discussed in greater detail in the accompanying Fifth Quarterly Report of the Director of War Mobilization and Reconversion.";
			
			Console.WriteLine(Profiler.AnnounceEach());
			List<KeyValuePair<string, string>> tokens = tagger.TagString(input);
			Sentence sentence = new Sentence(tokens);
			Profiler timer = new Profiler();
			IParsedPhrase parsed = sentence.Parse();
			Console.WriteLine(parsed.ToString());
			Console.WriteLine("Time: " + timer.GetTime());
		}
		
		public bool Receive(string message, object reference) {
			Console.WriteLine(message);
			return true;
		}
	}
}
