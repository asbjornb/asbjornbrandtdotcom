# Python

Check out this cheatsheet: [python cheatsheet](https://gto76.github.io/python-cheatsheet/#pandas)

## Virtual environments

To activate an existing virtual environment (venv) use the activate.bat. If project is at c:\someproject and has venv folder at c:\someproject\someVenv then usually call:
`someVenv\scripts\activate.bat`

`pip list` lists the actual packages in the venv

``` bat
// navigate to project folder  
cd c:\someproject
  
// create a new directory 'project-36'  
mkdir project-36  
  
// change currenct directory to 'project-36'  
cd .\project-36  
  
// create a virtual environment named 'venv', feel free to name it anything you like. -p Location is optional but makes sure to use a specific python version.
virtualenv venv -p C:\Python\Python36\python.exe  

// activate the virtual environment
.\venv\Scripts\activate

// check the python version
python --version

// list all packages installed by default
pip list

// deactivate the virtual environment
deactivate
```

See also pipx for managing global packages that are not project-bound.

## Taking arguments

Argparse has all the functionality one could want. See:
[docs.python.org](https://docs.python.org/3/howto/argparse.html)

## Stopping execution

On windows (and sometimes linux) cancelling python in the terminal can be tricky since io operations can ignore ctrl+c and similar commands. ctrl+break seems to work though and on thinkpad laptops is done with ctrl+fn+b due to the lacking break button.

## Project templates

Cookiecutter allows setting up new projects from templates.

* [Cookiecutter data science template](https://github.com/drivendata/cookiecutter-data-science)

## Notes on useful language elements, functions etc.

List comprehension (with condition):

````python3
fruits = ["apple", "banana", "cherry", "kiwi", "mango"]
newlist = [x for x in fruits if "a" in x]
````

Dict comprehensions and enumeration:

````python3
fruits = ["apple", "banana", "cherry", "kiwi", "mango"]
newDict = {fruit: index for index, fruit in enumerate(fruits)]
````

Sort with key:

````python3
numLetters = [(1,b),(2,a),(3,d)]
sortedByFirst = numLetters.sort(key=itemgetter(0))
sortedByFirstThenSecond = numLetters.sort(key=itemgetter(0,1))
reverseSortedByFirst = numLetters.sort(key=itemgetter(0), reverse=True)
````
