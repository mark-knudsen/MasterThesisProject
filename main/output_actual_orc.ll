; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@fmt_array = private unnamed_addr constant [16 x i8] c"Array(len=%ld)\0A\00", align 1

define ptr @main_1() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %arr_len = load i64, ptr %x_load, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_array, i64 %arr_len)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
