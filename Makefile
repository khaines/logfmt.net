all: clean restore build test

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
	dotnet pack -o ../ logfmt/
