; ModuleID = 'repl_module'
source_filename = "repl_module"

@true_str = private unnamed_addr constant [6 x i8] c"true\0A\00", align 1
@false_str = private unnamed_addr constant [7 x i8] c"false\0A\00", align 1
@x = global double 0.000000e+00, align 8
@fmt = private unnamed_addr constant [4 x i8] c"%f\0A\00", align 1

define i32 @main_2() {
entry:
  store double 5.000000e+00, ptr @x, align 8
  %x = load double, ptr @x, align 8
  %0 = call i32 (ptr, ...) @printf(ptr @fmt, double %x)
  ret i32 0
}

declare i32 @printf(ptr, ...)
