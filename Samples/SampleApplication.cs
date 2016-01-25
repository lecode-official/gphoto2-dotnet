
#region Using Directives

using System;
using System.Collections.Generic;
using System.Devices;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#endregion

namespace SamplesApplication
{
    /// <summary>
    /// Represents an application, which makes it possible for users to execute some gPhoto2.NET samples.
    /// </summar>
    public class SampleApplication
    {
        #region Public Static Methods
        
        /// <summary>
        /// The entrypoint for the gPhoto2.NET samples application.
        /// </summar>
        public static void Main()
        {
            // Calls the asynchronous version of the main method, so that asynchronous operations can be performed
            SampleApplication.MainAsync().Wait();
        }

        /// <summary>
        /// The asynchronous entrypoint to the gPhoto2.NET samples application.
        /// </summary>
        public static async Task MainAsync()
        {
            // Gets all classes within the application, that implement the ISample interface, and instantiates instances of them
            Type sampleInterfaceType = typeof(ISample);
            IEnumerable<ISample> samples = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => sampleInterfaceType.IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type => Activator.CreateInstance(type) as ISample);
            
            // Prints out the name of the application
            Console.WriteLine("gPhoto2.NET Samples Application");
            Console.WriteLine("===============================");
            Console.WriteLine();
            
            // Prints out the instructions
            Console.WriteLine("Select a sample by typing in its number or type 'exit' to quit the application");
            Console.WriteLine();
            
            // Prints out all the available samples
            for (int i = 0; i < samples.Count(); i++)
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}. {1}", i + 1, samples.ElementAt(i).Title));
            
            // Starts the REPL loop
            while (true)
            {
                // Gets the input from the users
                Console.Write("gPhoto2.NET > ");
                string input = Console.ReadLine();
                
                // Parses the input, if the user typed in exit, then the application is shutdown
                if (input.ToUpperInvariant() == "EXIT")
                    break;

                // Parses the input, if it was a number, then the corresponding sample is executed
                int sampleNumber;
                if (!int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out sampleNumber))
                {
                    // The sample number could not be parsed, therefore an error message is printed out
                    Console.WriteLine("Please enter the number of the sample or 'exit' to quit the application.");
                    continue;
                }
                
                // Tries to get the specified sample and execute it, if it could not be found then an error message is printed out
                try
                {
                    ISample sampleToExecute = samples.ElementAt(sampleNumber - 1);
                    await sampleToExecute.ExecuteAsync();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "A sample with the number {0} could not be found.", sampleNumber));
                }
            }
        }
        
        #endregion
    }
}