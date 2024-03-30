using System;
using System.IO;

namespace VMFGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            #if DEBUG
                args = new string[] { @"C:\PS\visual_studio\surf-map-gen\Output"};
            #endif

            string directory = "";
            if(args.Length > 0)
            {
                if(Directory.Exists(args[0]))
                {
                    directory = args[0];
                }
                else
                {
                    Console.WriteLine("No Way Jose");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Missing Arguments: [1] Output File Destination Directory, [2] Input File Path");
                return;
            }

            if(string.IsNullOrEmpty(directory))
            {
                Console.WriteLine("Invalid Arguments");
                return;
            }

            string vmfPath = directory + @"\GeneratedMap.vmf";

            if(File.Exists(vmfPath))
            {
                // move and increment name by 1
                File.Move(vmfPath, directory + @"\GeneratedMap" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".vmf");
            }

            File.WriteAllText(vmfPath, SurfController.GenerateSurfMap());

            //Thanks to Pikapi for reminding me to add this
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
