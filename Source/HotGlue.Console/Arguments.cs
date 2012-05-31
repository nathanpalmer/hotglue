using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue.Console
{
    internal class Arguments
    {
        public String Error { get; set; }
        public String InFilename { get; set; }
        public String OutFilename { get; set; }
        public String ScriptPath { get { return "Scripts"; } } // this could be configurable in the future. 
        public String SharedFolderName { get { return "shared"; } } // this could be configurable in the future. 


        private enum SwitchType
        {
            None,
            OutFilename,
            Unknown
        }

        private enum ParseState
        {
            Start,
            GotInfile,
            ExpectOutfile,
            GotOutFile
        }

        public static Arguments Parse(IEnumerable<String> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            string infile = null;
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
                                infile = arg;
                                state = ParseState.GotInfile;
                                break;
                            case SwitchType.OutFilename:
                            case SwitchType.Unknown:
                                return UsageHelp();
                            default:
                                throw new InvalidOperationException("Unknown SwitchType: " + switchType.ToString());
                        }
                        break;
                    case ParseState.GotInfile:
                        switch (switchType)
                        {
                            case SwitchType.None:
                                return UsageHelp();
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
                           InFilename = infile,
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
