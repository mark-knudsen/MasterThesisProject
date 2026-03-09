; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [6 x i8] c"harry\00", align 1
@fmt_str = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@fmt_int = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_3() {
entry:
  %printcall = call i32 (ptr, ...) @printf(ptr @fmt_str, ptr @str)
  %printcall1 = call i32 (ptr, ptr, ...) @printf(ptr @fmt_int, i32 %printcall)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
