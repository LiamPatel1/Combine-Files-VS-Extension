using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic; 
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
// Ensure this using directive is present for StringArrayEditor:
using System.Windows.Forms.Design; // For StringArrayEditor
using System.Drawing.Design; // For UITypeEditor, MultilineStringEditor
using System.ComponentModel.Design; 
using System.Drawing.Design;

namespace CombineFilesVSExtension
{
    public class OptionsPageGrid : DialogPage
    {

        private string _outputHeader = "";
        private string _outputFooter = "";

        private string _outputTemplate = String.Join(
            Environment.NewLine,
            "{{relative_filepath}}:",
            "`````{{type}}",
            "{{text}}",
            "`````",
            "");

        private List<string> _outputFilesPriority = new List<string> {
            "readme*", "*cmake*", "*.csproj", "AssemblyInfo*", "*manifest*", "main.*"
        };
        private List<string> _outputExcludedFiles = new List<string> { "license*", ".git*" };

        private Dictionary<string, string> _TypeMatching = new Dictionary<string, string>()
        {
            { "*.feature", "Cucumber"},{ "*.abap", "abap"},{ "*.adb", "ada"},{ "*.ads", "ada"},{ "*.ada", "ada"},
            { "*.ahk", "ahk"},{ "*.ahkl", "ahk"}, { ".htaccess", "apacheconf"},{ "apache.conf", "apacheconf"},{ "apache2.conf", "apacheconf"},
            { "*.applescript", "applescript"},{ "*.as", "as3"},{ "*.asy", "asy"},{ "*.sh", "bash"},{ "*.ksh", "bash"},{ "*.bash", "bash"},
            { "*.ebuild", "bash"},{ "*.eclass", "bash"}, { "*.bat", "bat"},{ "*.cmd", "bat"},{ "*.befunge", "befunge"}, { "*.bmx", "blitzmax"}, 
            { "*.boo", "boo"}, { "*.bf", "brainfuck"}, { "*.b", "brainfuck"}, { "*.c", "c"}, { "*.h", "cpp"}, { "*.cfml", "cfm"},{ "*.cfc", "cfm"},
            { "*.tmpl", "cheetah"},{ "*.spt", "cheetah"},{ "*.cl", "cl"},{ "*.lisp", "cl"},{ "*.el", "cl"},{ "*.clj", "clojure"},{ "*.cljs", "clojure"},
            { "*.cmake", "cmake"},{ "CMakeLists.txt", "cmake"},{ "*.coffee", "coffeescript"},{ "*.sh-session", "console"},{ "control", "control"},{ "*.cpp", "cpp"},
            { "*.hpp", "cpp"},{ "*.c++", "cpp"},{ "*.h++", "cpp"},{ "*.cc", "cpp"},{ "*.hh", "cpp"}, { "*.cxx", "cpp"},{ "*.hxx", "cpp"},
            { "*.pde", "cpp"},{ "*.cs", "csharp"},{ "*.css", "css"}, { "*.pyx", "cython"},{ "*.pxd", "cython"},{ "*.pxi", "cython"},{ "*.d", "d"},{ "*.di", "d"},
            { "*.pas", "delphi"},{ "*.diff", "diff"},{ "*.patch", "diff"},{ "*.dpatch", "dpatch"},{ "*.darcspatch", "dpatch"},{ "*.duel", "duel"},
            { "*.jbst", "duel"},{ "*.dylan", "dylan"},{ "*.dyl", "dylan"},{ "*.erb", "erb"},{ "*.erl-sh", "erl"},{ "*.erl", "erlang"},{ "*.hrl", "erlang"},
            { "*.evoque", "evoque"},{ "*.factor", "factor"},{ "*.flx", "felix"},{ "*.flxh", "felix"},{ "*.f", "fortran"},{ "*.f90", "fortran"},{ "*.s", "gas"},
            { "*.S", "gas"},{ "*.kid", "genshi"},{ "*.vert", "glsl"},{ "*.frag", "glsl"},{ "*.geo", "glsl"},{ "*.plot", "gnuplot"},{ "*.plt", "gnuplot"},{ "*.go", "go"},
            { "*.1", "groff"},{ "*.2", "groff"},{ "*.3", "groff"},{ "*.4", "groff"},{ "*.5", "groff"},{ "*.6", "groff"},{ "*.7", "groff"},{ "*.8", "groff"},{ "*.n", "groff"},
            { "*.man", "groff"},{ "*.haml", "haml"},{ "*.hs", "haskell"},{ "*.html", "html"},{ "*.htm", "html"},{ "*.xhtml", "html"},{ "*.hx", "hx"},{ "*.hy", "hybris"},
            { "*.hyb", "hybris"},{ "*.ini", "ini"},{ "*.cfg", "ini"},{ "*.io", "io"},{ "*.ik", "ioke"},{ "*.weechatlog", "irc"},{ "*.jade", "jade"},{ "*.java", "java"},
            { "*.js", "js"},{ "*.jsp", "jsp"},{ "*.lhs", "lhs"},{ "*.ll", "llvm"},{ "*.lgt", "logtalk"},{ "*.lua", "lua"},{ "*.wlua", "lua"},{ "*.mak", "make"},{ "Makefile", "make"},
            { "makefile", "make"},{ "Makefile.*", "make"},{ "GNUmakefile", "make"},{ "*.mao", "mako"},{ "*.maql", "maql"},{ "*.mhtml", "mason"},{ "*.mc", "mason"},{ "*.mi", "mason"},
            { "autohandler", "mason"},{ "dhandler", "mason"},{ "*.md", "markdown"},{ "*.mo", "modelica"},{ "*.def", "modula2"},{ "*.mod", "modula2"},{ "*.moo", "moocode"},
            { "*.mu", "mupad"},{ "*.mxml", "mxml"},{ "*.myt", "myghty"},{ "autodelegate", "myghty"},{ "*.asm", "nasm"},{ "*.ASM", "nasm"},{ "*.ns2", "newspeak"},{ "*.objdump", "objdump"},
            { "*.m", "objectivec"},{ "*.j", "objectivej"},{ "*.ml", "ocaml"},{ "*.mli", "ocaml"},{ "*.mll", "ocaml"},{ "*.mly", "ocaml"},{ "*.ooc", "ooc"},{ "*.pl", "perl"},
            { "*.pm", "perl"},{ "*.php", "php"},{ "*.php3", "php"},{ "*.phtml", "php"},{ "*.ps", "postscript"},{ "*.eps", "postscript"},{ "*.pot", "pot"},{ "*.po", "pot"},
            { "*.pov", "pov"},{ "*.inc", "pov"},{ "*.prolog", "prolog"},{ "*.pro", "prolog"},{ "*.properties", "properties"},{ "*.proto", "protobuf"},{ "*.py3tb", "py3tb"},{ "*.pytb", "pytb"},
            { "*.py", "python"},{ "*.pyw", "python"},{ "*.sc", "python"},{ "SConstruct", "python"},{ "SConscript", "python"},{ "*.tac", "python"},{ "*.rb", "rb"},{ "*.rbw", "rb"},
            { "Rakefile", "rb"},{ "*.rake", "rb"},{ "*.gemspec", "rb"},{ "*.rbx", "rb"},{ "*.duby", "rb"},{ "*.Rout", "rconsole"},{ "*.r", "rebol"},{ "*.r3", "rebol"},
            { "*.cw", "redcode"},{ "*.rhtml", "rhtml"},{ "*.rst", "rst"},{ "*.rest", "rst"},{ "*.sass", "sass"},{ "*.scala", "scala"},{ "*.scaml", "scaml"},{ "*.scm", "scheme"},
            { "*.scss", "scss"},{ "*.st", "smalltalk"},{ "*.tpl", "smarty"},{ "sources.list", "sourceslist"},{ "*.R", "splus"},{ "*.sql", "sql"},{ "*.sqlite3-console", "sqlite3"},
            { "squid.conf", "squidconf"},{ "*.ssp", "ssp"},{ "*.tcl", "tcl"},{ "*.tcsh", "tcsh"},{ "*.csh", "tcsh"},{ "*.tex", "tex"},{ "*.aux", "tex"},{ "*.toc", "tex"},
            { "*.txt", "text"},{ "*.v", "v"},{ "*.sv", "v"},{ "*.vala", "vala"},{ "*.vapi", "vala"},{ "*.vb", "vbnet"},{ "*.bas", "vbnet"},{ "*.vm", "velocity"},{ "*.fhtml", "velocity"},
            { "*.vim", "vim"},{ ".vimrc", "vim"},{ "*.xml", "xml"},{ "*.rss", "xml"},{ "*.xsd", "xml"},{ "*.wsdl", "xml"},{ "*.xqy", "xquery"},{ "*.xquery", "xquery"},{ "*.xsl", "xslt"},
            { "*.xslt", "xslt"},{ "*.yaml", "yaml"},{ "*.yml", "yaml"},{ "*.vsct", "xml"},{ "*.csproj", "xml"},{ "*.vsixmanifest", "xml"}
        };



