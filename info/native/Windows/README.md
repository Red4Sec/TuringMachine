# Windows Error Reporting
The Windows Error Reporting (WER) service is a crash reporting technology designed to collect post-error debug information from Windows systems as well as third-party software failures.

## Table of Contents

- [Enable WER](#enable-wer)
- [Requirements](#requirements)
- [Installation](#installation)

## Enable WER

To enable WER, first create a directory at `C:\Temporal\CrashDumps`, then execute the following 'reg' files.

- [werConfig x32.reg](werConfig%20x32.reg)
- [werConfig x64.reg](werConfig%20x64.reg)

After restart the computer, you will obtain a dmp file every time a Windows software dies due to a crash.

## Requirements

Download the windbg.exe binary:

- http://www.microsoft.com/whdc/devtools/debugging/installx86.mspx
- https://developer.microsoft.com/es-es/windows/downloads/windows-10-sdk

Download the !exploitable extension:

- http://msecdbg.codeplex.com/
- https://blog.didierstevens.com/2018/07/17/exploitable-crash-analyzer-statically-linked-crt/

## Installation

#### WinDbg

- Install windbg like any other application.

#### !exploitable

For !exploitable configuration:

- Unpack the zip file and copy the extracted files to `\Binaries\x86` and `\Binaries\x64`.
- Copy them to `\Program Files\Debugging Tools for Windows (x86)\winext\`.

To test _!exploitable_, open any crash dump in windbg and type:

```
!load winext\msec.dll;
!exploitable;
```
