all: clean restore build test pack

clean:
	dotnet clean logfmt/
	rm -f *.nupkg
restore:
	dotnet restore logfmt/
build:
	dotnet build logfmt/
test:
	dotnet test logfmt_tests/
pack:
	nuget pack logfmt/
