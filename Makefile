all: clean restore build test

clean:
	dotnet clean logfmt/
restore:
	dotnet restore logfmt/
build:
	dotnet build logfmt/
test:
	dotnet test logfmt_tests/
