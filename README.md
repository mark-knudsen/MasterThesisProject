# \ourLanguage Compiler

A compiler for **\ourLanguage**, a programming language inspired by Python, SQL, and C#. The project uses LLVM for code generation and targets x86 architectures on both Windows and Linux.

The primary goal of the project was to explore compiler construction, language design, and performance characteristics of dataframe operations. The language combines imperative and declarative programming concepts, allowing users to work with dataframes using familiar SQL-style operations while retaining the flexibility of a general-purpose programming language.

## Features

* LLVM-based code generation
* Python-inspired syntax
* SQL-style dataframe operations such as `select`, `where`, and `map`
* Row-oriented dataframe implementation
* Object-oriented concepts through dataframes and records
* Static type checking
* Error reporting during parsing, type checking, and code generation
* Support for Windows and Linux on x86 architectures

## Dataframe Design

The compiler implements dataframes using a row-oriented storage model. Each dataframe consists of an array of pointers, where each pointer references a row. This design simplifies operations such as sorting, insertion, and deletion because only pointers need to be moved rather than entire rows.

While this approach is flexible, it is not optimized for vectorized operations. Column-oriented storage would likely provide better performance for analytical workloads and operations that process large amounts of data in a single column.

## Current Limitations

The language and compiler are still experimental and several features remain unimplemented:

* User-defined functions
* Local and global variables
* Scope management and symbol tables for reusable declarations
* Garbage collection and automatic memory management
* Advanced runtime error handling
* Compiler optimizations and AST transformations
* Column-oriented dataframe storage

Memory allocated by the generated programs is currently not automatically released, meaning long-running programs may eventually exhaust available memory.

## Performance

Several performance benchmarks were conducted to compare \ourLanguage with Python and R. The results indicate that the language performs competitively for a number of dataframe operations, particularly when vectorization is not involved.

The project currently lacks advanced compiler optimizations, and future work could include AST simplification, operation fusion, and improved dataframe layouts to further increase performance.

## Future Work

Potential improvements include:

* Implementation of user-defined functions and variables
* Addition of lexical scoping and symbol tables
* Garbage collection or automated memory management
* Column-oriented dataframe implementation
* AST optimization passes
* Improved runtime safety checks
* Expanded test coverage and automated unit testing
* Refactoring and cleanup of the compiler codebase
* Additional benchmarking and performance analysis

## Project Structure

The compiler consists of several stages:

1. **Lexical Analysis** – Converts source code into tokens.
2. **Parsing** – Builds an Abstract Syntax Tree (AST).
3. **Type Checking** – Verifies type correctness and semantic validity.
4. **Code Generation** – Produces LLVM IR.
5. **Compilation** – LLVM generates machine code for the target platform.

Each stage is designed to provide meaningful error messages to assist users in diagnosing and correcting issues in their programs.

## Purpose

This project was developed as part of a study of compiler construction and programming language implementation. It serves both as a learning project and as an exploration of how LLVM can be used to create efficient compiled languages with dataframe-oriented functionality.
