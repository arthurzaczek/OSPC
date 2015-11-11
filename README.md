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

> Note! When you run the program the first time without a configuration or adapted set of Comparer settings you will only find exact matches that are longer than 1000 Tokens. Each programming course is different. You have to find the right values for your course.

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
Usage: OSPC [options] { file1 file2 ... }

  -h, -?, --help             Prints this help
  -c=VALUE                   Reads the given configuration. Note, this switch
                               should be the first argument as it overrides any
                               other argument parsed yet.
  --write-config=VALUE       Write the current configuration to the given
                               file. Note, this switch should be the last
                               argument.
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

    Checks all *.c files in somedir with the default settings.

  OSPC c:\somedir\file1.c c:\somedir\file2.c

    Checks file1.c and file2.c using absolute paths with the default settings.

  OSPC a.c b.c

    Checks file1.c and file2.c using relative paths with the default settings.

  OSPC -c basic_profile.xml --summay --html -f *.c

    Checks all c - files in the current directory and output a html report to.\report\index.html.

   OSPC --write-config default.xml

    Writes the default configuration to default.xml

  OSPC --min-match-length=100 --max-match-distance=2 --min-common-token=0.95 --write-config basic.xml

    Writes the current configuration to basic.xml

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


Then, each file is compared with all other files. Each Token is compared with all other token. Finally, the longest match are selected as the result match.

What is a match? A match is the longest chain of equal tokens, with some exceptions. 

1. Every {max-match-distance} token must match
2. {min-common-token} % of token must match.

### Example:

**File A**

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if(a < b)
{
    /* Yes! */
    printf("Yes");
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

**File B**

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if(x < y)
{
    // True
    printf("True");
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

A		  | B		  | Match
--------- | --------- | ---------
if		  | if		  | Yes
(		  | (		  | Yes
a		  | x		  | **No**
<		  | <		  | Yes
b		  | y		  | **No**
)		  | )		  | Yes
{		  | {		  | Yes
printf	  | printf	  | Yes
(		  | (		  | Yes
"Yes"	  | "True"	  | **No**
)		  | )		  | Yes
;		  | ;		  | Yes
}		  | }		  | Yes
**13**	  | **13**	  | **3**

10 Token of 13 are the same, resulting in a 76.92 % similarity. It depends on your individual progarmming course, if this match count's as equal or not.

