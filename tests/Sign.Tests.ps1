Describe "Sign" {
  Context "When sign files" {
    Copy-Item .\tests\resources\SignTest.exe .\tests\
    Copy-Item .\tests\resources\SignTestLibrary.dll .\tests\
    Copy-Item .\tests\resources\key.snk .\tests\

    .\sign\bin\Release\sign.exe -v --key="$(pwd)\tests\key.snk" --dirs=".\tests\"
    $errorCode = $LASTEXITCODE

    $result = .\tests\SignTest.exe

    It "Error Code should be 0 " {
        $errorCode | Should Be 0
    }
    It "signed file runs correctly " {
        $result | Should Be "OK"
    }
    }
 }