using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Msv.AutoMiner.Common;

namespace Msv.AutoMiner.Rig.Commands
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private readonly Dictionary<string, MethodInfo> m_Actions;

        private readonly ICommandProcessor m_Processor;

        public CommandInterpreter(ICommandProcessor processor)
        {
            m_Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            m_Actions = GetMethodAttributes()
                .ToDictionary(x => x.attribute.Action, x => x.method);
        }

        public bool Interpret(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (!args.Any())
                return false;
            try
            {
                return InterpretInternal(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex);
                return true;
            }
        }

        private bool InterpretInternal(string[] args)
        {
            var command = args[0].ToLowerInvariant();
            if (command == "-h")
            {
                ShowHelp();
                return true;
            }
            var action = m_Actions.TryGetValue(command);
            if (action == null)
            {
                Console.WriteLine("Unknown command - {0}", command);
                ShowHelp();
                return true;
            }
            var commandParameters = args.Skip(1).ToArray();
            var actionParameterValues = GetParameterAttributes(action)
                .Select(x => (x.parameter, index: Array.IndexOf(commandParameters, x.attribute.Name), isRequired: x.attribute.IsRequired))
                .Select(x =>
                {
                    if (x.index < 0)
                        return (x.parameter, value: null, error: x.isRequired ? $"Parameter '{x.parameter.Name}' not specified" : null);
                    if (x.parameter.ParameterType == typeof(bool))
                        return (x.parameter, value: true, error: null);
                    if (commandParameters.Length <= x.index + 1)
                        return (x.parameter, value: (object) null, error: "Invalid parameters count");
                    var parameterValue = commandParameters[x.index + 1];
                    if (x.parameter.ParameterType.IsArray)
                    {
                        var arrayMembers = parameterValue.Split(',');
                        // ReSharper disable once AssignNullToNotNullAttribute
                        var array = Array.CreateInstance(
                            x.parameter.ParameterType.GetElementType(), arrayMembers.Length);
                        for (var i = 0; i < array.Length; i++)
                            array.SetValue(ParseParameter(arrayMembers[i], x.parameter.ParameterType.GetElementType()), i);
                        return (x.parameter, value: array, error: null);
                    }
                    return (x.parameter, value: ParseParameter(parameterValue, x.parameter.ParameterType), error: null);
                })
                .OrderBy(x => x.parameter.Position)
                .ToArray();
            if (actionParameterValues.Any(x => x.error != null))
            {
                Console.WriteLine(string.Join(Environment.NewLine, actionParameterValues.Where(x => x.error != null).Select(x => x.error)));
                return true;
            }

            action.Invoke(m_Processor, actionParameterValues
                .Select(x => x.value ?? (x.parameter.ParameterType.IsValueType
                                 ? Activator.CreateInstance(x.parameter.ParameterType)
                                 : null))
                .ToArray());
            return true;
        }

        private static void ShowHelp()
        {
            var builder = new StringBuilder("Usage:")
                .AppendLine().AppendLine()
                .AppendLine("-h - show this help")
                .AppendLine();
            GetMethodAttributes()
                .ForEach(x =>
                {
                    if (!x.method.GetParameters().Any())
                    {
                        builder.AppendFormat("{0} - {1}",
                            x.attribute.Action,
                            x.attribute.Description).AppendLine().AppendLine();
                        return;
                    }
                    var parameterAttributes = GetParameterAttributes(x.method)
                        .Where(y => y.attribute != null)
                        .ToArray();
                    builder.AppendFormat("{0} {1} - {2}",
                            x.attribute.Action,
                            string.Join(" ", parameterAttributes.Select(y => $"{y.attribute.Name} <{y.parameter.Name}>")),
                            x.attribute.Description)
                        .AppendLine()
                        .AppendLine(" Parameters:");
                    parameterAttributes.ForEach(
                        y => builder.AppendFormat(
                                "   {0} <{1}> - {2}", y.attribute.Name, y.parameter.Name, y.attribute.Description)
                            .AppendLine());
                    builder.AppendLine(" Example:").AppendFormat("   {0} {1}", x.attribute.Action,
                        string.Join(" ",
                            parameterAttributes.SelectMany(y => new[] {y.attribute.Name, y.attribute.Example})))
                            .AppendLine().AppendLine();
                });
            Console.WriteLine(builder);
        }

        private static object ParseParameter(string str, Type targetType) 
            => Convert.ChangeType(str, Nullable.GetUnderlyingType(targetType) ?? targetType);

        private static IEnumerable<(MethodInfo method, CommandActionAttribute attribute)> GetMethodAttributes()
            => typeof(ICommandProcessor)
                .GetMethods()
                .Select(x => (method: x, attribute: x.GetCustomAttribute<CommandActionAttribute>()))
                .Where(x => x.attribute != null);

        private static (ParameterInfo parameter, CommandParameterAttribute attribute)[] GetParameterAttributes(MethodInfo method)
            => method.GetParameters()
                .Select(x => (parameter: x, attribute: x.GetCustomAttribute<CommandParameterAttribute>()))
                .ToArray();
    }
}
