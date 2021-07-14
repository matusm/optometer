using System;
using Bev.Instruments.P9710;
using At.Matus.StatisticPod;
using System.IO;

namespace optometer
{
    class Program
    {
        static void Main(string[] args)
        {
            // defaults
            int maximumSamples = 10;
            string port = "COM5";

            if (args.Length == 1 || args.Length == 2)
                port = args[0];

            if (args.Length == 2)
                maximumSamples = int.Parse(args[1]);

            var device = new P9710(port);
            var stp = new StatisticPod("Statistics");

            Console.WriteLine();
            Console.WriteLine($"Manufacturer: {device.InstrumentManufacturer}");
            Console.WriteLine($"InstrumentID: {device.InstrumentID}");
            Console.WriteLine($"Battery:      {device.InstrumentBatteryLevel} %");
            Console.WriteLine();
            Console.WriteLine($"DetectorID:   {device.DetectorID}");
            Console.WriteLine($"Calibration:  {device.DetectorCalibrationFactor}");
            Console.WriteLine($"Unit:         {device.PhotometricUnit}");
            Console.WriteLine();


            StreamWriter sw = new StreamWriter("Dump01.txt", true);
            sw.WriteLine($"InstrumentID: {device.InstrumentID}");
            sw.WriteLine($"DetectorID:   {device.DetectorID}");
            sw.WriteLine();

            device.WriteMagicString();
            device.WriteSecretString("BEV UMBB-300 2021    ");
            device.Query("RA9957");


            //for (int i = 9999; i >= 0; i--)
            //{
            //    Prompt($"RA{i}");
            //    string stAnswer = device.Query("ST");
            //    string line = $"{i,4} -> '{stAnswer}'";
            //    Console.WriteLine(line);
            //    sw.WriteLine(line);
            //    sw.Flush();
            //}
            sw.Close();



            //            device.WriteMagicString();
            //            device.WriteSecretString("GO2000            ");

            byte[] bytes = {
            0x50,
            0x54,
            0x39,
            0x36,
            0x31,
            0x30,
            0xB4,
            0x11,
            0x00,
            0x00,
            0x00,
            0xFF,
            0xFF,
            0xFF,
            0x00,
            0x00,
            0x47,
            0x4F,
            0x32,
            0x30,
            0x30,
            0x30,
            0x20,
            0x20,
            0x20,
            0x20,
            0x20,
            0x20,
            0x20,
            0x20,
            0x20,
            0x20,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x56,
            0x4C,
            0x8B,
            0xD1,
            0x06,
            0x0F,
            0x30,
            0x31,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00
            
            };

            device.WriteBytesToDetectorData(bytes, 0);

            device.WriteSecretString("BEV UMBB-300         ");

            Prompt("SE64");


            Console.WriteLine($"DetectorID:   {device.DetectorID}");
            Console.WriteLine($"Calibration:  {device.DetectorCalibrationFactor}");
            Console.WriteLine($"Unit:         {device.PhotometricUnit}");

            while (stp.SampleSize < maximumSamples)
            {
                double current = device.GetDetectorCurrent();
                stp.Update(current);
                Console.WriteLine($"{stp.SampleSize,4}:   {current * 1e9:F4} nA");
            }

            double stdDev = stp.StandardDeviation;
            double uCal = device.GetMeasurementUncertainty(stp.AverageValue);
            double u = Math.Sqrt(stdDev * stdDev + uCal * uCal);

            Console.WriteLine();
            Console.WriteLine($"Average value:                 {stp.AverageValue * 1e9:F4} nA   ({device.EstimateMeasurementRange(stp.AverageValue)})");
            Console.WriteLine($"Standard deviation:            {stdDev * 1e9:F4} nA");
            Console.WriteLine($"Instrument uncertainty:        {uCal * 1e9:F4} nA");
            Console.WriteLine($"Combined standard uncertainty: {u * 1e9:F4} nA");
            Console.WriteLine();


            void Prompt(string command)
            {
                Console.WriteLine($"{command} -> {device.Query(command)}");
            }

        }
    }
}
