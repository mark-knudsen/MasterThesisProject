; ModuleID = 'repl_module'
source_filename = "repl_module"

@true_str = private unnamed_addr constant [6 x i8] c"true\0A\00", align 1
@false_str = private unnamed_addr constant [7 x i8] c"false\0A\00", align 1
@str = private unnamed_addr constant [6 x i8] c"harry\00", align 1
@str.1 = private unnamed_addr constant [7 x i8] c"potter\00", align 1
@concat_fmt = private unnamed_addr constant [5 x i8] c"%s%s\00", align 1
@fmt_str = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

define i32 @main() {
entry:
  %concat_buf = call ptr @malloc(i64 256)
  %sprintf_call = call i32 (ptr, ptr, ...) @sprintf(ptr %concat_buf, ptr @concat_fmt, ptr @str, ptr @str.1)
  %printcall = call i32 (ptr, ...) @printf(ptr @fmt_str, ptr %concat_buf)
  ret i32 0
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare i32 @sprintf(ptr, ptr, ...)
