# Combine Files VS2022 Extension

Allows you to combine the contents of files together and output the result. To do this, select the desired files and right click, press "Combine Files". The output will be in the "Output" pane under "Combine Files Output".  Works for solutions and folder views.

## Templates
   
Lets you customise a template with macros to determine how files should be added together, for example:

```
This file is called: {{relative_filepath}}
And its contents are: {{text}}
```

This block would be output for each file selected, with the macros replaced. Available macros are {{absolute_filepath}}, {{filename}}, {{relative_filepath}}, {{type}} and {{text}}.

## Priority files

By default, the files are (usually, possibly not) output in the order they are selected. Certain files can be prioritised. For example:
```
- readme.*
- *.csproj
- *cargo*
- *cmake*
- main.*
```

You can fill it up with all sorts of files, even if you don't think all of them would ever be used at once. The above list might be set for a developer that switches between C#, C++ and Rust. The readme file will always be output first, then whatever build system files are found, then the main. The other files will follow in order of selection.

If you have an entry with no path seperator, then the entry will match no matter what directory it is in. If it has any path specifier at all, it will assume it to be relative to the working directory.
For example: If you add `db.*` to the priority list, then both `[PROJECT]/db.cpp` and `[PROJECT]/src/db.cpp` will be matched. If you add `db/db.*cpp*` to the priority list, then `[PROJECT]/db.cpp` will NOT be matched, `[PROJECT]/db/db.cpp` WILL be matched, but `[PROJECT]/src/db/db.cpp` will NOT be matched.

## Excluded files

You can set files to be always excluded. For example:
```
- .gitignore
- .gitattributes
- LICENSE*
```
This has the similar path matching to priority files. See above for more information.


## Header and Footer

You can define a header and footer which will be added once each to the output, no matter how many files are selected.


## Type matching

This extension will detect file type based on file name. and associate it with the macro {{type}}. For example:
```
- *.cs -> csharp
- *.py -> python
- *.cpp -> cpp
- *.csproj -> xml
- cmakelists.txt -> cmake
```

The main purpose of this is creating markdown with syntax highlighting. So with this user defined macro you can set your template to be:
````
**{{relative_filepath}}**:
```{{type}}
{{text}}
```

````
Which upon template resolution becomes:
````
**src/main.py**:
```python
print("Hello World!")
```

**project.csproj**:
```xml
<Project>
   Lotsa xml here
</Project>
```

````
A markdown decoder will make that look very pretty.
