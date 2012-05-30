using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue.Console
{
    internal class Arguments
    {
        public String Error { get; set; }
        public IEnumerable<String> InFilenames { get; set; }
        public String OutFilename { get; set; }

        private enum SwitchType
        {
            None,
            OutFilename,
            Unknown
        }

        private enum ParseState
        {
            Start,
            Infiles,
            ExpectOutfile,
            GotOutFile
        }

        public static Arguments Parse(IEnumerable<String> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            var infiles = new List<string>();
            var state = ParseState.Start; 
            string outfile = null;

            foreach (var arg in args)
            {
                var switchType = ToSwitchType(arg);
                switch (state)
                {
                    case ParseState.Start:
                        switch (switchType)
                        {
                            case SwitchType.None:
                                infiles.Add(arg);
                                state = ParseState.Infiles;
                                break;
                            case SwitchType.OutFilename:
                            case SwitchType.Unknown:
                                return UsageHelp();
                            default:
                                throw new InvalidOperationException("Unknown SwitchType: " + switchType.ToString());
                        }
                        break;
                    case ParseState.Infiles:
                        switch (switchType)
                        {
                            case SwitchType.None:
                                infiles.Add(arg);
                                break;
                            case SwitchType.OutFilename:
                                state = ParseState.ExpectOutfile;
                                break;
                            case SwitchType.Unknown:
                                return UsageHelp();
                            default:
                                throw new InvalidOperationException("Unknown SwitchType: " + switchType.ToString());
                        }
                        break;
                    case ParseState.ExpectOutfile:
                        switch (switchType)
                        {
                            case SwitchType.None:
                                outfile = arg;
                                state = ParseState.GotOutFile;
                                break;
                            case SwitchType.OutFilename:
                            case SwitchType.Unknown:
                                return UsageHelp();
                            default:
                                throw new InvalidOperationException("Unknown SwitchType: " + switchType.ToString());
                        }
                        break;
                    case ParseState.GotOutFile:
                        // "Extra" params -- error
                        return UsageHelp();
                    default:
                        throw new InvalidOperationException("Unknown state: " + state.ToString());
                }
            }

            if (state == ParseState.Start)
            {
                // no params received
                return UsageHelp();
            }

            return new Arguments
                       {
                           InFilenames = infiles,
                           OutFilename = outfile
                       };
        }

        public static Arguments UsageHelp()
        {
            return new Arguments { Error = Properties.Resources.UsageHelp };
        }

        private static SwitchType ToSwitchType(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException("argument");
            }

            if (argument[0] != '-')
            {
                return SwitchType.None; ;
            }

            if ("-outfile".StartsWith(argument))
            {
                return SwitchType.OutFilename;
            }

            return SwitchType.Unknown;
        }
    }
}
