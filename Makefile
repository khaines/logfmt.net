all: restore clean build test

clean:
	dotnet clean Logfmt/
	rm -f *.nupkg
restore:
	dotnet restore Logfmt/
build:	
	dotnet build Logfmt/
test:
	dotnet test Logfmt.Tests/
pack:
	dotnet pack -o ../ Logfmt/
