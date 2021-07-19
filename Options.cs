using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace optometer
{
    public class Options
    {
        [Option('p', "port", DefaultValue = "COM9", HelpText = "Serial port name.")]
        public string Port { get; set; }

        [Option('n', "number", DefaultValue = 10, HelpText = "Number of samples.")]
        public int MaximumSamples { get; set; }

        [Option("comment", DefaultValue = "---", HelpText = "User supplied comment string.")]
        public string UserComment { get; set; }

        [Option("logfile", DefaultValue = "optometer.log", HelpText = "Log file name.")]
        public string LogFileName { get; set; }

        [ValueList(typeof(List<string>), MaximumElements = 2)]
        public IList<string> ListOfFileNames { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string AppVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            HelpText help = new HelpText
            {
                Heading = new HeadingInfo($"{AppName}, version {AppVer}"),
                Copyright = new CopyrightInfo("Michael Matus", 2021),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };
            string preamble = "Program to operate the photo-current meter P-9710 (Gigahertz-Optik). It is controlled via its RS232 interface. " +
                "Measurement results are logged in a file.";
            help.AddPreOptionsLine(preamble);
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine($"Usage: {AppName} [options]");
            help.AddPostOptionsLine("");
            help.AddOptions(this);

            return help;
        }

    }
}

