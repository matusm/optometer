using System;
using System.IO;
using System.Globalization;
using At.Matus.StatisticPod;
using Bev.Instruments.P9710;
using System.Text;

namespace optometer
{
    class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // defaults
            int maximumSamples = 10;
            string logFileName = "optometer.log";
            string port = "COM9";

            // cinstants
            const string separator = "==========================================================";

            if (args.Length == 1 || args.Length == 2)
                port = args[0];
            if (args.Length == 2)
                maximumSamples = int.Parse(args[1]);

            var streamWriter = new StreamWriter(logFileName, true);
            var stringBuilder = new StringBuilder();
            var device = new P9710(port);
            var stp = new StatisticPod("Statistics");


            Console.WriteLine();
            Console.WriteLine($"Manufacturer: {device.InstrumentManufacturer}");
            Console.WriteLine($"InstrumentID: {device.InstrumentID}");
            Console.WriteLine($"Battery:      {device.InstrumentBatteryLevel} %");
            Console.WriteLine($"DetectorID:   {device.DetectorID}");
            Console.WriteLine($"Calibration:  {device.DetectorCalibrationFactor}");
            Console.WriteLine($"Unit:         {device.DetectorPhotometricUnit}");
            Console.WriteLine();

            while (stp.SampleSize < maximumSamples)
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


            Console.WriteLine();
            Console.WriteLine($"Average value:                 {stp.AverageValue * 1e9:F4} nA   ({device.EstimateMeasurementRange(stp.AverageValue)})");
            Console.WriteLine($"Standard deviation:            {stdDev * 1e9:F4} nA");
            Console.WriteLine($"Instrument uncertainty:        {uCal * 1e9:F4} nA");
            Console.WriteLine($"Combined standard uncertainty: {u * 1e9:F4} nA");
            Console.WriteLine($"Photometric value:             {photValue:F2} ({photU:F2}) {device.DetectorPhotometricUnit}");
            Console.WriteLine();
        }
    }
}
