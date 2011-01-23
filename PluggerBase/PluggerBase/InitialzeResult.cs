/******************************************************************\
 *      Class Name:     InitializeResult
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * The return value of IPlugin.Initialize(), to describe errors
 *   or dependency failures
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase
{
    public class InitializeResult
    {
        public enum DependencyType
        {
            ActionDependency,
            SourceDependency,
            PluginDependency,
            RepeatAttempt
        }

        protected string message;
        protected DependencyType? dependency;

        protected InitializeResult(string message, DependencyType? dependency)
        {
            this.message = message;
            this.dependency = dependency;
        }

        public bool IsSuccess
        {
            get
            {
                return message == null;
            }
        }

        public bool IsTryAgain
        {
            get
            {
                return message != null && dependency.HasValue;
            }
        }

        public static InitializeResult Success()
        {
            return new InitializeResult(null, null);
        }

        public static InitializeResult Failure(string reason)
        {
            return new InitializeResult(reason, null);
        }

        public static InitializeResult TryAgain(string reason)
        {
            return new InitializeResult(reason, DependencyType.RepeatAttempt);
        }

        public static InitializeResult DependencyDelay(string dependency, DependencyType type)
        {
            return new InitializeResult(dependency, type);
        }
    }
}
