function CleanEnvironmnet {
    Remove-Item -Path .\tests\environment  -Recurse -ErrorAction SilentlyContinue

    if( -not (Test-Path .\tests\environment ) )
    {
        New-Item -ItemType Directory -Path .\tests\environment
    }
}

function SignAndVerify( [string] $folderName ) {

    .\sign\bin\Release\sign.exe -v --key="$(pwd)\tests\environment\key.snk" --dirs=".\tests\environment\" | Out-Null
    $errorCode = $LASTEXITCODE

    It "Error Code should be 0 " {
        $errorCode | Should Be 0
    }
    
}

function RunAndVerify( [string] $path ) {
    $result = &$path

    It "signed file runs correctly " {
        $result | Should Be "OK"
    }
}


Describe "Sign" {
  Context "When sign files" {

    CleanEnvironmnet
    Copy-Item .\tests\resources\Async\* .\tests\environment\
    Copy-Item .\tests\resources\key.snk .\tests\environment\

    SignAndVerify -folderName .\tests\environment\

    RunAndVerify .\tests\environment\SignTest.exe
  }
}

Describe "Attributes with type as contructor argument" {
    Context "Nested classes" {
        CleanEnvironmnet
        Copy-Item .\tests\resources\NestedClasses\MainProgram\bin\Release\* .\tests\environment\
        Copy-Item .\tests\resources\key.snk .\tests\environment\

        SignAndVerify -folderName .\tests\environment\
    
        RunAndVerify .\tests\environment\MainProgram.exe
  }

    Context "Attribute with type that is already signed" {
        CleanEnvironmnet
        Copy-Item .\tests\resources\TypeFromSignAssembly\MainProgram\bin\Release\* .\tests\environment\
        Copy-Item .\tests\resources\key.snk .\tests\environment\

        SignAndVerify -folderName .\tests\environment\
    
        RunAndVerify .\tests\environment\MainProgram.exe
  }
}

Describe "InternalsVisibleTo" {
    Context "InternalsVisibleTo attribute should be updated" {
        CleanEnvironmnet
        Copy-Item .\tests\resources\InternalVisibleTo\MainProgram\bin\Release\* .\tests\environment\
        Copy-Item .\tests\resources\key.snk .\tests\environment\

        SignAndVerify -folderName .\tests\environment\
    
        RunAndVerify .\tests\environment\MainProgram.exe
    }
}
