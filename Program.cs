using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using At.Matus.StatisticPod;
using Bev.Instruments.P9710;

namespace optometer
{
    class Program
    {
        readonly static string fatSeparator = new string('=', 80);
        readonly static string thinSeparator = new string('-', 80);

        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            DateTime timeStamp = DateTime.UtcNow;
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string appVersionString = $"{appVersion.Major}.{appVersion.Minor}";

            Options options = new Options();
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
                Console.WriteLine("*** ParseArgumentsStrict returned false");

            var streamWriter = new StreamWriter(options.LogFileName, true);
            var device = new P9710(options.Port);
            var stp = new StatisticPod("Statistics");

            if (options.MaximumSamples < 2) options.MaximumSamples = 2;

            DisplayOnly("");
            LogOnly(fatSeparator);
            DisplayOnly($"Application:  {appName} {appVersionString}");
            LogOnly($"Application:  {appName} {appVersion}");
            LogAndDisplay($"StartTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}"); 
            LogAndDisplay($"InstrumentID: {device.InstrumentManufacturer} {device.InstrumentID}");
            LogAndDisplay($"Battery:      {device.InstrumentBatteryLevel} %");
            LogAndDisplay($"DetectorID:   {device.DetectorID}");
            LogAndDisplay($"Calibration:  {device.DetectorCalibrationFactor} A/({device.DetectorPhotometricUnit})");
            LogAndDisplay($"Samples (n):  {options.MaximumSamples}");
            LogAndDisplay($"Comment:      {options.UserComment}");
            LogOnly(fatSeparator);
            DisplayOnly("");

            DisplayOnly("press any key to start a measurement - 'q' to quit");

            int measurementIndex = 0;

            while (Console.ReadKey(true).Key != ConsoleKey.Q)
            {
                measurementIndex++;
                DisplayOnly("");
                DisplayOnly($"Measurement #{measurementIndex}");
                stp.Restart();
                timeStamp = DateTime.UtcNow;

                while (stp.SampleSize < options.MaximumSamples)
                {
                    double current = device.GetCurrent();
                    stp.Update(current);
                    DisplayOnly($"{stp.SampleSize,4}:   {current * 1e9:F4} nA");
                }

                double stdDev = stp.StandardDeviation;
                double uSpecs = device.GetMeasurementUncertainty(stp.AverageValue);
                double uCombined = Math.Sqrt(stdDev * stdDev + uSpecs * uSpecs);
                double sensitivity = device.DetectorCalibrationFactor;
                double photValue = stp.AverageValue / sensitivity;
                double photU = uCombined / sensitivity;

                DisplayOnly("");
                LogOnly($"Measurement number:            {measurementIndex}");
                LogOnly($"Triggered at:                  {timeStamp:dd-MM-yyyy HH:mm:ss}");
                LogAndDisplay($"Average value:                 {stp.AverageValue * 1e9:F4} nA  ({device.EstimateMeasurementRange(stp.AverageValue)})");
                LogAndDisplay($"Standard deviation:            {stdDev * 1e9:F4} nA");
                LogAndDisplay($"Specification uncertainty:     {uSpecs * 1e9:F4} nA");
                LogAndDisplay($"Combined standard uncertainty: {uCombined * 1e9:F4} nA");
                LogAndDisplay($"Photometric value:             {photValue:F3} ± {photU:F3} {device.DetectorPhotometricUnit}");
                LogOnly(thinSeparator);
                DisplayOnly("");
                DisplayOnly("press any key to start a measurement - 'q' to quit");
            }

            DisplayOnly("bye.");
            LogOnly("");
            LogOnly(fatSeparator);
            if (measurementIndex == 1)
                LogOnly($"{measurementIndex} measurement logged - StopTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            else
                LogOnly($"{measurementIndex} measurements logged - StopTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            LogOnly(fatSeparator);
            LogOnly("");

            streamWriter.Close();

            /***************************************************/
            void LogAndDisplay(string line)
            {
                DisplayOnly(line);
                LogOnly(line);
            }
            /***************************************************/
            void LogOnly(string line)
            {
                streamWriter.WriteLine(line);
                streamWriter.Flush();
            }
            /***************************************************/
            void DisplayOnly(string line)
            {
                Console.WriteLine(line);
            }
            /***************************************************/

        }
    }
}
