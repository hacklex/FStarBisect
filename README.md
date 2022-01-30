# FStarBisect
Bisects the input F* module to pinpoint the parser error in parser's stead :)

## Description
This little tool bisects your F* module and tries to pinpoint the parse error by feeding parts of your module to FStar.exe until it starts spitting out a parse_error.
Useful whenever FStar does not report the error location. 

## Usage
Just run `FStarBisect %Your.FStar.Module.fst%` in the console.

I advise adding FStarBisect binary directory to your PATH as well :)

## Prerequisites
* An installation of FStar accessible via PATH
