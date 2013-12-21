sign
====

It is simple tool to sign assemblies. The tool also fixes references.

## Nuget 

Nuget package http://nuget.org/packages/sign/

To Install from the Nuget Package Manager Console 
    
    PM> Install-Package sign

## Sample usage

    .\sign.exe --key=".\key.snk" --dirs=".\path\to\assemblies\"
    
## Other switches

    -f, --files      List of files to sign
    -d, --dirs       List of directories contains files to sign
    -k, --key        Required. *.snk file contains public key.
    -p, --filter     (Default: (dll|exe)$) File selection filter
    -v, --verbose    Display all messages.
    --help           Display this help screen.

## Input assemblies

![InputAssemblies](https://raw.github.com/rjasica/sign/master/web/before.png)

## Output assemblies

![OutputAssemblies](https://raw.github.com/rjasica/sign/master/web/after.png)

## Icon

Pen by Bram van Rijen from The [The Noun Project](http://thenounproject.com)
