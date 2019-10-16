<p align="center">
  <img src="https://avatars0.githubusercontent.com/u/33096324?s=200&v=4" width="200px">
</p>

<h1 align="center">Turing Machine</h1>
<p align="center">
  <a href="https://travis-ci.org/Red4Sec/TuringMachine" target="_blank">
    <img src="https://travis-ci.org/Red4Sec/TuringMachine.svg?branch=master" alt="Current TravisCI build status.">
  </a>
  <a href="https://codecov.io/gh/Red4Sec/TuringMachine" target="_blank">
    <img src="https://codecov.io/github/Red4Sec/TuringMachine/branch/master/graph/badge.svg" alt="Current Coverage Status." />
  </a>
  <a href="https://github.com/Red4Sec/TuringMachine/blob/master/LICENSE" target="_blank">
    <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License.">
  </a>
</p>
<p align="center">
  <a href="https://github.com/Red4Sec/TuringMachine/releases" target="_blank">
    <img src="https://badge.fury.io/gh/Red4Sec%2FTuringMachine.svg" alt="Current version.">
  </a>
  <a href="https://www.nuget.org/packages/TuringMachine.Core" target="_blank">
    <img src="https://badge.fury.io/nu/TuringMachine.Core.svg" alt="Current nuget version.">
  </a>
</p>

<p align="center">Turing Machine is a Fuzzer developed in Net Standard that allows to fuzz any type of applications or libraries, focused on .Net Framework, Dotnet Core and Net Standard applications.</p>
<p align="center">This tool is designed for software engineers, developers and bug hunters. An open-source tool for software security testing and quality assurance.</p>

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Build](#build)
- [Usage](#usage)
- [Debug](#debug)
- [HallOfFame](#hallOfFame)

## Features

Turing Machine currently supports the following fuzzing techniques:

- Mutation-based Fuzzing: mutations of existing input values.
- Random Fuzzing: randomly generated input values.
- More coming soon. Stay tuned! 

<p align="center">
  <img src="https://github.com/Red4Sec/TuringMachine/blob/master/info/types.png?raw=true" width="700px">
</p>

TuringMachine also allows to integrate code coverage in .Net applications using [coverlet](https://github.com/tonerdo/coverlet) library, which will allow you tracing the coverage of the fuzzed applications in real-time. Once finished, it will generate a full coverage report using [ReportGenerator](https://github.com/danielpalme/ReportGenerator) library.

## Requirements

If you want to use TuringMachine for .Net software, the only requirement is:

- Dotnet core 2.2

If you want to use it with native programs, please follow the steps described in:

- [Windows](info/native/Windows)

## Installation

Grab the latest binaries from [releases](https://github.com/Red4Sec/TuringMachine/releases).

## Build

```
git clone https://github.com/Red4Sec/TuringMachine
cd src\TuringMachine
dotnet restore
dotnet build
```

## Usage

[![Watch the demo](https://img.youtube.com/vi/EKnMCkSQiGY/maxresdefault.jpg)](https://youtu.be/EKnMCkSQiGY)

#### Clone & Build

```
git clone https://github.com/Red4Sec/TuringMachine
cd TuringMachine

dotnet restore src/TuringMachine/
dotnet build src/TuringMachine/

dotnet restore samples/Coverage.Payload/
dotnet build samples/Coverage.Payload/
```

#### Instrumentation
[Coverage.Payload](samples/Coverage.Payload)

```
dotnet .\src\TuringMachine\bin\Debug\netcoreapp2.2\TuringMachine.dll instrument --help
dotnet .\src\TuringMachine\bin\Debug\netcoreapp2.2\TuringMachine.dll instrument --path .\samples\Coverage.Payload\bin\Debug\netcoreapp2.2\
```

#### Start Server

```
dotnet .\src\TuringMachine\bin\Debug\netcoreapp2.2\TuringMachine.dll listen --help
dotnet .\src\TuringMachine\bin\Debug\netcoreapp2.2\TuringMachine.dll listen --inputs .\samples\random.input
```

#### Run Client

```
dotnet .\samples\Coverage.Payload\bin\Debug\netcoreapp2.2\Coverage.Payload.dll
```

#### Show the coverage

```
CONTROL+C  # in the client shell
Invoke-Item .\coverageReport\index.htm
```

## Debug

You can easily debug with Visual Studio 2019 by using `TuringMachine.sln`.

## HallOfFame

You can see the Hall of Fame [here](HallOfFame.md)
