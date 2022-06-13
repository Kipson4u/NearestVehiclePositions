using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace c
{
    internal class Program
    {       
        static void Main(string[] args)
        {           
            string fileName = $"{Directory.GetParent(typeof(Program).Assembly.Location).FullName}\\VehiclePositions.dat";

            List<VehiclesPosition> vehicleList = new List<VehiclesPosition>();
            List<VehiclesPosition> nearestVehicleList = new List<VehiclesPosition>();

            Stopwatch stopwatchLoad = new Stopwatch();

            if (File.Exists(fileName))
            {
                //Load file
                stopwatchLoad.Start();       
                vehicleList = ReadVehiclesPosition(fileName);
                stopwatchLoad.Stop();
                
                // search the nearest car for each position
                Stopwatch stopwatchCalc = new Stopwatch();
                stopwatchCalc.Start();
                nearestVehicleList = GetNearestVehicleList(vehicleList);
                stopwatchCalc.Stop();

                Console.WriteLine("Data file execution time: {0} ms", stopwatchLoad.ElapsedMilliseconds);
                Console.WriteLine("Closest position calculation execution time: {0} ms", stopwatchCalc.ElapsedMilliseconds);
                Console.WriteLine("Total execution time: {0} ms", stopwatchLoad.ElapsedMilliseconds + stopwatchCalc.ElapsedMilliseconds);
                Console.WriteLine();

                //print results
                Console.WriteLine("The nearest cars for ten positions:");

                int i = 1;
                
                foreach (var vehicle in nearestVehicleList)
                {
                    Console.WriteLine($"position = {i++}, the nearest vehicle ID = {vehicle.PositionId}, Latitude= {vehicle.Position.Latitude}, Longitude = {vehicle.Position.Longitude}");
                }
            }
            else
            {
                Console.WriteLine($"can not find file: {fileName}");
            }

            Console.ReadLine();
        }

        static  List<VehiclesPosition> ReadVehiclesPosition(string filePath)
        {
            List<VehiclesPosition> vehicleList = new List<VehiclesPosition>();
           
            if (File.Exists(filePath))
            {
                using (var stream = File.OpenRead(filePath))
                {
                    using (var reader = new BinaryReader(stream, Encoding.ASCII, false))
                    {
                        while (reader.PeekChar() !=-1)
                        {
                            var vehicle = new VehiclesPosition
                            (
                                reader.ReadInt32(),
                                reader.ReadChars(10).ToString(),
                                new GeoPosition(reader.ReadSingle(), reader.ReadSingle()),
                                reader.ReadUInt64()
                            );

                            vehicleList.Add(vehicle);
                        }
                    }
                }                
            }

            return vehicleList;
        }

        static List<VehiclesPosition> GetNearestVehicleList(List<VehiclesPosition> vehiclesPositions)
        {
            List<VehiclesPosition> nearestVehicleList = new List<VehiclesPosition>();
            VehiclesPosition vehiclePosition = default;

            float closeDistance= 0;

            foreach (var position in GetPositions())
            {
                foreach (var vehicle in vehiclesPositions)
                {                   
                    float latitudeDifference = (position.Latitude - vehicle.Position.Latitude);
                    float longitudeDifference = (position.Longitude - vehicle.Position.Longitude);

                    float latPow = latitudeDifference * latitudeDifference;                    
                    float lonPow = longitudeDifference * longitudeDifference;

                    //distance*distance = ((x2 − x1)*(x2 - x1)) + ((y2 − y1)*(y2 - y1))
                    float distance = latPow + lonPow;
                    
                    if (closeDistance == 0 || (distance) < closeDistance)
                    {
                        closeDistance = distance;
                        vehiclePosition = vehicle;
                    }
                }

                nearestVehicleList.Add(vehiclePosition);
                closeDistance = 0;
            }

            return nearestVehicleList;
        }
        static List<GeoPosition> GetPositions()
        {
            List<GeoPosition> geoPositions = new List<GeoPosition>()
            {
                new GeoPosition(34.544909f, -102.100843f),
                new GeoPosition(32.345544f, -99.123124f),
                new GeoPosition(33.234235f, -99.123124f),
                new GeoPosition(35.195739f, -95.348899f),
                new GeoPosition(31.895839f, -97.789573f),
                new GeoPosition(32.895839f, -101.789573f),
                new GeoPosition(34.115839f, -100.225732f),
                new GeoPosition(32.335839f, -99.992232f),
                new GeoPosition(33.535339f, -94.792232f),
                new GeoPosition(32.234235f, -100.222222f)
            };

            return geoPositions;
        }
    }

    public sealed class VehiclesPosition
    {
        public int PositionId { get; set; }
        public string VehicleRegistration { get; set; }       
        public GeoPosition Position{ get ;set; }
        public ulong RecordedTimeUTC { get; set; }

        public VehiclesPosition(int PositionId, string VehicleRegistration, GeoPosition Position, ulong RecordedTimeUTC)
        {
            this.PositionId = PositionId;
            this.Position = Position;
            this.VehicleRegistration = VehicleRegistration;
            this.RecordedTimeUTC = RecordedTimeUTC;
        }
    }

    public sealed class GeoPosition
    {               
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public GeoPosition (float Latitude, float Longitude)
        {
                this.Latitude = Latitude;
                this.Longitude = Longitude;
        }
    }
}
