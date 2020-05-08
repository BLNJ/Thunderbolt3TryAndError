using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Thunderbolt3TryAndError.Service
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else if(args.Length == 1)
            {
                string arg = args[0];
                if(arg == "install" || arg == "uninstall")
                {
                    bool install = (arg == "install");
                    try
                    {
                        using(AssemblyInstaller installer = new AssemblyInstaller(typeof(Program).Assembly, new string[] { }))
                        {
                            IDictionary state = new Hashtable();
                            installer.UseNewContext = true;

                            try
                            {
                                if(install)
                                {
                                    Console.WriteLine("Installing Service...");
                                    installer.Install(state);
                                    Console.WriteLine("Service installed!");

                                    Console.WriteLine("Commiting...");
                                    installer.Commit(state);
                                    Console.WriteLine("Commited!");
                                }
                                else
                                {
                                    Console.WriteLine("Uninstalling Service...");
                                    installer.Uninstall(state);
                                    Console.WriteLine("Service uninstalled!");
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Exception occured: " + ex.ToString());
                                try
                                {
                                    Console.WriteLine("Rolling back...");
                                    installer.Rollback(state);
                                    Console.WriteLine("Done!");
                                }
                                catch (Exception ex2)
                                {
                                    Console.WriteLine("Rollback failed: " + ex2.ToString());
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Exception occured: " + ex.ToString());
                    }

                    Console.ReadLine();
                }
            }
        }
    }
}
