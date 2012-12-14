/******************************************************************\
 *      Class Name:     HandleWrapper
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * HandleWrapper can wrap any function and attempts to determine
 * its interface using System.Reflections methods.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;

namespace PluggerBase.ActionReaction.Actions
{
    public class HandlerWrapper : AgentBase, IAction
    {
        protected object obj;
        protected MethodInfo method;
        protected int time;

        public HandlerWrapper(object obj, MethodInfo method, int time)
        {
            this.obj = obj;
            this.method = method;
            this.time = time;
        }

        public MethodInfo Method
        {
            get
            {
                return method;
            }
        }

        public IArgumentType GetArgumentType(Type type)
        {
            if (type == typeof(string))
                return new StringArgumentType(int.MaxValue, ".+", "bufallo");

            return new UnknownArgumentType();
        }

        #region IAction Members

        public IArgumentType Input
        {
            get {
                ParameterInfo[] parameters = method.GetParameters();

                List<IArgumentType> types = new List<IArgumentType>();
                foreach (ParameterInfo parameter in parameters)
                    if (parameter.IsIn)
                        types.Add(new LabelledArgumentType(parameter.Name,
                            GetArgumentType(parameter.ParameterType)));

                if (types.Count == 1)
                    return types[0];
                else
                    return new SeveralArgumentType(types.ToArray());
            }
        }

        public IArgumentType Output
        {
            get
            {
                List<IArgumentType> types = new List<IArgumentType>();

                types.Add(GetArgumentType(method.ReturnParameter.ParameterType));

                ParameterInfo[] parameters = method.GetParameters();
                foreach (ParameterInfo parameter in parameters)
                    if (parameter.IsOut)
                        types.Add(GetArgumentType(parameter.ParameterType));

                if (types.Count == 1)
                    return types[0];
                else
                    return new SeveralArgumentType(types.ToArray());
            }
        }

        #endregion

        #region ICallable Members

        public bool Call(object value, IContinuation succ, IFailure fail)
        {
            try
            {
                object results = method.Invoke(obj, new object[] { value });
                return arena.Continue(succ, salience, results, fail);
            }
            catch (Exception ex)
            {
                return arena.Fail(fail, salience, ex.Message, succ);
            }
        }

        #endregion
    }
}
