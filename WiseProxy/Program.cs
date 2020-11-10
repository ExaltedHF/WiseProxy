using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using Mono.Options;

namespace WiseProxy
{
    internal static class Program
    {
        private static readonly List<string> WorkingProxiesList = new List<string>();
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
                    $"\n\t Default: '{_testUrl}'" +
                    "\n\t Expects raw 'íp'",
                    val => { _testUrl = val; }
                },
                {
                    "f|file=",
                    $"Path to proxy file \n\t Default: '{_proxyPath}'",
                    val => { _proxyPath = val; }
                },
                {
                    "o|out=",
                    $"Path to output file \n\t Default: '{_outputPath}'",
                    val => { _outputPath = val; }
                },
                {
                    "t|threads=",
                    $"Number of threads \n\t Default: '{_threads}'",
                    val => { _threads = Convert.ToInt32(val); }
                },
                {
                    "p|proxyType=",
                    $"Type of proxies \n\t Valid: 'HTTP(S)|SOCKS4|SOCKS4A|SOCKS5' \n\t Default: '{_proxyType}'",
                    val => { _proxyType = val; }
                },
                {
                    "s|proxyTimeout=",
                    $"Timeout for proxy in ms \n\t Default: '{_proxyTimeout}'",
                    val => { _proxyTimeout = Convert.ToInt32(val); }
                },
                {
                    "c|requestTimeout=",
                    $"Timeout for request in ms \n\t Default: '{_requestTimeout}'",
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
                Console.WriteLine($"$ {e.Message} - '{e.OptionName}'");
                Console.WriteLine("$ Try using '--help' for a list of valid options.");
                return;
            }

            DisplayLogo();
            Console.SetCursorPosition(0, 9);

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
        }

        private static void UpdateStatistics()
        {
            // Todo: Do this with delegates
            while (true)
            {
                var currentString =
                    $"# Checked: {_checkedProxies}/{_totalProxies} # Working: {_workingProxies} # Failed: {_failedProxies}";
                Console.Title = currentString;
                Console.WriteLine(currentString);
                Console.SetCursorPosition(0, 9);
            }
        }

        private static void CheckProxies()
        {
            List<string> proxiesList;
            try
            {
                proxiesList = new List<string>(File.ReadAllLines(_proxyPath));
                _totalProxies = proxiesList.Count;
            }
            catch (Exception e)
            {
                Console.WriteLine($"$ {e.Message}");
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
                        WorkingProxiesList.Add(p);
                    }
                    else
                    {
                        Interlocked.Increment(ref _failedProxies);
                    }
                }
                catch (HttpException e)
                {
                    // Todo: Show this to the user
                    // Console.SetCursorPosition(0, 12);
                    // Console.WriteLine($"$ {p} - {e.Message}");
                    Interlocked.Increment(ref _failedProxies);
                }
                catch (Exception e)
                {
                    // Todo: Show this to the user
                    // Console.WriteLine($"$ : {e.Message}");
                    Interlocked.Increment(ref _failedProxies);
                    if (_workingProxies > 0)
                        OutputWorkingProxies();
                }
                finally
                {
                    Interlocked.Increment(ref _checkedProxies);
                    request?.Dispose();
                }
            });

            if (_workingProxies > 0)
                OutputWorkingProxies();
            else
                Console.WriteLine("$ They're all dead Jim, they're all dead... :(\n");
        }

        private static void OutputWorkingProxies()
        {
            Console.WriteLine($"$ Exporting working proxies to {_outputPath}");

            if (string.IsNullOrEmpty(_outputPath))
            {
                Console.WriteLine("$ You forgot the output path, enter a new one!");

                while (_outputPath != string.Empty)
                {
                    Console.Write("$ New output path: ");
                    _outputPath = Console.ReadLine();
                }
            }

            try
            {
                File.AppendAllLines(_outputPath, WorkingProxiesList);
            }
            catch (Exception e)
            {
                // Todo: Not be like this
                switch (e.InnerException)
                {
                    case DirectoryNotFoundException:
                        Console.WriteLine("$ Directory not found, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    case FileNotFoundException:
                        Console.WriteLine("$ File not found, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    case PathTooLongException:
                        Console.WriteLine("$ Path too long, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    case IOException:
                        Console.WriteLine("$ IO error while writing, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    case NotSupportedException:
                        Console.WriteLine("$ Error while writing, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    case SecurityException:
                        Console.WriteLine("$ Someone is blocking me, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    case UnauthorizedAccessException:
                        Console.WriteLine("$ I am not allowed to write here, enter new save path:");
                        _outputPath = Console.ReadLine();
                        OutputWorkingProxies();
                        break;
                    default:
                        throw new Exception();
                }
            }

            Console.WriteLine("$ Successfully wrote working proxies... Have fun ;)\n");
        }
    }
}