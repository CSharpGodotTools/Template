> [!NOTE]
> You can disable tests by setting `on: [push, pull_request]` to `on: []` in `.github\workflows\build_and_test.yml`. Also in this workflow you can change the path tests are found. By default it is set to `paths: 'res://Setup/Testing'`.

## Why should I use tests?

Tests make sure your code does what it is suppose to do. Write tests first, then and only then add more features when all your tests pass.

## Create `.runsettings` in root of project
Replace `PATH\TO\GODOT_EXE` with the path to your Godot executable.

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
    <RunConfiguration>
        <MaxCpuCount>1</MaxCpuCount>
        <TestAdaptersPaths>.</TestAdaptersPaths>
        <ResultsDirectory>./TestResults</ResultsDirectory>
        <TestSessionTimeout>1800000</TestSessionTimeout>
        <TreatNoTestsAsError>true</TreatNoTestsAsError>
        
        <!-- Environment variables available to tests -->
        <EnvironmentVariables>
            <GODOT_BIN>PATH\TO\GODOT_EXE</GODOT_BIN>
        </EnvironmentVariables>
    </RunConfiguration>

    <!-- Test result loggers -->
    <LoggerRunSettings>
        <Loggers>
            <Logger friendlyName="console" enabled="True">
                <Configuration>
                    <Verbosity>normal</Verbosity>
                </Configuration>
            </Logger>
            <Logger friendlyName="html" enabled="True">
                <Configuration>
                    <LogFileName>test-result.html</LogFileName>
                </Configuration>
            </Logger>
            <Logger friendlyName="trx" enabled="True">
                <Configuration>
                    <LogFileName>test-result.trx</LogFileName>
                </Configuration>
            </Logger>
        </Loggers>
    </LoggerRunSettings>

    <!-- GdUnit4-specific configuration -->
    <GdUnit4>
        <!-- Additional Godot runtime parameters -->
        <Parameters>--verbose</Parameters>
        
        <!-- Test display name format: SimpleName or FullyQualifiedName -->
        <DisplayName>FullyQualifiedName</DisplayName>
        
        <!-- Capture stdout from test cases -->
        <CaptureStdOut>true</CaptureStdOut>
        
        <!-- Compilation timeout for large projects (milliseconds) -->
        <CompileProcessTimeout>20000</CompileProcessTimeout>
    </GdUnit4>
</RunSettings>
```

## Setup
### Visual Studio
1. In Visual Studio, go to `Test > Configure Run Settings` and browse to and select the `.runsettings` file.
2. Restart Visual Studio.
3. Click on `Test > Test Explorer` and you should be able to run all tests.

<img width="846" height="200" alt="image" src="https://github.com/user-attachments/assets/b78d4e1c-e0d7-4c8a-b769-c5c73bcca798" />

### VSCode
1. In VSCode, go to `Extensions` and search for `C# Dev Kit by Microsoft` (do not install this just yet)
2. On the `C# Dev Kit` extension page, click on the gear icon to the right and click on `Download Specific Version VSIX` and select `1.5.12`
3. Move `ms-dotnettools.csdevkit-1.5.12-win32-x64.vsix` to the root of the project
4. Run `code --install-extension ms-dotnettools.csdevkit-1.5.12-win32-x64.vsix --force` from within VSCode terminal
5. Delete `ms-dotnettools.csdevkit-1.5.12-win32-x64.vsix`
6. Restart VSCode
7. Click on the `Testing` tab on left and you should be able to run all tests.

<img width="413" height="245" alt="image" src="https://github.com/user-attachments/assets/758e8f86-f440-42dd-89b8-7479489d9b90" />

> [!NOTE]
> Running `dotnet test` requires the Godot executable path to be in an environment variable named `GODOT_BIN`.

## Testing
```cs
using GdUnit4;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

[TestSuite]
public class Tests
{
    [TestCase]
    public void StringToLower()
    {
        AssertString("AbcD".ToLower()).IsEqual("abcd");
    }
}
```

See the relevant links below for more complete examples.

## Notes
The `.runsettings` must not be committed to the repo otherwise CI will use the path from that file and fail.

## Relevant Links
https://github.com/godot-gdunit-labs/gdUnit4  
https://godot-gdunit-labs.github.io/gdUnit4/latest/csharp_project_setup/csharp-setup/  
https://github.com/godot-gdunit-labs/gdUnit4NetExamples/tree/master/Examples/Basics  
https://github.com/godot-gdunit-labs/gdUnit4NetExamples/tree/master/Examples/Advanced  
https://github.com/marketplace/actions/gdunit4-test-runner-action  