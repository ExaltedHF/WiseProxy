# WiseProxy
![GitHub](https://img.shields.io/github/license/ExaltedHF/WiseProxy?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/ExaltedHF/WiseProxy?style=flat-square)
[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat-square)](https://github.com/moodiest/Proxy-Checker/issues)
[![HitCount](http://hits.dwyl.com/ExaltedHF/WiseProxy.svg)](http://hits.dwyl.com/ExaltedHF/WiseProxy)

A very simple cross-platform proxy checker written in C# utilizing .NET 5.0

## Features
* Command line interface
* HTTP(S), Socks4, Socks4a and Socks5
* Multi-threaded
* Customizable timeout
* Remove duplicates
* ???

## Usage

Example:

`./WiseProxy --file=proxies.text --out=out.txt --threads=25 --timeout=2000 --type=HTTPS --removeDoubles`

```
██╗    ██╗██╗███████╗███████╗██████╗ ██████╗  ██████╗ ██╗  ██╗██╗   ██╗
██║    ██║██║██╔════╝██╔════╝██╔══██╗██╔══██╗██╔═══██╗╚██╗██╔╝╚██╗ ██╔╝
██║ █╗ ██║██║███████╗█████╗  ██████╔╝██████╔╝██║   ██║ ╚███╔╝  ╚████╔╝
██║███╗██║██║╚════██║██╔══╝  ██╔═══╝ ██╔══██╗██║   ██║ ██╔██╗   ╚██╔╝
╚███╔███╔╝██║███████║███████╗██║     ██║  ██║╚██████╔╝██╔╝ ██╗   ██║
 ╚══╝╚══╝ ╚═╝╚══════╝╚══════╝╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝

Usage: WiseProxy [OPTIONS]

Options:
  -f, --file=VALUE           Path to text file containing proxies
                                 Default: 'proxies.txt'
  -o, --out=VALUE            Path to text file to export working proxies to
                                 Default: 'out.txt'
  -t, --threads=VALUE        Amount of threads to run
                                 Default: '50'
  -x, --timeout=VALUE        Timeout after x milliseconds
                                 Default: '2500'
  -s, --type=VALUE           Type of proxies
                                 Valid: 'HTTP(S)|SOCKS4|SOCKS4A|SOCKS5'
                                 Default: 'HTTPS'
  -r, --removeDoubles        Removes doubles from proxy file before checking
                                 Default: 'False'
  -h, --help                 Prints this and exits
                                 Default: 'False'
```

## Screenshot

![CLI](https://i.imgur.com/y7upGu8.png)