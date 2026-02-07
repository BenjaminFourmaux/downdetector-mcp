# downdetector-mcp

![](https://img.shields.io/badge/10.0-mode?logo=dotnet&logoColor=white&label=C%23&color=darkgreen)
![](https://img.shields.io/badge/Down-detector-website?logo=downdetector&logoColor=red&color=red)
![](https://badge.mcpx.dev?type=server 'MCP Server')

HTTP based [MCP](https://modelcontextprotocol.io/) server to get online services status via [Downdector](https://downdetector.com/).

Providing data on the status of online services for agentic AI applications.

## Tools 🛠

List of tools available in this MCP server

### SearchServiceName

Used to retrieve the correct service name same as named in Downdetector (also name `TechnicalServiceName` in the MCP).

> [!TIP]
> This must be called in first, for the AI get the correct name for use the [GetServiceStatus tool](#getservicestatus) and avoid error when retrieve data with wrong service name.

#### Input parameters

| param | description |
| ----- | ----------- |
| `serviceName` | Service name to search |
| `country` | Alpha-2 country code for a search in the right country instanceof Downdetector website |

#### Output example

Tool calling with params :
- `serviceName` : `google`
- `country` : `us`

```toon
SearchWord: google
Results[4]{ServiceName,TechnicalName,Url}:
  Google,google,https://downdetector.com/status/google
  Google Cloud,google-cloud,https://downdetector.com/status/google-cloud
  Google Gemini,googlegemini,https://downdetector.com/status/googlegemini
  Google Nest,google-nest,https://downdetector.com/status/google-nest
```

### GetServiceStatus

Get data about the status of the online service. Like the most reported issues, the last number of report and the baseline.

#### Input parameters

| param | description |
| ----- | ----------- |
| `technicalServiceName` | Technical service name to getting data |
| `country` | Alpha-2 country code for a search in the right country instanceof Downdetector website |
| `includeHistoricalReportData` | Boolean to retrieve chart points historic of the last 24 hours. By default `false` |

#### Output example

Tool calling with params :
- `technicalServiceName` : `googlegemini`
- `country` : `us`

```toon
ServiceName: Google Gemini
Status: WARNING
MostReportedIssues[3]{Issue,Percentage}:
  AI Generation,57
  Website,24
  App,20
ReportData[96]{Time,Report,Baseline}:
  14:05 07/02/2026,8,2
  15:05 07/02/2026,4,2
  16:05 07/02/2026,3,1
  ...
LastReportData:
  Time: 14:05 08/02/2026
  Report: 25
  Baseline: 5
```

## Using 🚀

### Docker (recommended)

#### From Docker Hub

```bash
docker run -d -p 8080:8080 --name downdetector-mcp benjaminfourmauxb/downdetector-mcp:latest
```

#### Build from source

```bash
docker build -t downdetector-mcp .
```

#### Run the container

```bash
docker run -d -p 8080:8080 --name downdetector-mcp downdetector-mcp
```

The server will be available on `http://localhost:8080`.

### From source code

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PowerShell](https://docs.microsoft.com/powershell/scripting/install/installing-powershell) (required by Playwright CLI)

#### Build

```bash
dotnet build
```

#### Install Playwright Chromium

After building, install the Chromium browser used by Playwright :

```bash
pwsh DowdetectorMCP.Server/bin/Debug/net10.0/playwright.ps1 install chromium
```

> [!NOTE]
> This command also installs the required OS-level dependencies for Chromium. You only need to run it once (or after a Playwright version update).

#### Run

```bash
dotnet run --project DowdetectorMCP.Server
```

The server starts on `http://localhost:5057` by default.

## How it works 🏗

This app is build in **C# .Net 10** and use the SDK [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore/) to implement the MCP protocol.

The Project `DowndetectorMCP.API.csproj` is used to query data from Downdetector websites.

The only easier way to get data from Downdetector is to simulate a web browser. 
\
For that, DowndetectorMCP.API use [Playwright](https://playwright.dev/dotnet/) a End-to-End testing framework diverted to browse DOM page via a Chronium Headless browser.

Downdetector use Cloudflare to protect their websites against robot or DDoS attack. Request multi times the website can cause rate limit error or blocked with the famous page "✅ Are you a Human".

To reduce the number of request sent to Donwdetector, a cache system is set for [tool SearchServiceName](#searchservicename). This cache save the couple `<serviceName, country> => result`.
\
The lifetime of a cache record is 24 Hours.

Is not viable to save all `technicalServiceName`, because there is not a finite number of services (_"Tomorrow a new AI company can spawn..."_). That's why there are a dedicated tool for search service.

All tools ouput use the [Token Oriented Object Notation (Toon) format](https://github.com/toon-format/toon) to represent data. This CSV-like format can reduce the number of input token (and inference input time) by remove redundant field names (like JSON).

Error messages are very verbose and human like error explication for indicate the the agent what somthing wrong append (Cloudflare rate limit reach, service not found...).

## Contributing 👨‍👨‍👧

Contributions are appreciate !
\
Don't hesitate if you want to add a new feature.


Please also let me know if you use my MCP and in which project. I'd appreciate it.
