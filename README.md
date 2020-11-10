# WiseProxy
![GitHub](https://img.shields.io/github/license/ExaltedHF/WiseProxy?style=flat-square)
[![GitHub last commit](https://img.shields.io/github/last-commit/ExaltedHF/WiseProxy.svg?style=flat-square)]()
![GitHub release (latest by date)](https://img.shields.io/github/v/release/ExaltedHF/WiseProxy?style=flat-square)
![!Github issues](https://img.shields.io/github/issues/ExaltedHF/WiseProxy?style=flat-square)
[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat-square)](https://github.com/moodiest/Proxy-Checker/issues)
[![HitCount](http://hits.dwyl.com/ExaltedHF/WiseProxy.svg)](http://hits.dwyl.com/ExaltedHF/WiseProxy)

WiseProxy - A very simple open source cross-platform proxy checker written in C# utilizing [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0), [Leaf.xNet](https://github.com/csharp-leaf/Leaf.xNet) and [Mono.Options](https://www.nuget.org/packages/Mono.Options/).

## Features
* Command line interface
* Cross-platform
* Multi-threaded    
* HTTP(S), Socks4, Socks4a and Socks5
* Customizable test URL
* Customizable timeouts (Proxy & Request)
* Remove proxy duplicates
* ???

## Usage

### Example

`./WiseProxy -f proxy2020.txt -o valid.text -t 45 -p SOCKS5 -d`

### Arguments 

```
Usage: $ ./WiseProxy [OPTIONS]

Options:
  -u, --url=VALUE            The URL used for testing
                                 Default: 'https://api.ipify.org/'
                                 Expects raw 'íp'
  -f, --file=VALUE           Path to proxy file
                                 Default: 'proxies.txt'
  -o, --out=VALUE            Path to output file
                                 Default: 'out.txt'
  -t, --threads=VALUE        Number of threads
                                 Default: '35'
  -p, --proxyType=VALUE      Type of proxies
                                 Valid: 'HTTP(S)|SOCKS4|SOCKS4A|SOCKS5'
                                 Default: 'HTTPS'
  -s, --proxyTimeout=VALUE   Timeout for proxy in ms
                                 Default: '2500'
  -c, --requestTimeout=VALUE Timeout for request in ms
                                 Default: '2500'
  -d, --removeDuplicates     Remove duplicates before checking
                                 Default: 'False'
  -h, --help                 Prints this output and exits
                                 Default: 'False'
```

## To-do
* Cleanup
* Let me know!

## Screenshot

![CLI](https://i.imgur.com/rSQCWre.png)