        [Category("Combine Files Settings")]
        [DisplayName("Output Header")]
        [Description("This will be prepended to the output. No macros supported.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string OutputHeader
        {
            get { return _outputHeader; }
            set { _outputHeader = value; }
        }

        [Category("Combine Files Settings")]
        [DisplayName("Output Footer")]
        [Description("This will be appened to the output. No macros supported.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string OutputFooter
        {
            get { return _outputFooter; }
            set { _outputFooter = value; }
        }

        [Category("Combine Files Settings")]
        [DisplayName("Output File Template")]
        [Description("Template for each file selected to be added to the output. Macros available: {{absolute_filepath}}, {{relative_filepath}}, {{filename}}, {{type}}, {{text}}.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string OutputTemplate
        {
            get { return _outputTemplate; }
            set { _outputTemplate = value; }
        }



        [Category("Combine Files Settings")]
        [DisplayName("Priority files")]
        [Description("If these files are in the selection, they will be outputted first. (all else are in order of selection) If multiple prioritised files are found, they are placed in order defined here. Wildcards (*, ?) supported. ")]
        //[TypeConverter(typeof(StringArrayConverter))]

        [TypeConverter(typeof(StringListConverter))] 
        [Editor(typeof(StringListEditor), typeof(UITypeEditor))] 
        public List<string> PriorityFiles
        {
            get { return _outputFilesPriority; }
            set { _outputFilesPriority = value; }
        }



        [Category("Combine Files Settings")]
        [DisplayName("Excluded files")]
        [Description("If these files are in the selection, they will be excluded. Wildcards (*, ?) supported. ")]
        [TypeConverter(typeof(StringListConverter))]
        [Editor(typeof(StringListEditor), typeof(UITypeEditor))] // Apply the custom editor
        //[TypeConverter(typeof(StringArrayConverter))]
        public List<string> ExcludeFiles
        {
            get { return _outputExcludedFiles; }
            set { _outputExcludedFiles = value; }
        }

        [Category("Combine Files Settings")]
        [DisplayName("Type macro matching")]
        [Description("Defines the {{type}} macro (e.g., '*.cs' -> 'csharp'). Useful for markdown code blocks ")]
        [TypeConverter(typeof(StringDictionaryConverter))]
        [Editor(typeof(StringDictionaryEditor), typeof(UITypeEditor))]
        public Dictionary<string, string> TypeMatching
        {
            get { return _TypeMatching; }
            set { _TypeMatching = value; }
        }
    }
}