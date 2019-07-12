all: clean restore build test

clean:
	dotnet clean logfmt/
	rm -f *.nupkg
restore:
	dotnet restore logfmt/
build:
	
	sed -i '' "s;\<PackageVersion\>.*\<;\<PackageVersion\>$(shell ./tools/getversion.sh)\<;g" logfmt/logfmt.csproj
	dotnet build logfmt/
test:
	dotnet test logfmt_tests/
pack:
	dotnet pack -o ../ logfmt/
