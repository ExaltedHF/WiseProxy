using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using Mono.Options;

namespace WiseProxy
{
    internal static class Program
    {
        private static string _proxyPath = "proxies.txt";
        private static string _proxyType = "HTTPS";
        private static string _outputPath = "out.txt";
        private static int _threads = 50;
        private static int _timeout = 2500;
        private static int _totalProxies;
        private static int _checkedProxies;
        private static int _workingProxies;
        private static int _failedProxies;
        private static bool _showHelp;
        private static bool _removeDoubles;

        private static void Main(string[] args)
        {
            var options = new OptionSet
            {
                "Usage: ProxyChecker [OPTIONS]",
                "",
                "Options:",
                {
                    "f|file=",
                    "Path to text file containing proxies \n\t Default: 'proxies.txt'",
                    val => { _proxyPath = val; }
                },
                {
                    "o|out=",
                    "Path to text file to export working proxies to \n\t Default: 'out.txt'",
                    val => { _outputPath = val; }
                },
                {
                    "t|threads=",
                    "Amount of threads to run \n\t Default: '50'",
                    val => { _threads = Convert.ToInt32(val); }
                },
                {
                    "x|timeout=",
                    "Timeout after x milliseconds \n\t Default: '2500'",
                    val => { _timeout = Convert.ToInt32(val); }
                },
                {
                    "s|type=",
                    "Type of proxies \n\t Valid: 'HTTP(S)|SOCKS4|SOCKS4A|SOCKS5' \n\t Default: 'HTTPS'",
                    val => { _proxyType = val; }
                },
                {
                    "r|removeDoubles",
                    "Removes doubles from proxy file before checking \n\t Default: 'False'",
                    val => { _removeDoubles = val != null; }
                },
                {
                    "h|help",
                    "Prints this and exits \n\t Default: 'False'",
                    val => { _showHelp = val != null; }
                }
            };

            try
            {
                var extra = options.Parse(args);
                foreach (var item in extra) throw new OptionException("Option does not exist", item);
            }
            catch (OptionException e)
            {
                Console.WriteLine($"WiseProx$ {e.Message} - '{e.OptionName}'");
                Console.WriteLine("WiseProx$ Try using '--help' for a list of valid options.");
                return;
            }

            DisplayLogo();

            if (_showHelp)
                options.WriteOptionDescriptions(Console.Out);
            else
                CheckProxies();
        }

        private static void DisplayLogo()
        {
            const string title = @"
██╗    ██╗██╗███████╗███████╗██████╗ ██████╗  ██████╗ ██╗  ██╗██╗   ██╗
██║    ██║██║██╔════╝██╔════╝██╔══██╗██╔══██╗██╔═══██╗╚██╗██╔╝╚██╗ ██╔╝
██║ █╗ ██║██║███████╗█████╗  ██████╔╝██████╔╝██║   ██║ ╚███╔╝  ╚████╔╝ 
██║███╗██║██║╚════██║██╔══╝  ██╔═══╝ ██╔══██╗██║   ██║ ██╔██╗   ╚██╔╝  
╚███╔███╔╝██║███████║███████╗██║     ██║  ██║╚██████╔╝██╔╝ ██╗   ██║   
 ╚══╝╚══╝ ╚═╝╚══════╝╚══════╝╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝ 
";
            Console.WriteLine(title + "\n\n\n");
        }

        private static void UpdateStatistics()
        {
            while (true)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 3);
                Console.WriteLine($"Checked: {_checkedProxies}/{_totalProxies}");
                Console.WriteLine($"Working: {_workingProxies}");
                Console.WriteLine($"Failed: {_failedProxies}");
                Thread.Sleep(100);
            }
        }

        private static void CheckProxies()
        {
            List<string> proxiesList;
            var workingProxiesList = new List<string>();
            try
            {
                proxiesList = new List<string>(File.ReadAllLines(_proxyPath));
                _totalProxies = proxiesList.Count;
            }
            catch (Exception e)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 4);
                Console.WriteLine($"\nWiseProx$ {e.Message}");
                return;
            }

            if (_removeDoubles)
                proxiesList = proxiesList.Distinct().ToList();

            Task.Factory.StartNew(UpdateStatistics);
            Parallel.ForEach(proxiesList, new ParallelOptions {MaxDegreeOfParallelism = _threads}, p =>
            {
                HttpRequest request = null;
                try
                {
                    request = new HttpRequest
                    {
                        Proxy = _proxyType switch
                        {
                            "HTTP" => HttpProxyClient.Parse(p),
                            "HTTPS" => HttpProxyClient.Parse(p),
                            "SOCKS4" => Socks4ProxyClient.Parse(p),
                            "SOCKS4A" => Socks4AProxyClient.Parse(p),
                            "SOCKS5" => Socks5ProxyClient.Parse(p),
                            _ => HttpProxyClient.Parse(p)
                        }
                    };

                    request.Proxy.ConnectTimeout = _timeout;
                    var response = request.Get("https://api.ipify.org").ToString();
                    if (response == p.Split(":")[0])
                    {
                        Interlocked.Increment(ref _workingProxies);
                        workingProxiesList.Add(p);
                    }
                    else
                    {
                        Interlocked.Increment(ref _failedProxies);
                    }
                }
                catch (HttpException)
                {
                    Interlocked.Increment(ref _failedProxies);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _failedProxies);
                    if (_workingProxies > 0)
                        OutputWorkingProxies(workingProxiesList);
                }
                finally
                {
                    Interlocked.Increment(ref _checkedProxies);
                    request?.Dispose();
                }
            });

            if (_workingProxies > 0)
                OutputWorkingProxies(workingProxiesList);
            else
                Console.WriteLine("WiseProx$ They're all dead Jim, they're all dead...");
        }

        private static void OutputWorkingProxies(IEnumerable<string> workingProxiesList)
        {
            Console.WriteLine($"\n\nWiseProx$ Exporting working proxies to {_outputPath}");
            File.AppendAllLines(_outputPath, workingProxiesList);
            Console.WriteLine("WiseProx$ Successfully wrote working proxies... Have fun ;)");
        }
    }
}