# Contributing to logfmt.net

First off, thank you for considering contributing to logfmt.net! It's people like you that make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

## Getting Started

To work on this project, you will need the following tools installed on your machine:

* **.NET SDK**: This project targets .NET 8.0 and .NET 10.0. You will need the .NET 10 SDK installed. You can download it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download).
* **IDE/Editor**: Visual Studio, JetBrains Rider, or VS Code (with C# Dev Kit) are recommended.

## Building the Project

The project is a standard .NET solution. You can build it using the .NET CLI from the root of the repository:

```bash
dotnet build
```

## Running Tests

We use `xUnit` for testing. Please ensure all tests pass before submitting a Pull Request.

To run the tests:

```bash
dotnet test
```

If you are adding new features, please include unit tests to cover the new functionality. We aim for high test coverage to ensure stability.

## Code Style and Standards

* **StyleCop**: We use `StyleCop.Analyzers` to enforce code style. The build will fail if there are style violations (warnings are elevated to errors).
* **Formatting**: Please ensure your code is formatted according to the project's `.editorconfig` (if present) or standard C# conventions.
* **Nullable Reference Types**: The project uses nullable reference types (`<Nullable>enable</Nullable>`). Please ensure your code handles nullability correctly and does not introduce new warnings.

## Submitting Changes

1. **Fork the Repository**: Create a fork of the repository on GitHub.
2. **Create a Branch**: Create a new branch for your feature or bug fix.

    ```bash
    git checkout -b feature/amazing-feature
    ```

3. **Commit Changes**: Make your changes and commit them with a clear and descriptive commit message.
4. **Push to Fork**: Push your branch to your forked repository.

    ```bash
    git push origin feature/amazing-feature
    ```

5. **Open a Pull Request**: Go to the original repository and open a Pull Request. Please describe your changes in detail and reference any related issues.

## Benchmarks

If you are making performance-sensitive changes, please run the benchmarks to ensure no regressions.

```bash
cd Logfmt.Benchmarks
dotnet run -c Release
```

## License

By contributing, you agree that your contributions will be licensed under its MIT License.
