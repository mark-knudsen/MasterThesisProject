; ModuleID = 'repl_module'
source_filename = "repl_module"

@fmt_float = private unnamed_addr constant [4 x i8] c"%f\0A\00", align 1

define ptr @main_2() {
entry:
  %printcall = call i32 (ptr, ptr, ...) @printf(ptr @fmt_float, double -1.000000e+00)
  %float_mem = call ptr @malloc(i64 8)
  store double -1.000000e+00, ptr %float_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 2, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %float_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
