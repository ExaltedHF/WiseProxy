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
        private static string _testUrl = "https://api.ipify.org";
        private static string _proxyType = "HTTPS";
        private static string _proxyPath = "proxies.txt";
        private static string _outputPath = "out.txt";
        private static int _threads = 50;
        private static int _requestTimeout = 2500;
        private static int _proxyTimeout = 2500;
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
                "Usage: $ ./WiseProxy [OPTIONS]",
                "",
                "Options:",
                {
                    "u|url=",
                    "The URL used for testing " +
                    "\n\t Default: 'https://api.ipify.org/'" +
                    "\n\t Expects raw 'íp'",
                    val => { _testUrl = val; }
                },
                {
                    "f|file=",
                    "Path to proxy file \n\t Default: 'proxies.txt'",
                    val => { _proxyPath = val; }
                },
                {
                    "o|out=",
                    "Path to output file \n\t Default: 'out.txt'",
                    val => { _outputPath = val; }
                },
                {
                    "t|threads=",
                    "Number of threads \n\t Default: '35'",
                    val => { _threads = Convert.ToInt32(val); }
                },
                {
                    "p|proxyType=",
                    "Type of proxies \n\t Valid: 'HTTP(S)|SOCKS4|SOCKS4A|SOCKS5' \n\t Default: 'HTTPS'",
                    val => { _proxyType = val; }
                },
                {
                    "s|proxyTimeout=",
                    "Timeout for proxy in ms \n\t Default: '2500'",
                    val => { _proxyTimeout = Convert.ToInt32(val); }
                },
                {
                    "c|requestTimeout=",
                    "Timeout for request in ms \n\t Default: '2500'",
                    val => { _requestTimeout = Convert.ToInt32(val); }
                },
                {
                    "d|removeDuplicates",
                    "Remove duplicates before checking \n\t Default: 'False'",
                    val => { _removeDoubles = val != null; }
                },
                {
                    "h|help",
                    "Prints this output and exits \n\t Default: 'False'\n\n",
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
                Console.WriteLine($"WiseProxy $ {e.Message} - '{e.OptionName}'");
                Console.WriteLine("WiseProxy $ Try using '--help' for a list of valid options.");
                return;
            }

            DisplayLogo();
            Console.SetCursorPosition(0, Console.CursorTop - 4);

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
 ╚══╝╚══╝ ╚═╝╚══════╝╚══════╝╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝ ";
            Console.WriteLine(title);
            Console.WriteLine("\n\n\n\n");
        }

        private static void UpdateStatistics()
        {
            while (true)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 4);
                Console.WriteLine(
                    $"# Checked: {_checkedProxies}/{_totalProxies}\n# Working: {_workingProxies}\n# Failed: {_failedProxies}\n");
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
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                Console.WriteLine($"WiseProxy $ {e.Message}");
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
                        },
                        ConnectTimeout = _requestTimeout
                    };

                    request.Proxy.ConnectTimeout = _proxyTimeout;
                    var response = request.Get(_testUrl).ToString();

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
                Console.WriteLine("WiseProxy $ They're all dead Jim, they're all dead... :(\n");
        }

        private static void OutputWorkingProxies(IEnumerable<string> workingProxiesList)
        {
            Console.WriteLine($"WiseProxy $ Exporting working proxies to {_outputPath}");
            File.AppendAllLines(_outputPath, workingProxiesList);
            Console.WriteLine("WiseProxy $ Successfully wrote working proxies... Have fun ;)\n");
        }
    }
}