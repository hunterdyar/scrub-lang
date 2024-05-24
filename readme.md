# The Scrub Language

Scrub is a language and runtime environment designed for education.

The goal of the language is to create an investigatable playground for students to learn see programming languages work in a hands on way.

The lanuage is simple but competent, with a permissive syntax. It's main special feature is that every operation can be reversed. "Scrub" like scrubbing a timeline.

Instead of throwing errors, the language contains a 'notice+pause' feature, which is similar to an automatic breakpoint. When you hit one, you can start stepping backwards and see how the program got into the unexpected state.

Instead of 'print' statements littering code, debugging with Scrub should leverage the 'Pause()' function instead. 

When paused, the state of the program is completely viewable - the stack, the instruction set, and - of course - memory.

## The Scrub Language
Scrub is an dynamically typed scripting language with first-class functions where everything is an expression.

It is generous in it's syntax - instead of blocking users from making mistakes, it aims to be a viewport into the execution of the program.

- Variables are declared implicitly.
- Functions are first-class. They can be passed around like any other variable.
- Semicolons are optional. Newlines and semicolons both get parsed as a break in the expression.
- Return statements are optional. An expression block ({}'s) will return whatever the last expression evaluated to.
- Indexing non-arrays will default to giving you a types underlying bits as booleans, or bytes (UTf8 values) for a string.

The program "print("Hello, World")" will return "Hello World \nNull". This is correct! It's because everything is an expression. Print is an expression that writes it's arguments to the console, and then returns Null.

### Unique Syntax
- '+' is sum, and '++' is either integer increment (i++) or the 'Concatenate' operator: "a ++ b" will return the string "ab". This saves us from needing a lot of casting
  + Which is good. scrub does not currently support casting. 

## The Concept
In Scrub, every operation can be un-done. That's the key to all of the special sauce.
- For deterministic operations, this is completed with the "UnStack", a stack that - simply - pop's the stacks pushes and pushes the stack's pops. Undoing an op is the same as executing it on the un-stack.
- For non-deterministic operations, this is done by storing reference to state on the un-stack, and restoring that state.

## Workings/Inspiration
- The core of the project is inspired by the concept of Time-Travel Debugging. Partiicularly, a [talk](https://us.pycon.org/2024/schedule/presentation/166/) by [Toby Ho](https://tobyho.com/video/Time-Travel-Debugging-(in-Python).html).
  - *Observation 1: This would be easier to implement if you were working with a fully custom simple language instead of a fork of a complex one.*
  - *Observation 2: Code running to a failure, then a student going 'huh', and simply backing up and observing, would be an excellent learning moment.*
- The Lexer is a simple FSM with 0 lookahead.
- The Parser is based on Bob Nystrom's Blog Post '[Pratt Parsers: Expression Parsing Made Easy](https://journal.stuffwithstuff.com/2011/03/19/pratt-parsers-expression-parsing-made-easy/)'. [jfcardinal](https://github.com/jfcardinal/BantamCs) already wrote a C# port of [bantam](https://github.com/munificent/bantam), and it's fair to say that I copied their homework.
- The Compiler and Virtual Machine are based on the 'Monkey' language from [Thorsten Ball](thorstenball.com)'s boo, "*Writing A Compiler in Go*".
  - *Much of the tasks left to do are to refactor the code to be more C# idiomatic, but following closely while starting was helpful to not get lost in translation.*
  - I chose to implement a VM instead of a tree-walker, because bugs and gotchas often come from the differences between a mental model, the AST, and the linear reality of the execution.
  - I also had the idea of an 'unstack' for rewinding. The name was too good. I had to do it.
- I really wanted to implement a type-less language, but it wouldn't be as useful for students to learn from. That said, permissive operators, and operating on the underlying data of objects, are important values of the scrub language.