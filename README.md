## Project Intro
logfmt.net is intended to be a simple & lightweight structured logging library for .net applications.

Goals:
- Support the logfmt output format.
- Enable the practice of structured logging.
- Provide space delimited or json encoding of output.
- Sending output to default console stream or any other provided stream.

Non-Goals:
- Managing file rollovers
- Data routing rules & filters
- Output templates


## Installation

Binary distribution of this library is available in a nuget package. Run the following command to install it:

```
dotnet add package logfmt.net --version 0.0.2-alpha
```

Note: this requires the .NET CLI tool. Other installation methods for nuget packages are available on the package page @ https://www.nuget.org/packages/logfmt.net

## Building the source

Running `make all` in a shell from the root directory of this repository will build the library and execute the tests. It uses the `dotnet` command, so the .NET CLI tool must be installed. 


## Using the library

To use the logfmt logger, create a new instance of the Logger class:

```
var log = new logfmt.Logger();
log.Info("hello logs!");
```

The default contructor will send the log output to the console output stream. There is an overload for the constructor to provide a different stream to send output to. 

All of the severity log methods (Debug,Info,Warn,Error) accept KeyValuePair<string,string> params in addition to the log message. This enables the logging of structured data outside of the log message string.

```
var log = new logfmt.Logger();
log.Info("hello logs!", new KeyValuePair<string,string>("foo","bar"),new KeyValuePair<string,string>("bar","foo"));
```

If there is a key value pair that is needed on every logging call, the WithData method can register it for all future calls in the instance.

```
var log = new logfmt.Logger().WithData(new KeyValuePair<string,string>("foo","bar"));
// the Info call below will have foo=bar added to the output 
log.Info("hello logs!");
```

## Contributions

Contributions are welcomed & encouraged! If there is something missing or broken please feel free to write an issue, or submit a PR. 