; ModuleID = 'repl_module'
source_filename = "repl_module"

@true_str = private unnamed_addr constant [6 x i8] c"true\0A\00", align 1
@false_str = private unnamed_addr constant [7 x i8] c"false\0A\00", align 1

define i32 @main() {
entry:
  %print_bool = call i32 (ptr, ...) @printf(ptr @true_str)
  ret i32 0
}

declare i32 @printf(ptr, ...)

declare i32 @printf.1(ptr, ...)
