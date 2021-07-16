using System;
using System.IO;
using System.Globalization;
using At.Matus.StatisticPod;
using Bev.Instruments.P9710;
using System.Reflection;

namespace optometer
{
    class Program
    {
        public static void Main(string[] args)
        {
            const string fatSeparator = "==========================================================";
            const string thinSeparator = "----------------------------------------------------------";
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var options = new Options();

            var streamWriter = new StreamWriter(options.LogFileName, true);
            var device = new P9710(options.Port);
            var stp = new StatisticPod("Statistics");

            DateTime timeStamp = DateTime.UtcNow;

            DisplayOnly("");
            LogOnly(fatSeparator);
            LogAndDisplay($"{Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
            LogAndDisplay($"StartTimeUTC: {timeStamp.ToString("dd-MM-yyyy HH:mm")}");
            LogAndDisplay($"Manufacturer: {device.InstrumentManufacturer}");
            LogAndDisplay($"InstrumentID: {device.InstrumentID}");
            LogAndDisplay($"Battery:      {device.InstrumentBatteryLevel} %");
            LogAndDisplay($"DetectorID:   {device.DetectorID}");
            LogAndDisplay($"Calibration:  {device.DetectorCalibrationFactor}");
            LogAndDisplay($"Unit:         {device.DetectorPhotometricUnit}");
            LogAndDisplay($"Samples (n):  {options.MaximumSamples}");
            LogAndDisplay($"Comment:      {options.UserComment}");
            LogOnly(fatSeparator);
            DisplayOnly("");

            DisplayOnly("press any key to start a measurement - 'q' to quit");

            while (Console.ReadKey(true).Key != ConsoleKey.Q)
            {
                DisplayOnly("");
                stp.Restart();
                timeStamp = DateTime.UtcNow;

                while (stp.SampleSize < options.MaximumSamples)
                {
                    double current = device.GetCurrent();
                    stp.Update(current);
                    Console.WriteLine($"{stp.SampleSize,4}:   {current * 1e9:F4} nA");
                }

                double stdDev = stp.StandardDeviation;
                double uCal = device.GetMeasurementUncertainty(stp.AverageValue);
                double u = Math.Sqrt(stdDev * stdDev + uCal * uCal);
                double sensitivity = device.DetectorCalibrationFactor;
                double photValue = stp.AverageValue / sensitivity;
                double photU = u / sensitivity;

                DisplayOnly("");
                LogOnly($"Sample started at:             {timeStamp:dd-MM-yyyy HH:mm:ss}");
                LogAndDisplay($"Average value:                 {stp.AverageValue * 1e9:F4} nA  ({device.EstimateMeasurementRange(stp.AverageValue)})");
                LogAndDisplay($"Standard deviation:            {stdDev * 1e9:F4} nA");
                LogAndDisplay($"Instrument uncertainty:        {uCal * 1e9:F4} nA");
                LogAndDisplay($"Combined standard uncertainty: {u * 1e9:F4} nA");
                LogAndDisplay($"Photometric value:             {photValue:F3} ({photU:F3}) {device.DetectorPhotometricUnit}");
                LogOnly(thinSeparator);
                DisplayOnly("");
                DisplayOnly("press any key to start a measurement - 'q' to quit");
            }
            DisplayOnly("bye.");
            LogOnly("");
            LogOnly(fatSeparator);
            LogOnly($"StopTimeUTC:  {timeStamp:dd-MM-yyyy HH:mm}");
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
