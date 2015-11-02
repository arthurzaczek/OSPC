Open Software Plagiarism Checker
================================

A open source software plagiarism checker. It checks the similarity of individual files and generates various types of reports. Also it is able to find groups of similar submissions.

Features
--------

* Supports (currently) any C like language like C, C++, Java or C#. Extendable to any other language.
* Configurable: Adapt similarity levels to each kind of submissions. It makes a difference, if it's a basic programming course or a advanced programming course
* Html Reports: The most powerful report is the Html Report.
* FriendFinder: The program is able to find groups of similar submissions.

Usage
-----

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
Usage: OSPC [options] { file1 file2 ... }

  -h, -?, --help             Prints this help
  -f=VALUE                   File filter. If -d is specified, then -f
                               defaults to "*.*."
  -d=VALUE                   Specifies a directory where the filer applies.
                               If -f is specified, then -d defaults to "."
  --include=VALUE            Specifies a regular expression that every file
                               must match. More than one expression is allowed.
                               A file must match any of these expressions.
  --exclude=VALUE            Specifies a regular expression to exclude files.
                               More than one expression is allowed. If a file
                               must match any of these expressions it will be
                               excluded.
  --detailed                 Print a detailed report to the console
  --summary                  Print only a summay to the console. Usefull if --
                               html is used.
  --html[=VALUE]             Saves a html report to the specified directory.
                               Defaults to "report"
  --min-match-length=VALUE   Minimum count of matching tokens, including non-
                               matching tokens.
  --max-match-distance=VALUE Maximum distance between tokens to count as a
                               match. 1 = exact match.
  --min-common-token=VALUE   Percent of token that must match to count as a
                               match. 1 = every token must match.
  -v, --verbose              Verbose output.
  file1 file2                Optional. Files or additional files, if -f or -d
                               is not used or not applicalable.

Examples:

  OSPC -d c:\somedir -f *.c

    Checks all *.c files in somedir.

  OSPC c:\somedir\file1.c c:\somedir\file2.c

    Checks file1.c and file2.c using absolute paths.

  OSPC a.c b.c

    Checks file1.c and file2.c using relative paths.

  OSPC --summay --html -f *.c

    Checks all c-files in the current directory and output a html report to .\report\index.html.
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

How it works
------------

Each file is split up into tokens and symbols by a Tokenizer. Comments and whitespaces are ignored.

### Example:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if(a < b)
{
    /* Yes! */
    printf("Yes");
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The result would be:

--------- | --------------------------------
if		  | Token
(		  | Symbol
a		  | Token
<		  | Symbol
b		  | Token
)		  | Symbol
{		  | Symbol
printf	  | Token
(		  | Symbol
"Yes"	  | Token (as quoted string)
)		  | Symbol
;		  | Symbol
}		  | Symbol
--------- | --------------------------------

